using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static SkyHopper.EditorTools.SceneBuilderUtility;

namespace SkyHopper.EditorTools
{
    /// <summary>
    /// 상단 메뉴 "Sky Hopper > 스테이지 1 만들기"로 스테이지 1 "첫 번째 섬"을 자동 생성합니다.
    /// GAME_DESIGN.md 7번 표: 이동, 점프, 더블점프, 코인을 익히는 튜토리얼 스테이지.
    ///
    /// 코스 순서
    ///   ① 시작 섬 (코인 줄 따라 걷기)
    ///   ② 작은 섬 3개 (3m 틈을 점프로 건너며 한 칸씩 올라감)
    ///   ③ 체크포인트 1
    ///   ④ 오른쪽으로 계단 섬 3개 (1.5m씩 오르기)
    ///   ⑤ 7m 틈 (더블점프 연습)
    ///   ⑥ 3.2m 높은 벽 (더블점프로 오르기) → 체크포인트 2
    ///   ⑦ 점점 넓어지는 틈 3개 (4m, 5m, 6m)
    ///   ⑧ 골 게이트
    /// </summary>
    public static class Stage1Builder
    {
        const string ScenePath = SceneFolder + "/Stage1.unity";
        const string PickupSoundPath = "Assets/SourceFiles/SoundFX/SFX_PositiveSound01.ogg";

        static Material cloud, coinMaterial, goldTrim;
        static AudioClip pickupSound;
        static Transform islands, coins;

