#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EditorIcons : EditorWindow
{
    #region Icon Names

    // https://gist.github.com/MattRix/c1f7840ae2419d8eb2ec0695448d4321
    // https://gist.githubusercontent.com/MattRix/c1f7840ae2419d8eb2ec0695448d4321/raw/UnityEditorIcons.txt

    private static string[] _icoList =
    {
        "ScriptableObject Icon", "_Popup", "_Help", "Clipboard", "SocialNetworks.UDNOpen", "SocialNetworks.Tweet", "SocialNetworks.FacebookShare",
        "SocialNetworks.LinkedInShare", "SocialNetworks.UDNLogo", "animationvisibilitytoggleoff", "animationvisibilitytoggleon", "tree_icon", "tree_icon_leaf",
        "tree_icon_frond", "tree_icon_branch", "tree_icon_branch_frond", "editicon.sml", "TreeEditor.Refresh", "TreeEditor.Duplicate", "TreeEditor.Trash",
        "TreeEditor.AddBranches", "TreeEditor.AddLeaves", "preAudioPlayOn", "preAudioPlayOff", "AvatarInspector/RightFingersIk", "AvatarInspector/LeftFingersIk",
        "AvatarInspector/RightFeetIk", "AvatarInspector/LeftFeetIk", "AvatarInspector/RightFingers", "AvatarInspector/LeftFingers", "AvatarInspector/RightArm",
        "AvatarInspector/LeftArm", "AvatarInspector/RightLeg", "AvatarInspector/LeftLeg", "AvatarInspector/Head", "AvatarInspector/Torso", "AvatarInspector/MaskEditor_Root",
        "AvatarInspector/BodyPartPicker", "AvatarInspector/BodySIlhouette", "Mirror", "SpeedScale", "Toolbar Minus", "Toolbar Plus More", "Toolbar Plus",
        "AnimatorController Icon", "TextAsset Icon", "Shader Icon", "boo Script Icon", "cs Script Icon", "js Script Icon", "Prefab Icon", "Profiler.NextFrame",
        "Profiler.PrevFrame", "sv_icon_none", "ColorPicker.CycleSlider", "ColorPicker.CycleColor", "EyeDropper.Large", "ClothInspector.PaintValue", "ClothInspector.ViewValue",
        "ClothInspector.SettingsTool", "ClothInspector.PaintTool", "ClothInspector.SelectTool", "WelcomeScreen.AssetStoreLogo", "AboutWindow.MainHeader", "UnityLogo",
        "AgeiaLogo", "MonoLogo", "PlayButtonProfile Anim", "StepButton Anim", "PauseButton Anim", "PlayButton Anim", "PlayButtonProfile On", "StepButton On", "PauseButton On",
        "PlayButton On", "PlayButtonProfile", "StepButton", "PauseButton", "PlayButton", "ViewToolOrbit On", "ViewToolZoom On", "ViewToolMove On", "ViewToolOrbit",
        "ViewToolZoom", "ViewToolMove", "ScaleTool On", "RotateTool On", "MoveTool On", "ScaleTool", "RotateTool", "MoveTool", "Icon Dropdown", "Avatar Icon", "AvatarPivot",
        "AvatarInspector/DotSelection", "AvatarInspector/DotFrameDotted", "AvatarInspector/DotFrame", "AvatarInspector/DotFill", "AvatarInspector/RightHandZoom",
        "AvatarInspector/LeftHandZoom", "AvatarInspector/HeadZoom", "AvatarInspector/RightHandZoomSilhouette", "AvatarInspector/LeftHandZoomSilhouette",
        "AvatarInspector/HeadZoomSilhouette", "AvatarInspector/BodySilhouette", "Animation.AddKeyframe", "Animation.NextKey", "Animation.PrevKey", "lightMeter/redLight",
        "lightMeter/orangeLight", "lightMeter/lightRim", "lightMeter/greenLight", "Animation.AddEvent", "SceneviewAudio", "SceneviewLighting", "MeshRenderer Icon",
        "Terrain Icon", "BuildSettings.SelectedIcon", "Animation.Record", "Animation.Play", "PreTextureRGB", "PreTextureAlpha", "PreTextureMipMapHigh", "PreTextureMipMapLow",
        "TerrainInspector.TerrainToolSettings", "TerrainInspector.TerrainToolPlants", "TerrainInspector.TerrainToolTrees", "TerrainInspector.TerrainToolSplat",
        "TerrainInspector.TerrainToolSmoothHeight", "TerrainInspector.TerrainToolSetHeight", "TerrainInspector.TerrainToolRaise", "SettingsIcon", "PreMatLight1",
        "PreMatLight0", "PreMatTorus", "PreMatCylinder", "PreMatCube", "PreMatSphere", "Camera Icon", "Animation.EventMarker", "AS Badge New", "AS Badge Move",
        "AS Badge Delete", "WaitSpin00", "WaitSpin01", "WaitSpin02", "WaitSpin03", "WaitSpin04", "WaitSpin05", "WaitSpin06", "WaitSpin07", "WaitSpin08", "WaitSpin09",
        "WaitSpin10", "WaitSpin11", "WelcomeScreen.UnityAnswersLogo", "WelcomeScreen.UnityForumLogo", "WelcomeScreen.UnityBasicsLogo", "WelcomeScreen.VideoTutLogo",
        "WelcomeScreen.MainHeader", "VerticalSplit", "HorizontalSplit", "PrefabNormal Icon", "PrefabModel Icon", "GameObject Icon", "preAudioLoopOn", "preAudioLoopOff",
        "preAudioAutoPlayOn", "preAudioAutoPlayOff", "BuildSettings.Web.Small", "BuildSettings.Standalone.Small", "BuildSettings.iPhone.Small", "BuildSettings.Android.Small",
        "BuildSettings.BlackBerry.Small", "BuildSettings.Tizen.Small", "BuildSettings.XBox360.Small", "BuildSettings.XboxOne.Small", "BuildSettings.PS3.Small",
        "BuildSettings.PSP2.Small", "BuildSettings.PS4.Small", "BuildSettings.PSM.Small", "BuildSettings.FlashPlayer.Small", "BuildSettings.Metro.Small",
        "BuildSettings.WP8.Small", "BuildSettings.SamsungTV.Small", "BuildSettings.Web", "BuildSettings.Standalone", "BuildSettings.iPhone", "BuildSettings.Android",
        "BuildSettings.BlackBerry", "BuildSettings.Tizen", "BuildSettings.XBox360", "BuildSettings.XboxOne", "BuildSettings.PS3", "BuildSettings.PSP2", "BuildSettings.PS4",
        "BuildSettings.PSM", "BuildSettings.FlashPlayer", "BuildSettings.Metro", "BuildSettings.WP8", "BuildSettings.SamsungTV", "TreeEditor.BranchTranslate",
        "TreeEditor.BranchRotate", "TreeEditor.BranchFreeHand", "TreeEditor.BranchTranslate On", "TreeEditor.BranchRotate On", "TreeEditor.BranchFreeHand On",
        "TreeEditor.LeafTranslate", "TreeEditor.LeafRotate", "TreeEditor.LeafTranslate On", "TreeEditor.LeafRotate On", "sv_icon_dot15_pix16_gizmo", "sv_icon_dot1_sml",
        "sv_icon_dot4_sml", "sv_icon_dot7_sml", "sv_icon_dot5_pix16_gizmo", "sv_icon_dot11_pix16_gizmo", "sv_icon_dot12_sml", "sv_icon_dot15_sml", "sv_icon_dot9_pix16_gizmo",
        "sv_icon_name6", "sv_icon_name3", "sv_icon_name4", "sv_icon_name0", "sv_icon_name1", "sv_icon_name2", "sv_icon_name5", "sv_icon_name7", "sv_icon_dot1_pix16_gizmo",
        "sv_icon_dot8_pix16_gizmo", "sv_icon_dot2_pix16_gizmo", "sv_icon_dot6_pix16_gizmo", "sv_icon_dot0_sml", "sv_icon_dot3_sml", "sv_icon_dot6_sml", "sv_icon_dot9_sml",
        "sv_icon_dot11_sml", "sv_icon_dot14_sml", "sv_label_0", "sv_label_1", "sv_label_2", "sv_label_3", "sv_label_5", "sv_label_6", "sv_label_7", "sv_icon_dot14_pix16_gizmo",
        "sv_icon_dot7_pix16_gizmo", "sv_icon_dot3_pix16_gizmo", "sv_icon_dot0_pix16_gizmo", "sv_icon_dot2_sml", "sv_icon_dot5_sml", "sv_icon_dot8_sml",
        "sv_icon_dot10_pix16_gizmo", "sv_icon_dot12_pix16_gizmo", "sv_icon_dot10_sml", "sv_icon_dot13_sml", "sv_icon_dot4_pix16_gizmo", "sv_label_4",
        "sv_icon_dot13_pix16_gizmo"
    };

    #endregion

    private int _buttonSize = 70;
    private bool _darkPreview = true;

    // Cached list to eliminate per-frame allocations
    private List<GUIContent> _filteredIconList = new();
    private GUIStyle? _iconButtonStyle;

    private readonly List<GUIContent> _iconContentListAll = new();
    private readonly List<GUIContent> _iconContentListBig = new();
    private readonly List<GUIContent> _iconContentListSmall = new();
    private GUIStyle? _iconPreviewBlack;
    private GUIStyle? _iconPreviewWhite;
    private GUIContent? _iconSelected;
    private Vector2 _scroll;
    private string _search = string.Empty;

    private bool _viewBigIcons = true;

    private bool IsWide => Screen.width > 550;
    private bool DoSearch => !string.IsNullOrWhiteSpace(_search);

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
        var knownIcons = new HashSet<string>(_icoList.Where(x => GetIcon(x) != null));

        var discovered = Resources.FindObjectsOfTypeAll<Texture2D>()
            .Select(t => t.name)
            .Where(iconName => GetIcon(iconName) != null && !knownIcons.Contains(iconName))
            .ToList();

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
        using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            if (GUILayout.Button("Save all icons to folder...", EditorStyles.toolbarButton))
                SaveAllIcons();

            EditorGUI.BeginChangeCheck();
            if (GUILayout.Toggle(!_viewBigIcons, "Small", EditorStyles.toolbarButton)) _viewBigIcons = false;
            if (GUILayout.Toggle(_viewBigIcons, "Big", EditorStyles.toolbarButton)) _viewBigIcons = true;

            // Update cache if view mode changes
            if (EditorGUI.EndChangeCheck()) UpdateFilteredList();

            if (IsWide) DrawSearchBar();
        }
    }

    private void DrawSearchBar()
    {
        using (new GUILayout.HorizontalScope())
        {
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

            // Update cache only when the search string changes
            if (EditorGUI.EndChangeCheck())
            {
                _search = newSearch;
                UpdateFilteredList();
            }
        }
    }

    private void UpdateFilteredList()
    {
        if (!DoSearch)
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

        int row = 0, index = 0;
        while (index < _filteredIconList.Count)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(marginLeft);

                for (var i = 0; i < gridW && index < _filteredIconList.Count; ++i, ++index)
                {
                    var icon = _filteredIconList[i + row * gridW];

                    if (GUILayout.Button(icon, _iconButtonStyle,
                            GUILayout.Width(_buttonSize), GUILayout.Height(_buttonSize)))
                    {
                        EditorGUI.FocusTextInControl("");
                        _iconSelected = icon;
                    }
                }
            }

            row++;
        }

        GUILayout.Space(10);
    }

    private void DrawSelectionPreview()
    {
        if (_iconSelected == null) return;

        GUILayout.FlexibleSpace();

        using (new GUILayout.HorizontalScope(EditorStyles.helpBox,
                   GUILayout.MaxHeight(_viewBigIcons ? 140 : 120)))
        {
            DrawPreviewImage();
            GUILayout.Space(10);
            DrawPreviewDetails();
            GUILayout.Space(10);

            if (GUILayout.Button("X", GUILayout.ExpandHeight(true)))
                _iconSelected = null;
        }
    }

    private void DrawPreviewImage()
    {
        using (new GUILayout.VerticalScope(GUILayout.Width(130)))
        {
            GUILayout.Space(2);

            float height = _viewBigIcons ? 128 : 40;
            var previewRect = GUILayoutUtility.GetRect(128, height, GUILayout.Width(128));

            var bgColor = _darkPreview ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.85f, 0.85f, 0.85f);
            EditorGUI.DrawRect(previewRect, bgColor);

            // Pattern Matching for null check and assignment
            if (_iconSelected?.image is { } image)
                GUI.DrawTexture(previewRect, image, ScaleMode.ScaleToFit);

            GUILayout.Space(5);

            _darkPreview = GUILayout.SelectionGrid(
                _darkPreview ? 1 : 0, new[] { "Light", "Dark" },
                2, EditorStyles.miniButton) == 1;

            GUILayout.FlexibleSpace();
        }
    }

    private void DrawPreviewDetails()
    {
        using (new GUILayout.VerticalScope())
        {
            var iconName = _iconSelected!.tooltip;
            var isProSkin = iconName.StartsWith("d_") ? "Yes" : "No";

            // Null check bypass for image via pattern matching
            var width = _iconSelected.image is { } imgW ? imgW.width : 0;
            var height = _iconSelected.image is { } imgH ? imgH.height : 0;

            var info = $"Size: {width}x{height}"
                       + $"\nIs Pro Skin Icon: {isProSkin}"
                       + $"\nTotal {_iconContentListAll.Count} icons";

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
    }

    #endregion

    #region Icon Loading

    private void InitIcons()
    {
        if (_iconButtonStyle != null) return;

        // Target-Typed new()
        _iconButtonStyle = new GUIStyle(EditorStyles.miniButton) { margin = new RectOffset(0, 0, 0, 0), fixedHeight = 0 };

        _iconPreviewBlack = new GUIStyle(_iconButtonStyle);
        ApplyBackgroundToAllStates(_iconPreviewBlack, CreatePixelTexture(new Color(0.15f, 0.15f, 0.15f)));

        _iconPreviewWhite = new GUIStyle(_iconButtonStyle);
        ApplyBackgroundToAllStates(_iconPreviewWhite, CreatePixelTexture(new Color(0.85f, 0.85f, 0.85f)));

        _iconContentListSmall.Clear();
        _iconContentListBig.Clear();
        _iconContentListAll.Clear();

        foreach (var iconName in _icoList)
        {
            var ico = GetIcon(iconName);

            if (ico == null) continue;

            ico.tooltip = iconName;
            _iconContentListAll.Add(ico);

            if (ico.image is { } img && img.width > 36 && img.height > 36)
                _iconContentListBig.Add(ico);
            else
                _iconContentListSmall.Add(ico);
        }

        UpdateFilteredList(); // Initialize the cache
    }

    private static GUIContent? GetIcon(string iconName)
    {
        if (string.IsNullOrEmpty(iconName)) return null;

        Debug.unityLogger.logEnabled = false;
        var content = EditorGUIUtility.IconContent(iconName);
        Debug.unityLogger.logEnabled = true;

        return content?.image != null ? content : null;
    }

    #endregion

    #region Save Operations

    private void SaveIcon(string iconName)
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
            var outTex = CopyTexture(tex);
            File.WriteAllBytes(path, outTex.EncodeToPNG());
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
            {
                if (EditorGUIUtility.IconContent(icon).image is not Texture2D tex)
                    continue;

                var fileName = icon.Split('/').Last();
                var path = Path.Combine(folderPath, $"{fileName}.png");

                if (File.Exists(path))
                {
                    Debug.Log($"File already exists, skipping: {path}");
                    continue;
                }

                var outTex = CopyTexture(tex);
                File.WriteAllBytes(path, outTex.EncodeToPNG());
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Cannot save the icons: {e.Message}");
        }
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
#endif
