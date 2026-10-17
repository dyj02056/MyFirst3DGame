using TMPro;
using UnityEngine;
using static SkyHopper.EditorTools.SceneBuilderUtility;
using static SkyHopper.EditorTools.UIBuilder;

namespace SkyHopper.EditorTools
{
    /// <summary>
    /// 모든 스테이지 씬에 공통으로 들어가는 것: StageManager, 화면 글자(HUD), 클리어 화면, 일시정지 화면, 설정 창.
    /// 스테이지 2~5를 만들 때도 CreateStageSystems 한 줄로 똑같이 넣을 수 있습니다.
    /// </summary>
    internal static class StageUIBuilder
    {
        internal static void CreateStageSystems(PlayerController player)
        {
            var manager = new GameObject("StageManager").AddComponent<StageManager>();
            SetReference(manager, "player", player);
            SetReference(manager, "catalog", GetOrCreateStageCatalog());

            var canvas = CreateCanvas("StageUI");
            CreateEventSystem();
            CreateHUD(canvas.transform);
            CreatePauseMenu(canvas.transform);
        }

        static void CreateHUD(Transform root)
        {
            var center = new Vector2(0.5f, 0.5f);

            // 왼쪽 위: 시간, 코인
            var box = CreatePanel(root, "StatusBox", new Color(0f, 0f, 0f, 0.35f),
                new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(420f, 150f));
            var timeText = CreateText(box.transform, "TimeText", "시간  00:00.00", 46f, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(28f, -12f), new Vector2(380f, 64f));
            var coinText = CreateText(box.transform, "CoinText", "코인  0 / 0", 46f, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(28f, -76f), new Vector2(380f, 64f));

            // 별 상자: 별이 있는 스테이지에서만 보임
            var starBox = CreatePanel(root, "StarBox", new Color(0f, 0f, 0f, 0.35f),
                new Vector2(0f, 1f), new Vector2(30f, -192f), new Vector2(420f, 80f));
            var starText = CreateText(starBox.transform, "StarText", "별  0 / 3", 46f, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(28f, -8f), new Vector2(380f, 64f));
            starText.color = new Color(1f, 0.86f, 0.3f);

            // 왼쪽 아래: 조작법
            var hint = CreateText(root, "HintText",
                "WASD 이동    Space 점프 (공중에서 한 번 더)    Shift 대시    R 다시 시작    Esc 일시정지",
                28f, TextAlignmentOptions.Left, new Vector2(0f, 0f), new Vector2(36f, 24f), new Vector2(1500f, 50f));
            hint.color = new Color(1f, 1f, 1f, 0.85f);

            // 가운데: 클리어 화면
            var clearPanel = CreatePanel(root, "ClearPanel", PanelColor, center, Vector2.zero, new Vector2(1100f, 640f));
            var clearText = CreateText(clearPanel.transform, "ClearText", "스테이지 클리어!", 96f, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1000f, 420f));
            var buttons = CreateRow(clearPanel.transform, "Buttons", new Vector2(0.5f, 0f), new Vector2(0f, 40f),
                new Vector2(1040f, 100f), 24f);
            var nextButton = CreateButton(buttons, "NextButton", "다음 스테이지", new Vector2(320f, 92f), 42f);
            var retryButton = CreateButton(buttons, "RetryButton", "다시 하기", new Vector2(320f, 92f), 42f);
            var selectButton = CreateButton(buttons, "StageSelectButton", "스테이지 선택", new Vector2(320f, 92f), 42f);

            var hud = root.gameObject.AddComponent<StageHUD>();
            SetReference(hud, "timeText", timeText);
            SetReference(hud, "coinText", coinText);
            SetReference(hud, "starBox", starBox.gameObject);
            SetReference(hud, "starText", starText);
            SetReference(hud, "clearPanel", clearPanel.gameObject);
            SetReference(hud, "clearText", clearText);
            SetReference(hud, "nextButton", nextButton);
            SetReference(hud, "retryButton", retryButton);
            SetReference(hud, "stageSelectButton", selectButton);
        }

        static void CreatePauseMenu(Transform root)
        {
            var center = new Vector2(0.5f, 0.5f);

            // 일시정지 화면: 뒤를 어둡게 덮고 가운데에 메뉴
            var pausePanel = CreateOverlay(root, "PausePanel", new Color(0f, 0f, 0f, 0.5f));
            var box = CreatePanel(pausePanel.transform, "Box", PanelColor, center, Vector2.zero, new Vector2(700f, 720f));
            CreateText(box.transform, "Title", "일시정지", 80f, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(600f, 110f));
            var column = CreateColumn(box.transform, "Buttons", center, new Vector2(0f, -60f), new Vector2(600f, 500f), 22f);
            var resume = CreateButton(column, "ResumeButton", "계속하기", new Vector2(460f, 96f));
            var restart = CreateButton(column, "RestartButton", "다시 시작", new Vector2(460f, 96f));
            var settings = CreateButton(column, "SettingsButton", "설정", new Vector2(460f, 96f));
            var title = CreateButton(column, "TitleButton", "스테이지 선택으로", new Vector2(460f, 96f));

            var settingsPanel = CreateSettingsPanel(root);

            var pauseMenu = root.gameObject.AddComponent<PauseMenu>();
            SetReference(pauseMenu, "pausePanel", pausePanel.gameObject);
            SetReference(pauseMenu, "settingsPanel", settingsPanel);
            SetReference(pauseMenu, "resumeButton", resume);
            SetReference(pauseMenu, "restartButton", restart);
            SetReference(pauseMenu, "settingsButton", settings);
            SetReference(pauseMenu, "titleButton", title);
        }
    }
}
