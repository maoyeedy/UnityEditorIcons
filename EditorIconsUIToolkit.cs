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

public class EditorIconsUIToolkit : EditorWindow
{
    #region Icon Names

    private static readonly string[] _icoList =
    {
        "ScriptableObject Icon", "_Popup"
    };

    #endregion

    private static readonly List<IconCacheData> _iconCache = new();
    private static readonly List<string> _categories = new() { "All" };

    private readonly List<IconUI> _uiElements = new();

    private bool _darkPreview = true;
    private bool _viewBigIcons = true;
    private string _searchQuery = string.Empty;
    private string _selectedCategory = "All";

    private ScrollView _gridContainer = null!;
    private TextField _iconCodeField = null!;
    private VisualElement _previewBackground = null!;
    private Label _previewDetails = null!;
    private Image _previewImage = null!;
    private VisualElement _previewPanel = null!;
    private IconCacheData? _selectedIcon;

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
        UpdateFilter();
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
            if (name.Contains('/')) return name.Split('/')[0];
            if (name.Contains('.')) return name.Split('.')[0];
            if (name.StartsWith("sv_")) return "SceneView";
            if (name.StartsWith("Pre")) return "Preview";
            if (name.StartsWith("WaitSpin")) return "WaitSpin";
            return "General";
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

        var knownNames = new HashSet<string>(_icoList);

        var discovered = Resources.FindObjectsOfTypeAll<Texture2D>()
            .Select(t => t.name)
            .Where(name => !knownNames.Contains(name));

        var combinedNames = _icoList.Concat(discovered).Distinct();

        foreach (var iconName in combinedNames)
        {
            if (string.IsNullOrEmpty(iconName)) continue;

            Debug.unityLogger.logEnabled = false;
            var content = EditorGUIUtility.IconContent(iconName);
            Debug.unityLogger.logEnabled = true;

            if (content?.image is Texture2D tex)
            {
                var isBig = tex.width > 36 && tex.height > 36;
                _iconCache.Add(new IconCacheData(iconName, tex, isBig));
            }
        }

        // Generate category list
        var uniqueCategories = _iconCache.Select(x => x.Category).Distinct().Where(c => c != "General").OrderBy(c => c).ToList();
        _categories.Clear();
        _categories.Add("All");
        _categories.AddRange(uniqueCategories);
        _categories.Add("General");

        Resources.UnloadUnusedAssets();
        GC.Collect();
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

        var toggleBig = new Toggle("Toggle Big/Small")
        {
            value = _viewBigIcons,
            style = { alignSelf = Align.Center, marginLeft = 5, marginRight = 5 }
        };
        toggleBig.RegisterValueChangedCallback(evt =>
        {
            _viewBigIcons = evt.newValue;
            UpdateFilter();
        });
        toolbar.Add(toggleBig);

        var categoryDropdown = new DropdownField(_categories, 0)
        {
            style = { width = 140, marginLeft = 5, alignSelf = Align.Center }
        };
        categoryDropdown.RegisterValueChangedCallback(evt =>
        {
            _selectedCategory = evt.newValue;
            UpdateFilter();
        });
        toolbar.Add(categoryDropdown);

        toolbar.Add(new ToolbarSpacer { style = { flexGrow = 1 } });

        var searchField = new ToolbarSearchField { style = { minWidth = 150, flexShrink = 1 } };
        searchField.RegisterValueChangedCallback(evt =>
        {
            _searchQuery = evt.newValue.ToLowerInvariant();
            UpdateFilter();
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
                    paddingTop = 0, paddingBottom = 0, paddingLeft = 0, paddingRight = 0,
                    marginTop = 2, marginBottom = 2, marginLeft = 2, marginRight = 2,
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
                borderTopWidth = 1, borderTopColor = Color.gray,
                paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10,
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
            if (_selectedIcon != null) SaveIcon(_selectedIcon);
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

    private void UpdateFilter()
    {
        var size = _viewBigIcons ? 70 : 40;
        var noQuery = string.IsNullOrEmpty(_searchQuery);

        foreach (var ui in _uiElements)
        {
            var match = ui.Data.IsBig == _viewBigIcons &&
                        (_selectedCategory == "All" || ui.Data.Category == _selectedCategory) &&
                        (noQuery || ui.Data.NameLower.Contains(_searchQuery));

            if (match)
            {
                ui.Element.style.display = DisplayStyle.Flex;
                ui.Element.style.width = size;
                ui.Element.style.height = size;
            }
            else
            {
                ui.Element.style.display = DisplayStyle.None;
            }
        }
    }

    private void SetPreview(IconCacheData? icon)
    {
        _selectedIcon = icon;

        if (icon == null)
        {
            _previewPanel.style.display = DisplayStyle.None;
            return;
        }

        _previewPanel.style.display = DisplayStyle.Flex;
        _previewImage.image = icon.Texture;
        _iconCodeField.value = $"EditorGUIUtility.IconContent(\"{icon.Name}\")";

        var isProSkin = icon.Name.StartsWith("d_") ? "Yes" : "No";
        _previewDetails.text = $"Name: {icon.Name}\nCategory: {icon.Category}\nSize: {icon.Texture.width}x{icon.Texture.height}\nIs Pro Skin: {isProSkin}";
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

    private void SaveIcon(IconCacheData icon)
    {
        var path = EditorUtility.SaveFilePanel("Save icon", "", icon.Name.Split('/').Last(), "png");
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
            var fileName = data.Name.Split('/').Last();
            var path = Path.Combine(folderPath, $"{fileName}.png");

            if (!File.Exists(path) && WriteTextureToFile(data.Texture, path)) savedCount++;
        }

        Debug.Log($"Saved {savedCount} icons to {folderPath}");
    }

    private bool WriteTextureToFile(Texture2D source, string path)
    {
        try
        {
            var copy = new Texture2D(source.width, source.height, source.format, source.mipmapCount, true);
            Graphics.CopyTexture(source, copy);
            File.WriteAllBytes(path, copy.EncodeToPNG());
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save icon to {path}: {e.Message}");
            return false;
        }
    }

    #endregion
}
#endif
