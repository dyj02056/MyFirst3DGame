using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static SkyHopper.EditorTools.SceneBuilderUtility;
using static SkyHopper.EditorTools.UIBuilder;

namespace SkyHopper.EditorTools
{
    /// <summary>
    /// 상단 메뉴 "Sky Hopper > 타이틀 화면 만들기"
    /// 타이틀(시작하기·설정·종료) + 스테이지 선택 + 설정 화면이 들어 있는 Title 씬을 만들고,
    /// 게임을 실행하면 가장 먼저 열리는 씬으로 지정합니다.
    /// </summary>
    public static class TitleBuilder
    {
        const string ScenePath = SceneFolder + "/" + SceneLoader.TitleScene + ".unity";
        const int StageButtonCount = 5;

        [MenuItem("Sky Hopper/타이틀 화면 만들기", priority = -1)]
        static void Build()
        {
            if (!TryCreateNewScene(ScenePath, "타이틀 화면 만들기", out var scene))
                return;

            var catalog = GetOrCreateStageCatalog();
            CreateLighting();
            CreateBackground();

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<Camera>().farClipPlane = 500f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<TitleCameraOrbit>();
            cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 5f, -14f), Quaternion.Euler(16f, 0f, 0f));

            var canvas = CreateCanvas("TitleUI");
            CreateEventSystem();
            var root = canvas.transform;
            var center = new Vector2(0.5f, 0.5f);

            // ── 메인 화면 ──
            var mainPanel = CreateFullScreen(root, "MainPanel");
            var title = CreateText(mainPanel, "Title", "Sky Hopper", 160f, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(1600f, 220f));
            title.color = new Color(1f, 0.98f, 0.9f);
            var subtitle = CreateText(mainPanel, "Subtitle", "하늘섬 점프 모험", 60f, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0f, -330f), new Vector2(1200f, 90f));
            subtitle.color = new Color(1f, 0.85f, 0.35f);

            var menu = CreateColumn(mainPanel, "Buttons", center, new Vector2(0f, -150f), new Vector2(600f, 400f), 24f);
            var startButton = CreateButton(menu, "StartButton", "시작하기", new Vector2(480f, 100f), 54f);
            var settingsButton = CreateButton(menu, "SettingsButton", "설정", new Vector2(480f, 100f), 54f);
            var quitButton = CreateButton(menu, "QuitButton", "종료", new Vector2(480f, 100f), 54f);

            CreateText(mainPanel, "Controls", "WASD 이동     Space 점프 (공중에서 한 번 더)     Shift 대시     Esc 일시정지",
                30f, TextAlignmentOptions.Center, new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(1600f, 50f));

            // ── 스테이지 선택 화면 ──
            var stageSelect = CreatePanel(root, "StageSelectPanel", PanelColor, center, Vector2.zero, new Vector2(1000f, 960f));
            CreateText(stageSelect.transform, "Title", "스테이지 선택", 72f, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(900f, 100f));
            var list = CreateColumn(stageSelect.transform, "StageList", center, new Vector2(0f, 10f), new Vector2(900f, 680f), 16f);
            var stageButtons = new Object[StageButtonCount];
            var stageLabels = new Object[StageButtonCount];
            for (int i = 0; i < StageButtonCount; i++)
            {
                stageButtons[i] = CreateButton(list, "Stage" + (i + 1), "스테이지 " + (i + 1),
                    new Vector2(860f, 118f), 46f, out var label);
                stageLabels[i] = label;
            }
            var backButton = CreateButton(stageSelect.transform, "BackButton", "뒤로", new Vector2(300f, 90f));
            var backRect = (RectTransform)backButton.transform;
            backRect.anchorMin = backRect.anchorMax = backRect.pivot = new Vector2(0.5f, 0f);
            backRect.anchoredPosition = new Vector2(0f, 36f);

            // ── 설정 화면 ──
            var settings = CreateSettingsPanel(root);

            var titleMenu = canvas.gameObject.AddComponent<TitleMenu>();
            SetReference(titleMenu, "catalog", catalog);
            SetReference(titleMenu, "mainPanel", mainPanel.gameObject);
            SetReference(titleMenu, "stageSelectPanel", stageSelect.gameObject);
            SetReference(titleMenu, "settingsPanel", settings);
            SetReference(titleMenu, "startButton", startButton);
            SetReference(titleMenu, "settingsButton", settingsButton);
            SetReference(titleMenu, "quitButton", quitButton);
            SetReference(titleMenu, "backButton", backButton);
            SetReferenceArray(titleMenu, "stageButtons", stageButtons);
            SetReferenceArray(titleMenu, "stageLabels", stageLabels);
            stageSelect.gameObject.SetActive(false);

            SaveScene(scene, ScenePath);
            MoveSceneToFront(ScenePath);
            Debug.Log("[Sky Hopper] 타이틀 화면을 만들고 게임 첫 화면으로 지정했습니다: " + ScenePath);
        }

        /// <summary>뒤에 보이는 하늘섬 풍경 (카메라가 천천히 돌며 보여 줌)</summary>
        static void CreateBackground()
        {
            var scenery = new GameObject("Scenery").transform;
            CreateIsland(scenery, "Main_Island", Vector3.zero, new Vector2(10f, 10f));
            CreateIsland(scenery, "Island_East", new Vector3(14f, 2f, 6f), new Vector2(5f, 5f));
            CreateIsland(scenery, "Island_West", new Vector3(-13f, -1f, 7f), new Vector2(6f, 6f));
            CreateIsland(scenery, "Island_North", new Vector3(3f, 4f, 16f), new Vector2(4f, 4f));

            // 섬 위에서 빙글빙글 도는 커다란 코인
            var coin = new GameObject("Big_Coin");
            coin.transform.SetParent(scenery, false);
            coin.transform.localPosition = new Vector3(0f, 2f, 0f);
            coin.AddComponent<Rotator>();
            var disc = CreateVisual(PrimitiveType.Cylinder, "Disc", coin.transform, Vector3.zero,
                new Vector3(1.6f, 0.1f, 1.6f), LoadMaterial("Material_GoldGlow"));
            disc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var cloud = GetOrCreateColorMaterial("SH_Cloud", new Color(0.97f, 0.98f, 1f));
            var cloudPositions = new[]
            {
                new Vector3(-20f, -6f, 20f), new Vector3(22f, -4f, -10f), new Vector3(5f, -9f, -25f),
                new Vector3(-25f, 3f, -15f), new Vector3(28f, 6f, 25f),
            };
            for (int i = 0; i < cloudPositions.Length; i++)
            {
                var puff = CreateVisual(PrimitiveType.Sphere, "Cloud_" + (i + 1), scenery, cloudPositions[i],
                    new Vector3(14f, 3f, 9f), cloud);
                puff.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
    }
}
