#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.EditorIconsUI
{
    public class EditorIconsUIToolkit : EditorWindow
    {
        private const long SearchDebounceMs = 200;

        private static readonly List<IconCacheData> _iconCache = new();
        private static readonly List<IconCacheData> _bigIcons = new();
        private static readonly List<IconCacheData> _smallIcons = new();
        private static readonly Dictionary<string, List<IconCacheData>> _categoryToBigIcons = new();
        private static readonly Dictionary<string, List<IconCacheData>> _categoryToSmallIcons = new();
        private static bool _needsCleanup;

        private readonly List<string> _categories = new();
        private readonly Dictionary<string, string> _categoryMapping = new();
        private readonly List<IconUI> _uiElements = new();

        private bool _darkPreview = true;
        private bool _viewBigIcons = true;
        private string _searchQuery = string.Empty;
        private string _selectedCategory = "All";

        private ScrollView _gridContainer = null!;
        private DropdownField _categoryDropdown = null!;
        private TextField _iconCodeField = null!;
        private VisualElement _previewBackground = null!;
        private Label _previewDetails = null!;
        private Image _previewImage = null!;
        private VisualElement _previewPanel = null!;
        private IconCacheData? _selectedIcon;
        private IVisualElementScheduledItem? _searchDebounce;
        private bool? _lastFilterBig;

        [MenuItem("Tools/Editor Icons UI Toolkit %e", priority = -1001)]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorIconsUIToolkit>("Editor Icons");
            window.minSize = new Vector2(400, 450);
            window.Show();
        }

        public void CreateGUI()
        {
            DiscoverIcons();
            BuildUI();
            UpdateCategoryDropdownChoices();
            UpdateFilter();
        }

        private void OnDestroy()
        {
            if (!_needsCleanup) return;
            _needsCleanup = false;
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        #region Initialization & Caching

        private class IconCacheData
        {
            public string Name { get; }
            public string NameLower { get; }
            public string Category { get; }
            public Texture2D Texture { get; }
            public bool IsBig { get; }

            public IconCacheData(string name, Texture2D texture, bool isBig)
            {
                Name = name;
                NameLower = name.ToLowerInvariant();
                Category = ExtractCategory(name);
                Texture = texture;
                IsBig = isBig;
            }

            private static string ExtractCategory(string name)
            {
                var slashIdx = name.IndexOf('/');
                if (slashIdx >= 0) return name[..slashIdx];

                var dotIdx = name.IndexOf('.');
                if (dotIdx >= 0) return name[..dotIdx];

                return name switch
                {
                    _ when name.StartsWith("sv_") => "SceneView",
                    _ when name.StartsWith("Pre") => "Preview",
                    _ when name.StartsWith("WaitSpin") => "WaitSpin",
                    _ => "General"
                };
            }
        }

        private class IconUI
        {
            public IconCacheData Data { get; }
            public Button Element { get; }

            public IconUI(IconCacheData data, Button element)
            {
                Data = data;
                Element = element;
            }
        }

        private void DiscoverIcons()
        {
            if (_iconCache.Count > 0) return;

            var allNames = CollectAllIconNames();
            if (allNames.Count == 0) return;

            _iconCache.Capacity = allNames.Count;

            foreach (var iconName in allNames)
            {
                if (TryLoadIcon(iconName) is { } icon)
                    _iconCache.Add(icon);
            }

            RebuildLookups();
            _needsCleanup = true;
        }

        private static HashSet<string> CollectAllIconNames()
        {
            var icoList = EditorIconsList.IcoList;
            if (icoList is not { Length: > 0 })
            {
                Debug.LogWarning("No icons found in EditorIconsList. Make sure the IcoList is populated.");
                return new HashSet<string>();
            }

            var allNames = new HashSet<string>(icoList);

            foreach (var t in Resources.FindObjectsOfTypeAll<Texture2D>())
            {
                if (!string.IsNullOrEmpty(t.name))
                    allNames.Add(t.name);
            }

            return allNames;
        }

        private static IconCacheData? TryLoadIcon(string iconName)
        {
            Debug.unityLogger.logEnabled = false;
            var content = EditorGUIUtility.IconContent(iconName);
            Debug.unityLogger.logEnabled = true;

            if (content?.image is not Texture2D tex) return null;

            var isBig = tex is { width: > 36, height: > 36 };
            return new IconCacheData(iconName, tex, isBig);
        }

        private static void RebuildLookups()
        {
            _bigIcons.Clear();
            _smallIcons.Clear();
            _categoryToBigIcons.Clear();
            _categoryToSmallIcons.Clear();

            foreach (var icon in _iconCache)
            {
                var (sizeList, categoryMap) = icon.IsBig
                    ? (_bigIcons, _categoryToBigIcons)
                    : (_smallIcons, _categoryToSmallIcons);

                sizeList.Add(icon);
                AddToCategory(categoryMap, icon);
            }
        }

        private static void AddToCategory(Dictionary<string, List<IconCacheData>> map, IconCacheData icon)
        {
            if (!map.TryGetValue(icon.Category, out var list))
            {
                list = new List<IconCacheData>();
                map[icon.Category] = list;
            }
            list.Add(icon);
        }

        #endregion

        #region UI Construction

        private void BuildUI()
        {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;

            BuildToolbar(root);

            _gridContainer = new ScrollView { style = { flexGrow = 1, paddingBottom = 10 } };
            _gridContainer.contentContainer.style.flexDirection = FlexDirection.Row;
            _gridContainer.contentContainer.style.flexWrap = Wrap.Wrap;
            _gridContainer.contentContainer.style.justifyContent = Justify.Center;
            root.Add(_gridContainer);

            InitializeGridElements();
            BuildPreviewPanel(root);
        }

        private void BuildToolbar(VisualElement root)
        {
            var toolbar = new Toolbar();

            var btnSaveAll = new ToolbarButton(SaveAllIcons) { text = "Save all..." };
            toolbar.Add(btnSaveAll);
            toolbar.Add(new ToolbarSpacer { style = { flexGrow = 0, width = 10 } });

            var toggleBig = new Toggle("Toggle Big/Small") { value = _viewBigIcons, style = { alignSelf = Align.Center, marginLeft = 5, marginRight = 5 } };
            toggleBig.RegisterValueChangedCallback(evt =>
            {
                _viewBigIcons = evt.newValue;
                UpdateCategoryDropdownChoices();
                UpdateFilter();
            });
            toolbar.Add(toggleBig);

            _categoryDropdown = new DropdownField() { style = { width = 180, marginLeft = 5, alignSelf = Align.Center } };
            _categoryDropdown.RegisterValueChangedCallback(evt =>
            {
                if (_categoryMapping.TryGetValue(evt.newValue, out var cat))
                {
                    _selectedCategory = cat;
                    UpdateFilter();
                }
            });
            toolbar.Add(_categoryDropdown);

            toolbar.Add(new ToolbarSpacer { style = { flexGrow = 1 } });

            var searchField = new ToolbarSearchField { style = { minWidth = 150, flexShrink = 1 } };
            searchField.RegisterValueChangedCallback(evt =>
            {
                _searchQuery = evt.newValue.ToLowerInvariant();

                // Debounce: cancel previous scheduled filter and reschedule after delay
                _searchDebounce?.Pause();
                _searchDebounce = searchField.schedule.Execute(UpdateFilter).StartingIn(SearchDebounceMs);
            });
            toolbar.Add(searchField);

            root.Add(toolbar);
        }

        private void InitializeGridElements()
        {
            _gridContainer.Clear();
            _uiElements.Clear();

            foreach (var data in _iconCache)
            {
                var btn = new Button(() => SetPreview(data))
                {
                    tooltip = data.Name,
                    style =
                    {
                        backgroundImage = data.Texture,
                        paddingTop = 0,
                        paddingBottom = 0,
                        paddingLeft = 0,
                        paddingRight = 0,
                        marginTop = 2,
                        marginBottom = 2,
                        marginLeft = 2,
                        marginRight = 2,
                        backgroundColor = Color.clear,
                        display = DisplayStyle.None
                    }
                };

                _uiElements.Add(new IconUI(data, btn));
                _gridContainer.Add(btn);
            }
        }

        private void BuildPreviewPanel(VisualElement root)
        {
            _previewPanel = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    borderTopWidth = 1,
                    borderTopColor = Color.gray,
                    paddingTop = 10,
                    paddingBottom = 10,
                    paddingLeft = 10,
                    paddingRight = 10,
                    height = 150,
                    display = DisplayStyle.None,
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f)
                }
            };

            var leftCol = new VisualElement { style = { width = 130, marginRight = 15 } };
            _previewBackground = new VisualElement { style = { height = 80, marginBottom = 5, justifyContent = Justify.Center } };
            _previewImage = new Image { scaleMode = ScaleMode.ScaleToFit, style = { width = 128, height = 80 } };
            _previewBackground.Add(_previewImage);

            var themeToggle = new Button(TogglePreviewTheme) { text = "Toggle Light/Dark", style = { height = 20 } };
            leftCol.Add(_previewBackground);
            leftCol.Add(themeToggle);

            var rightCol = new VisualElement { style = { flexGrow = 1, justifyContent = Justify.SpaceBetween } };
            _previewDetails = new Label();
            _iconCodeField = new TextField { isReadOnly = true };

            var btnRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 5 } };
            var btnCopy = new Button(() => EditorGUIUtility.systemCopyBuffer = _selectedIcon?.Name) { text = "Copy Name", style = { flexGrow = 1 } };
            var btnSave = new Button(() =>
            {
                if (_selectedIcon is { } selected) SaveIcon(selected);
            }) { text = "Save to File...", style = { flexGrow = 1 } };

            btnRow.Add(btnCopy);
            btnRow.Add(btnSave);
            rightCol.Add(_previewDetails);
            rightCol.Add(_iconCodeField);
            rightCol.Add(btnRow);

            var closeBtn = new Button(() => SetPreview(null)) { text = "X", style = { width = 30 } };

            _previewPanel.Add(leftCol);
            _previewPanel.Add(rightCol);
            _previewPanel.Add(closeBtn);
            root.Add(_previewPanel);

            UpdatePreviewTheme();
        }

        #endregion

        #region State & Logic

        private void UpdateCategoryDropdownChoices()
        {
            _categories.Clear();
            _categoryMapping.Clear();

            var filteredIcons = _viewBigIcons ? _bigIcons : _smallIcons;
            var categoryMap = _viewBigIcons ? _categoryToBigIcons : _categoryToSmallIcons;

            var allDisplay = $"All ({filteredIcons.Count})";
            AddCategoryChoice(allDisplay, "All");

            foreach (var cat in categoryMap.Keys.Where(k => k != "General").OrderBy(k => k))
                AddCategoryChoice($"{cat} ({categoryMap[cat].Count})", cat);

            if (categoryMap.TryGetValue("General", out var generalList))
                AddCategoryChoice($"General ({generalList.Count})", "General");

            _categoryDropdown.choices = _categories;

            // Reset to "All" if current category has no icons in this mode
            var newDisplay = _categoryMapping.FirstOrDefault(x => x.Value == _selectedCategory).Key
                             ?? allDisplay;

            if (newDisplay == allDisplay)
                _selectedCategory = "All";

            _categoryDropdown.SetValueWithoutNotify(newDisplay);
        }

        private void AddCategoryChoice(string display, string category)
        {
            _categories.Add(display);
            _categoryMapping[display] = category;
        }

        private void UpdateFilter()
        {
            var size = _viewBigIcons ? 70 : 40;
            var sizeChanged = _lastFilterBig != _viewBigIcons;
            _lastFilterBig = _viewBigIcons;

            var visibleSet = BuildVisibleSet();

            foreach (var ui in _uiElements)
            {
                var visible = IsIconVisible(ui.Data, visibleSet);
                ui.Element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

                if (visible && sizeChanged)
                {
                    ui.Element.style.width = size;
                    ui.Element.style.height = size;
                }
            }
        }

        private HashSet<IconCacheData>? BuildVisibleSet()
        {
            if (_selectedCategory == "All") return null;

            var categoryMap = _viewBigIcons ? _categoryToBigIcons : _categoryToSmallIcons;
            return categoryMap.TryGetValue(_selectedCategory, out var catList)
                ? new HashSet<IconCacheData>(catList)
                : null;
        }

        private bool IsIconVisible(IconCacheData data, HashSet<IconCacheData>? visibleSet) =>
            data.IsBig == _viewBigIcons
            && (visibleSet is null || visibleSet.Contains(data))
            && (string.IsNullOrEmpty(_searchQuery) || data.NameLower.Contains(_searchQuery, StringComparison.Ordinal));

        private void SetPreview(IconCacheData? icon)
        {
            _selectedIcon = icon;

            if (icon is null)
            {
                _previewPanel.style.display = DisplayStyle.None;
                return;
            }

            _previewPanel.style.display = DisplayStyle.Flex;
            _previewImage.image = icon.Texture;
            _iconCodeField.value = $"EditorGUIUtility.IconContent(\"{icon.Name}\")";

            _previewDetails.text = $"Name: {icon.Name}\nCategory: {icon.Category}\nSize: {icon.Texture.width}x{icon.Texture.height}";
        }

        private void TogglePreviewTheme()
        {
            _darkPreview = !_darkPreview;
            UpdatePreviewTheme();
        }

        private void UpdatePreviewTheme()
        {
            var col = _darkPreview ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.85f, 0.85f, 0.85f);
            _previewBackground.style.backgroundColor = col;
        }

        #endregion

        #region Save Operations

        private static string GetIconFileName(string iconName)
        {
            var slashIdx = iconName.LastIndexOf('/');
            return slashIdx >= 0 ? iconName[(slashIdx + 1)..] : iconName;
        }

        private void SaveIcon(IconCacheData icon)
        {
            var fileName = GetIconFileName(icon.Name);
            var path = EditorUtility.SaveFilePanel("Save icon", "", fileName, "png");
            if (string.IsNullOrEmpty(path)) return;

            WriteTextureToFile(icon.Texture, path);
        }

        private void SaveAllIcons()
        {
            var folderPath = EditorUtility.SaveFolderPanel("Save All Icons", "", "");
            if (string.IsNullOrWhiteSpace(folderPath)) return;

            var savedCount = 0;
            foreach (var data in _iconCache)
            {
                var path = Path.Combine(folderPath, $"{GetIconFileName(data.Name)}.png");
                if (!File.Exists(path) && WriteTextureToFile(data.Texture, path))
                    savedCount++;
            }

            Debug.Log($"Saved {savedCount} icons to {folderPath}");
        }

        private bool WriteTextureToFile(Texture2D source, string path)
        {
            Texture2D? copy = null;
            try
            {
                copy = new Texture2D(source.width, source.height, source.format, source.mipmapCount, true);
                Graphics.CopyTexture(source, copy);
                File.WriteAllBytes(path, copy.EncodeToPNG());
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save icon to {path}: {e.Message}");
                return false;
            }
            finally
            {
                if (copy is not null) DestroyImmediate(copy);
            }
        }

        #endregion
    }
}
#endif
