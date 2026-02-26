#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.EditorIconsUI
{
    public class EditorIcons : EditorWindow
    {
        private static string[] _icoList = Array.Empty<string>();

        private int _buttonSize = 70;
        private bool _darkPreview = true;

        private List<GUIContent> _filteredIconList = new();
        private GUIStyle? _iconButtonStyle;

        private readonly List<GUIContent> _iconContentListAll = new();
        private readonly List<GUIContent> _iconContentListBig = new();
        private readonly List<GUIContent> _iconContentListSmall = new();
        private GUIContent? _iconSelected;
        private Vector2 _scroll;
        private string _search = string.Empty;

        private bool _viewBigIcons = true;

        private bool IsWide => Screen.width > 550;
        private bool HasSearch => !string.IsNullOrWhiteSpace(_search);

        #region Menu

        [MenuItem("Tools/Editor Icons %e", priority = -1001)]
        public static void EditorIconsOpen()
        {
            var w = CreateWindow<EditorIcons>("Editor Icons");
            w.ShowUtility();
            w.minSize = new Vector2(320, 450);
        }

        #endregion

        #region Lifecycle

        private void OnEnable()
        {
            var knownIcons = new HashSet<string>(EditorIconsList.IcoList.Where(x => GetIcon(x) is not null));

            var discovered = Resources.FindObjectsOfTypeAll<Texture2D>()
                .Select(t => t.name)
                .Where(name => GetIcon(name) is not null && !knownIcons.Contains(name));

            _icoList = _icoList.Concat(discovered).ToArray();

            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        private void OnGUI()
        {
            InitIcons();

            if (!IsWide) DrawSearchBar();
            DrawToolbar();
            if (IsWide) GUILayout.Space(3);
            DrawIconGrid();
            DrawSelectionPreview();
        }

        #endregion

        #region GUI Drawing

        private void DrawToolbar()
        {
            using var _ = new GUILayout.HorizontalScope(EditorStyles.toolbar);

            if (GUILayout.Button("Save all icons to folder...", EditorStyles.toolbarButton))
                SaveAllIcons();

            EditorGUI.BeginChangeCheck();
            if (GUILayout.Toggle(!_viewBigIcons, "Small", EditorStyles.toolbarButton)) _viewBigIcons = false;
            if (GUILayout.Toggle(_viewBigIcons, "Big", EditorStyles.toolbarButton)) _viewBigIcons = true;

            if (EditorGUI.EndChangeCheck()) UpdateFilteredList();

            if (IsWide) DrawSearchBar();
        }

        private void DrawSearchBar()
        {
            using var _ = new GUILayout.HorizontalScope();

            if (IsWide) GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            var newSearch = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);

            GUIStyle cancelStyle = string.IsNullOrEmpty(newSearch)
                ? "ToolbarSearchCancelButtonEmpty"
                : "ToolbarSearchCancelButton";

            if (GUILayout.Button("", cancelStyle))
            {
                newSearch = string.Empty;
                GUI.FocusControl(null);
            }

            if (!EditorGUI.EndChangeCheck()) return;

            _search = newSearch;
            UpdateFilteredList();
        }

        private void UpdateFilteredList()
        {
            if (!HasSearch)
            {
                _filteredIconList = _viewBigIcons ? _iconContentListBig : _iconContentListSmall;
                return;
            }

            var lowerSearch = _search.ToLowerInvariant();
            _filteredIconList = _iconContentListAll
                .Where(x => x.tooltip.ToLowerInvariant().Contains(lowerSearch))
                .ToList();
        }

        private void DrawIconGrid()
        {
            var ppp = EditorGUIUtility.pixelsPerPoint;

            using var scope = new GUILayout.ScrollViewScope(_scroll);
            GUILayout.Space(10);
            _scroll = scope.scrollPosition;

            _buttonSize = _viewBigIcons ? 70 : 40;

            var renderWidth = Screen.width / ppp - 13f;
            var gridW = Mathf.FloorToInt(renderWidth / _buttonSize);
            var marginLeft = (renderWidth - _buttonSize * gridW) / 2;

            for (var index = 0; index < _filteredIconList.Count;)
                DrawIconRow(ref index, gridW, marginLeft);

            GUILayout.Space(10);
        }

        private void DrawIconRow(ref int index, int gridW, float marginLeft)
        {
            using var _ = new GUILayout.HorizontalScope();
            GUILayout.Space(marginLeft);

            for (var col = 0; col < gridW && index < _filteredIconList.Count; ++col, ++index)
            {
                var icon = _filteredIconList[index];
                if (!GUILayout.Button(icon, _iconButtonStyle,
                        GUILayout.Width(_buttonSize), GUILayout.Height(_buttonSize))) continue;

                EditorGUI.FocusTextInControl("");
                _iconSelected = icon;
            }
        }

        private void DrawSelectionPreview()
        {
            if (_iconSelected is null) return;

            GUILayout.FlexibleSpace();

            using var _ = new GUILayout.HorizontalScope(EditorStyles.helpBox,
                GUILayout.MaxHeight(_viewBigIcons ? 140 : 120));

            DrawPreviewImage();
            GUILayout.Space(10);
            DrawPreviewDetails();
            GUILayout.Space(10);

            if (GUILayout.Button("X", GUILayout.ExpandHeight(true)))
                _iconSelected = null;
        }

        private void DrawPreviewImage()
        {
            using var _ = new GUILayout.VerticalScope(GUILayout.Width(130));

            GUILayout.Space(2);

            float height = _viewBigIcons ? 128 : 40;
            var previewRect = GUILayoutUtility.GetRect(128, height, GUILayout.Width(128));

            var bgColor = _darkPreview
                ? new Color(0.15f, 0.15f, 0.15f)
                : new Color(0.85f, 0.85f, 0.85f);
            EditorGUI.DrawRect(previewRect, bgColor);

            if (_iconSelected?.image is { } image)
                GUI.DrawTexture(previewRect, image, ScaleMode.ScaleToFit);

            GUILayout.Space(5);

            _darkPreview = GUILayout.SelectionGrid(
                _darkPreview ? 1 : 0, new[] { "Light", "Dark" },
                2, EditorStyles.miniButton) == 1;

            GUILayout.FlexibleSpace();
        }

        private void DrawPreviewDetails()
        {
            using var _ = new GUILayout.VerticalScope();

            var iconName = _iconSelected!.tooltip;
            var isProSkin = iconName.StartsWith("d_") ? "Yes" : "No";

            var (width, height) = _iconSelected.image is { } img
                ? (img.width, img.height)
                : (0, 0);

            var info = $"Size: {width}x{height}\nIs Pro Skin Icon: {isProSkin}\nTotal {_iconContentListAll.Count} icons";

            GUILayout.Space(5);
            EditorGUILayout.HelpBox(info, MessageType.None);
            GUILayout.Space(5);
            EditorGUILayout.TextField($"EditorGUIUtility.IconContent(\"{iconName}\")");
            GUILayout.Space(5);

            if (GUILayout.Button("Copy to clipboard", EditorStyles.miniButton))
                EditorGUIUtility.systemCopyBuffer = iconName;

            if (GUILayout.Button("Save icon to file ...", EditorStyles.miniButton))
                SaveIcon(iconName);
        }

        #endregion

        #region Icon Loading

        private void InitIcons()
        {
            if (_iconButtonStyle is not null) return;

            _iconButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                margin = new RectOffset(0, 0, 0, 0),
                fixedHeight = 0
            };

            _iconContentListSmall.Clear();
            _iconContentListBig.Clear();
            _iconContentListAll.Clear();

            foreach (var iconName in _icoList)
            {
                if (GetIcon(iconName) is not { } ico) continue;

                ico.tooltip = iconName;
                _iconContentListAll.Add(ico);

                var bucket = ico.image is { width: > 36, height: > 36 }
                    ? _iconContentListBig
                    : _iconContentListSmall;
                bucket.Add(ico);
            }

            UpdateFilteredList();
        }

        private static GUIContent? GetIcon(string iconName)
        {
            if (string.IsNullOrEmpty(iconName)) return null;

            Debug.unityLogger.logEnabled = false;
            var content = EditorGUIUtility.IconContent(iconName);
            Debug.unityLogger.logEnabled = true;

            return content?.image is not null ? content : null;
        }

        #endregion

        #region Save Operations

        private static void SaveIcon(string iconName)
        {
            if (EditorGUIUtility.IconContent(iconName).image is not Texture2D tex)
            {
                Debug.LogError("Cannot save the icon: null texture error!");
                return;
            }

            var path = EditorUtility.SaveFilePanel("Save icon", "", iconName, "png");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                File.WriteAllBytes(path, CopyTexture(tex).EncodeToPNG());
            }
            catch (Exception e)
            {
                Debug.LogError($"Cannot save the icon: {e.Message}");
            }
        }

        private void SaveAllIcons()
        {
            var folderPath = EditorUtility.SaveFolderPanel("", "", "");
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                Debug.LogError("Folder path invalid...");
                return;
            }

            try
            {
                foreach (var icon in _icoList)
                    TrySaveIcon(icon, folderPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Cannot save the icons: {e.Message}");
            }
        }

        private static void TrySaveIcon(string icon, string folderPath)
        {
            if (EditorGUIUtility.IconContent(icon).image is not Texture2D tex) return;

            var fileName = icon.Split('/')[^1];
            var path = Path.Combine(folderPath, $"{fileName}.png");

            if (File.Exists(path))
            {
                Debug.Log($"File already exists, skipping: {path}");
                return;
            }

            File.WriteAllBytes(path, CopyTexture(tex).EncodeToPNG());
        }

        #endregion

        #region Utility

        private static Texture2D CopyTexture(Texture2D source)
        {
            var copy = new Texture2D(source.width, source.height, source.format, source.mipmapCount, true);
            Graphics.CopyTexture(source, copy);
            return copy;
        }

        private static Texture2D CreatePixelTexture(Color color)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, color);
            t.Apply();
            return t;
        }

        private static void ApplyBackgroundToAllStates(GUIStyle style, Texture2D texture)
        {
            var scaled = new[] { texture };
            style.hover.background = style.onHover.background =
                style.focused.background = style.onFocused.background =
                    style.active.background = style.onActive.background =
                        style.normal.background = style.onNormal.background = texture;
            style.hover.scaledBackgrounds = style.onHover.scaledBackgrounds =
                style.focused.scaledBackgrounds = style.onFocused.scaledBackgrounds =
                    style.active.scaledBackgrounds = style.onActive.scaledBackgrounds =
                        style.normal.scaledBackgrounds = style.onNormal.scaledBackgrounds = scaled;
        }

        #endregion
    }
}

#endif