        [MenuItem("Sky Hopper/스테이지 1 만들기", priority = 0)]
        static void Build()
        {
            if (!TryCreateNewScene(ScenePath, "스테이지 1 만들기", out var scene))
                return;

            LoadResources();
            CreateLighting();

            islands = new GameObject("Islands").transform;
            coins = new GameObject("Coins").transform;
            var checkpoints = new GameObject("Checkpoints").transform;

            // ① 시작 섬: 걸으면서 코인 줍기
            Island("01_Start", new Vector3(0f, 0f, 0f), new Vector2(16f, 16f));
            CoinLine(new Vector3(0f, 1f, -1f), new Vector3(0f, 1f, 5f), 4);

            // ② 3m 틈을 건너며 1m씩 올라가는 작은 섬
            Island("02_Hop_A", new Vector3(0f, 0f, 14f), new Vector2(6f, 6f));
            Island("02_Hop_B", new Vector3(0f, 1f, 23f), new Vector2(6f, 6f));
            Island("02_Hop_C", new Vector3(0f, 2f, 32f), new Vector2(6f, 6f));
            Coin(new Vector3(0f, 2f, 9.5f));
            Coin(new Vector3(0f, 1f, 14f));
            Coin(new Vector3(0f, 3f, 18.5f));
            Coin(new Vector3(0f, 2f, 23f));
            Coin(new Vector3(0f, 4f, 27.5f));
            Coin(new Vector3(0f, 3f, 32f));

            // ③ 체크포인트 1 (다음 길은 오른쪽 +X 방향)
            Island("03_Checkpoint1", new Vector3(0f, 2f, 44f), new Vector2(12f, 12f));
            CreateCheckpoint(checkpoints, new Vector3(0f, 2f, 41f), Quaternion.Euler(0f, 90f, 0f));
            Coin(new Vector3(-3f, 3f, 46f));
            Coin(new Vector3(3f, 3f, 46f));

            // ④ 1.5m씩 올라가는 계단 섬
            Island("04_Step_A", new Vector3(10f, 3.5f, 44f), new Vector2(4f, 4f));
            Island("04_Step_B", new Vector3(15f, 5f, 44f), new Vector2(4f, 4f));
            Island("04_Step_C", new Vector3(20f, 6.5f, 44f), new Vector2(4f, 4f));
            Coin(new Vector3(10f, 4.5f, 44f));
            Coin(new Vector3(15f, 6f, 44f));
            Coin(new Vector3(20f, 7.5f, 44f));

            // ⑤ 7m 틈: 더블점프 연습 (틈 위 코인은 더블점프 궤적을 따라 배치)
            Island("05_DoubleJump", new Vector3(33f, 6.5f, 44f), new Vector2(8f, 8f));
            Coin(new Vector3(24.5f, 8.5f, 44f));
            Coin(new Vector3(26.5f, 9f, 44f));
            Coin(new Vector3(33f, 7.5f, 44f));

            // ⑥ 3.2m 높은 벽 위의 체크포인트 2 (다음 길은 앞쪽 +Z 방향)
            Island("06_HighWall_Checkpoint2", new Vector3(43f, 9.7f, 44f), new Vector2(12f, 12f));
            CreateCheckpoint(checkpoints, new Vector3(40.5f, 9.7f, 44f), Quaternion.identity);
            Coin(new Vector3(43f, 10.7f, 47f));
            Coin(new Vector3(46f, 10.7f, 47f));

            // ⑦ 점점 넓어지는 틈: 4m → 5m → 6m
            Island("07_Gap4", new Vector3(43f, 9.7f, 56f), new Vector2(4f, 4f));
            Island("07_Gap5", new Vector3(43f, 10.5f, 65f), new Vector2(4f, 4f));
            Island("07_Gap6", new Vector3(43f, 10.5f, 75f), new Vector2(4f, 4f));
            Coin(new Vector3(43f, 11.7f, 52f));
            Coin(new Vector3(43f, 10.7f, 56f));
            Coin(new Vector3(43f, 12.5f, 61f));
            Coin(new Vector3(43f, 11.5f, 65f));
            Coin(new Vector3(43f, 12.8f, 70.5f));
            Coin(new Vector3(43f, 11.5f, 75f));

            // ⑧ 골
            Island("08_Goal", new Vector3(43f, 10.5f, 85f), new Vector2(10f, 10f));
            CreateGoal(new Vector3(43f, 10.5f, 87f));

            CreateClouds();

            var player = CreatePlayer(new Vector3(0f, 0.1f, -4f), Quaternion.identity);
            CreateCamera(player);
            CreateStageManager(player);
            CreateHUD();

            SaveScene(scene, ScenePath);
            Selection.activeGameObject = player.gameObject;
            Debug.Log($"[Sky Hopper] 스테이지 1을 만들었습니다 (코인 {coins.childCount}개): {ScenePath}");
        }

        static void LoadResources()
        {
            coinMaterial = LoadMaterial("Material_GoldGlow");
            goldTrim = LoadMaterial("Material_Simple_Yellow");
            cloud = GetOrCreateColorMaterial("SH_Cloud", new Color(0.97f, 0.98f, 1f));
            pickupSound = LoadAsset<AudioClip>(PickupSoundPath);
        }

        static void Island(string name, Vector3 topCenter, Vector2 size)
        {
            CreateIsland(islands, name, topCenter, size);
        }

