using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace SkyHopper.EditorTools
{
    /// <summary>화면(UI)을 코드로 만드는 도우미: 한글 글꼴, 글자, 버튼, 슬라이더, 설정 창.</summary>
    internal static class UIBuilder
    {
        const string FontPath = "Assets/_SkyHopper/Fonts/Jua-Regular.ttf";
        const string FontAssetPath = "Assets/_SkyHopper/Fonts/Jua SDF.asset";

        internal static readonly Color PanelColor = new Color(0.08f, 0.13f, 0.28f, 0.88f);
        static readonly Color ButtonColor = new Color(1f, 0.96f, 0.86f);
        static readonly Color ButtonTextColor = new Color(0.2f, 0.24f, 0.42f);

        static TMP_FontAsset koreanFont;

        /// <summary>
        /// Jua 글꼴로 TextMeshPro용 글꼴 파일(Jua SDF)을 만들거나 불러옴.
        /// "Dynamic" 방식이라 화면에 처음 나오는 글자를 그때그때 추가해서, 한글 11,172자를 미리 넣지 않아도 됩니다.
        /// </summary>
        internal static TMP_FontAsset GetKoreanFont()
        {
            if (koreanFont != null)
                return koreanFont;

            koreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (koreanFont != null)
                return koreanFont;

            var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null)
            {
                Debug.LogWarning("[Sky Hopper] 한글 글꼴 파일이 없습니다: " + FontPath);
                return null;
            }

            koreanFont = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048,
                AtlasPopulationMode.Dynamic, true);
            koreanFont.name = "Jua SDF";
            AssetDatabase.CreateAsset(koreanFont, FontAssetPath);
            koreanFont.atlasTextures[0].name = "Jua SDF Atlas";
            AssetDatabase.AddObjectToAsset(koreanFont.atlasTextures[0], koreanFont);
            koreanFont.material.name = "Jua SDF Material";
            AssetDatabase.AddObjectToAsset(koreanFont.material, koreanFont);
            AssetDatabase.SaveAssets();
            return koreanFont;
        }

        internal static Canvas CreateCanvas(string name, int sortingOrder = 0)
        {
            var canvasObject = new GameObject(name, typeof(RectTransform));
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        /// <summary>버튼 클릭을 받으려면 씬에 EventSystem이 하나 있어야 함</summary>
        internal static void CreateEventSystem()
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        /// <summary>anchor 지점을 기준으로 position만큼 떨어진 곳에 size 크기의 영역</summary>
        internal static RectTransform CreateRect(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        /// <summary>부모 영역을 꽉 채우도록</summary>
        internal static RectTransform Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        internal static RectTransform CreateFullScreen(Transform parent, string name) =>
            Stretch(CreateRect(parent, name, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero));

        internal static TextMeshProUGUI CreateText(Transform parent, string name, string text, float fontSize,
            TextAlignmentOptions alignment, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var rect = CreateRect(parent, name, anchor, position, size);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = GetKoreanFont();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            return label;
        }

        /// <summary>둥근 모서리 사각형 배경</summary>
        internal static Image CreatePanel(Transform parent, string name, Color color, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var rect = CreateRect(parent, name, anchor, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            image.type = Image.Type.Sliced;
            image.color = color;
            return image;
        }

        /// <summary>단색 배경 (화면 전체를 어둡게 덮을 때 등)</summary>
        internal static Image CreateOverlay(Transform parent, string name, Color color)
        {
            var image = CreateFullScreen(parent, name).gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        /// <summary>안에 넣은 버튼들을 세로로 가운데 정렬해 쌓는 영역</summary>
        internal static RectTransform CreateColumn(Transform parent, string name, Vector2 anchor, Vector2 position,
            Vector2 size, float spacing)
        {
            var rect = CreateRect(parent, name, anchor, position, size);
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            ConfigureLayout(layout, spacing);
            return rect;
        }

        /// <summary>안에 넣은 버튼들을 가로로 가운데 정렬해 늘어놓는 영역</summary>
        internal static RectTransform CreateRow(Transform parent, string name, Vector2 anchor, Vector2 position,
            Vector2 size, float spacing)
        {
            var rect = CreateRect(parent, name, anchor, position, size);
            var layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            ConfigureLayout(layout, spacing);
            return rect;
        }

        static void ConfigureLayout(HorizontalOrVerticalLayoutGroup layout, float spacing)
        {
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        internal static Button CreateButton(Transform parent, string name, string label, Vector2 size,
            float fontSize, out TextMeshProUGUI labelText)
        {
            var rect = CreateRect(parent, name, new Vector2(0.5f, 0.5f), Vector2.zero, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            image.color = Color.white;

            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = ButtonColor;
            colors.highlightedColor = Color.white;
            colors.selectedColor = ButtonColor;
            colors.pressedColor = new Color(0.85f, 0.8f, 0.68f);
            colors.disabledColor = new Color(0.55f, 0.55f, 0.6f, 0.7f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            rect.gameObject.AddComponent<ButtonClickSound>();

            labelText = CreateText(rect, "Label", label, fontSize, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), Vector2.zero, size);
            Stretch(labelText.rectTransform);
            labelText.color = ButtonTextColor;
            return button;
        }

        internal static Button CreateButton(Transform parent, string name, string label, Vector2 size, float fontSize = 48f) =>
            CreateButton(parent, name, label, size, fontSize, out _);

        /// <summary>설정 창 (처음엔 꺼진 상태)</summary>
        internal static SettingsPanel CreateSettingsPanel(Transform parent)
        {
            var panel = CreatePanel(parent, "SettingsPanel", PanelColor, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(1000f, 660f));
            CreateText(panel.transform, "Title", "설정", 72f, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(900f, 100f));

            CreateSliderRow(panel.transform, "Sensitivity", "마우스 감도", new Vector2(0f, 110f),
                out var sensitivitySlider, out var sensitivityValue);
            CreateSliderRow(panel.transform, "Volume", "전체 음량", new Vector2(0f, 0f),
                out var volumeSlider, out var volumeValue);
            CreateSliderRow(panel.transform, "Music", "배경음악", new Vector2(0f, -110f),
                out var musicSlider, out var musicValue);

            var close = CreateButton(panel.transform, "CloseButton", "닫기", new Vector2(320f, 90f));
            var closeRect = (RectTransform)close.transform;
            closeRect.anchorMin = closeRect.anchorMax = closeRect.pivot = new Vector2(0.5f, 0f);
            closeRect.anchoredPosition = new Vector2(0f, 40f);

            var settings = panel.gameObject.AddComponent<SettingsPanel>();
            SceneBuilderUtility.SetReference(settings, "sensitivitySlider", sensitivitySlider);
            SceneBuilderUtility.SetReference(settings, "sensitivityValue", sensitivityValue);
            SceneBuilderUtility.SetReference(settings, "volumeSlider", volumeSlider);
            SceneBuilderUtility.SetReference(settings, "volumeValue", volumeValue);
            SceneBuilderUtility.SetReference(settings, "musicSlider", musicSlider);
            SceneBuilderUtility.SetReference(settings, "musicValue", musicValue);
            SceneBuilderUtility.SetReference(settings, "closeButton", close);

            panel.gameObject.SetActive(false);
            return settings;
        }

        /// <summary>[이름] [━━━●━━━] [값] 한 줄</summary>
        static void CreateSliderRow(Transform parent, string name, string label, Vector2 position,
            out Slider slider, out TextMeshProUGUI valueText)
        {
            var row = CreateRect(parent, name, new Vector2(0.5f, 0.5f), position, new Vector2(880f, 90f));
            CreateText(row, "Label", label, 46f, TextAlignmentOptions.Left,
                new Vector2(0f, 0.5f), Vector2.zero, new Vector2(260f, 80f));
            valueText = CreateText(row, "Value", "", 46f, TextAlignmentOptions.Right,
                new Vector2(1f, 0.5f), Vector2.zero, new Vector2(150f, 80f));

            var resources = new DefaultControls.Resources
            {
                standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
                background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
                knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
            };
            var sliderObject = DefaultControls.CreateSlider(resources);
            sliderObject.name = "Slider";
            var sliderRect = (RectTransform)sliderObject.transform;
            sliderRect.SetParent(row, false);
            sliderRect.anchorMin = sliderRect.anchorMax = sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.anchoredPosition = new Vector2(30f, 0f);
            sliderRect.sizeDelta = new Vector2(420f, 36f);
            slider = sliderObject.GetComponent<Slider>();
        }
    }
}