        static void Coin(Vector3 position)
        {
            var coin = new GameObject("Coin");
            coin.transform.SetParent(coins, false);
            coin.transform.localPosition = position;

            var trigger = coin.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.6f;

            var coinScript = coin.AddComponent<SkyHopper.Coin>();
            SetReference(coinScript, "pickupSound", pickupSound);

            var visual = CreateVisual(PrimitiveType.Cylinder, "Visual", coin.transform,
                Vector3.zero, new Vector3(0.7f, 0.06f, 0.7f), coinMaterial);
            visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        static void CoinLine(Vector3 from, Vector3 to, int count)
        {
            for (int i = 0; i < count; i++)
                Coin(Vector3.Lerp(from, to, count == 1 ? 0f : i / (float)(count - 1)));
        }

        static void CreateGoal(Vector3 position)
        {
            var goal = new GameObject("Goal");
            goal.transform.position = position;

            CreateBlock(goal.transform, "Pillar_Left", new Vector3(-2f, 2f, 0f), new Vector3(0.6f, 4f, 0.6f), goldTrim);
            CreateBlock(goal.transform, "Pillar_Right", new Vector3(2f, 2f, 0f), new Vector3(0.6f, 4f, 0.6f), goldTrim);
            CreateBlock(goal.transform, "Beam", new Vector3(0f, 4.3f, 0f), new Vector3(4.6f, 0.6f, 0.6f), goldTrim);
            CreateVisual(PrimitiveType.Cube, "Glow", goal.transform,
                new Vector3(0f, 2f, 0f), new Vector3(3.4f, 4f, 0.05f), coinMaterial);

            var trigger = goal.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 2f, 0f);
            trigger.size = new Vector3(3.4f, 4f, 1.5f);

            var goalScript = goal.AddComponent<Goal>();
            SetReference(goalScript, "clearSound", pickupSound);
        }

        static void CreateClouds()
        {
            var clouds = new GameObject("Clouds").transform;
            var positions = new[]
            {
                new Vector3(-18f, -6f, 10f), new Vector3(15f, -8f, 25f), new Vector3(-12f, -3f, 45f),
                new Vector3(25f, -2f, 60f), new Vector3(60f, 2f, 50f), new Vector3(30f, 4f, 95f),
                new Vector3(60f, 6f, 85f), new Vector3(-5f, -10f, 75f),
            };
            for (int i = 0; i < positions.Length; i++)
            {
                var puff = CreateVisual(PrimitiveType.Sphere, "Cloud_" + (i + 1), clouds,
                    positions[i], new Vector3(14f, 3f, 9f), cloud);
                CreateVisual(PrimitiveType.Sphere, "Puff", puff.transform,
                    new Vector3(0.25f, 0.6f, 0.1f), new Vector3(0.5f, 1.2f, 0.6f), cloud);
                puff.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        static void CreateStageManager(PlayerController player)
        {
            var manager = new GameObject("StageManager").AddComponent<StageManager>();
            SetReference(manager, "player", player);
        }

        static void CreateHUD()
        {
            var canvasObject = new GameObject("HUD");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var timeText = CreateText(canvasObject.transform, "TimeText", new Vector2(0f, 1f),
                new Vector2(40f, -30f), new Vector2(600f, 70f), 48f, TextAlignmentOptions.TopLeft);
            var coinText = CreateText(canvasObject.transform, "CoinText", new Vector2(0f, 1f),
                new Vector2(40f, -95f), new Vector2(600f, 70f), 48f, TextAlignmentOptions.TopLeft);
            var hint = CreateText(canvasObject.transform, "HintText", new Vector2(0f, 0f),
                new Vector2(40f, 30f), new Vector2(900f, 50f), 28f, TextAlignmentOptions.BottomLeft);
            hint.text = "WASD Move   SPACE Jump x2   SHIFT Dash   R Restart   ESC Mouse";
            hint.color = new Color(1f, 1f, 1f, 0.8f);

            var panel = new GameObject("ClearPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasObject.transform, false);
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(900f, 520f);
            panel.GetComponent<Image>().color = new Color(0.05f, 0.1f, 0.2f, 0.75f);

            var clearText = CreateText(panel.transform, "ClearText", new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(860f, 480f), 72f, TextAlignmentOptions.Center);
            clearText.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            var hud = canvasObject.AddComponent<StageHUD>();
            SetReference(hud, "timeText", timeText);
            SetReference(hud, "coinText", coinText);
            SetReference(hud, "clearPanel", panel);
            SetReference(hud, "clearText", clearText);
        }

        static TextMeshProUGUI CreateText(Transform parent, string name, Vector2 anchor, Vector2 position,
            Vector2 size, float fontSize, TextAlignmentOptions alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            var rect = (RectTransform)textObject.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Bold;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }
    }
}
