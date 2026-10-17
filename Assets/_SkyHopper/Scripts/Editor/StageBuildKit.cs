using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static SkyHopper.EditorTools.SceneBuilderUtility;

namespace SkyHopper.EditorTools
{
    /// <summary>
    /// 스테이지를 만들 때 쓰는 도구 상자. 스테이지 빌더는 이 도구들로 섬, 코인, 별, 장치, 적을 배치합니다.
    ///
    ///   var kit = StageBuildKit.Begin("Stage2", 1, askUser);   // 새 씬 시작 (1 = 스테이지 목록의 두 번째 칸)
    ///   kit.Island(...); kit.Coin(...); kit.JumpPad(...); ...
    ///   kit.Finish(시작 위치, 바라보는 방향);                 // 플레이어·카메라·화면 UI 추가 후 저장
    /// </summary>
    internal class StageBuildKit
    {
        const string GimmickFolder = "Assets/_SkyHopper/Prefabs/Gimmicks/";
        const string EnemyFolder = "Assets/_SkyHopper/Prefabs/Enemies/";

        readonly Scene scene;
        readonly string sceneName;
        readonly string scenePath;
        readonly int catalogIndex;

        readonly Transform islands, coins, stars, checkpoints, gimmicks, enemies, clouds;
        readonly Material coinMaterial, goldTrim, starMaterial, cloudMaterial;
        readonly AudioClip pickupSound;

        /// <summary>새 스테이지 씬을 시작. 사용자가 취소하면 null</summary>
        internal static StageBuildKit Begin(string sceneName, int catalogIndex, bool askUser)
        {
            string path = SceneFolder + "/" + sceneName + ".unity";
            if (!TryCreateNewScene(path, sceneName + " 만들기", out var scene, askUser))
                return null;
            CreateLighting();
            return new StageBuildKit(scene, sceneName, path, catalogIndex);
        }

        StageBuildKit(Scene scene, string sceneName, string scenePath, int catalogIndex)
        {
            this.scene = scene;
            this.sceneName = sceneName;
            this.scenePath = scenePath;
            this.catalogIndex = catalogIndex;

            islands = Group("Islands");
            coins = Group("Coins");
            stars = Group("Stars");
            checkpoints = Group("Checkpoints");
            gimmicks = Group("Gimmicks");
            enemies = Group("Enemies");
            clouds = Group("Clouds");

            coinMaterial = LoadMaterial("Material_GoldGlow");
            goldTrim = LoadMaterial("Material_Simple_Yellow");
            starMaterial = GetOrCreateColorMaterial("SH_Star", new Color(1f, 0.84f, 0.2f));
            cloudMaterial = GetOrCreateColorMaterial("SH_Cloud", new Color(0.97f, 0.98f, 1f));
            pickupSound = LoadAsset<AudioClip>(PickupSoundPath);
        }

        static Transform Group(string name) => new GameObject(name).transform;

        // ───────────── 지형 ─────────────

        /// <summary>하늘섬. top = 윗면 가운데, size = 가로(X) × 세로(Z)</summary>
        public void Island(string name, Vector3 top, Vector2 size) => CreateIsland(islands, name, top, size);

        public void Clouds(params Vector3[] positions)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                var puff = CreateVisual(PrimitiveType.Sphere, "Cloud_" + (i + 1), clouds,
                    positions[i], new Vector3(14f, 3f, 9f), cloudMaterial);
                CreateVisual(PrimitiveType.Sphere, "Puff", puff.transform,
                    new Vector3(0.25f, 0.6f, 0.1f), new Vector3(0.5f, 1.2f, 0.6f), cloudMaterial);
                puff.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        // ───────────── 모으는 것 ─────────────

        public void Coin(Vector3 position)
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

        /// <summary>from부터 to까지 일직선으로 count개</summary>
        public void CoinLine(Vector3 from, Vector3 to, int count)
        {
            for (int i = 0; i < count; i++)
                Coin(Vector3.Lerp(from, to, count == 1 ? 0f : i / (float)(count - 1)));
        }

        /// <summary>from과 to 사이에 포물선(점프 궤적) 모양으로 count개. height = 가운데가 솟는 높이</summary>
        public void CoinArc(Vector3 from, Vector3 to, float height, int count)
        {
            for (int i = 1; i <= count; i++)
            {
                float t = i / (float)(count + 1);
                Coin(Vector3.Lerp(from, to, t) + Vector3.up * (4f * height * t * (1f - t)));
            }
        }

        /// <summary>별: 다섯 갈래로 뻗은 노란 별 모양</summary>
        public void Star(Vector3 position)
        {
            var star = new GameObject("Star");
            star.transform.SetParent(stars, false);
            star.transform.localPosition = position;

            var trigger = star.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.8f;

            var starScript = star.AddComponent<SkyHopper.Star>();
            SetReference(starScript, "pickupSound", pickupSound);

            CreateVisual(PrimitiveType.Sphere, "Core", star.transform, Vector3.zero, new Vector3(0.5f, 0.5f, 0.3f), starMaterial);
            for (int i = 0; i < 5; i++)
            {
                // 72도씩 돌린 방향마다, 세로로 길게 늘린 마름모 = 별의 뾰족한 끝
                var arm = new GameObject("Arm_" + (i + 1)).transform;
                arm.SetParent(star.transform, false);
                arm.localRotation = Quaternion.Euler(0f, 0f, i * 72f);
                arm.localPosition = arm.localRotation * new Vector3(0f, 0.35f, 0f);
                arm.localScale = new Vector3(1f, 2.4f, 1f);
                var point = CreateVisual(PrimitiveType.Cube, "Point", arm, Vector3.zero,
                    new Vector3(0.22f, 0.22f, 0.18f), starMaterial);
                point.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            }
        }

        // ───────────── 진행 ─────────────

        /// <summary>yaw = 리스폰 후 바라볼 방향 (0 = 앞쪽 +Z, 90 = 오른쪽 +X)</summary>
        public void Checkpoint(Vector3 position, float yaw) =>
            CreateCheckpoint(checkpoints, position, Quaternion.Euler(0f, yaw, 0f));

        /// <summary>골 게이트. yaw = 게이트가 바라보는 방향 (플레이어가 들어오는 쪽의 반대)</summary>
        public void Goal(Vector3 position, float yaw)
        {
            var goal = new GameObject("Goal");
            goal.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

            CreateBlock(goal.transform, "Pillar_Left", new Vector3(-2f, 2f, 0f), new Vector3(0.6f, 4f, 0.6f), goldTrim);
            CreateBlock(goal.transform, "Pillar_Right", new Vector3(2f, 2f, 0f), new Vector3(0.6f, 4f, 0.6f), goldTrim);
            CreateBlock(goal.transform, "Beam", new Vector3(0f, 4.3f, 0f), new Vector3(4.6f, 0.6f, 0.6f), goldTrim);
            CreateVisual(PrimitiveType.Cube, "Glow", goal.transform,
                new Vector3(0f, 2f, 0f), new Vector3(3.4f, 4f, 0.05f), coinMaterial);

            var trigger = goal.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 2f, 0f);
            trigger.size = new Vector3(3.4f, 4f, 1.5f);

            var goalScript = goal.AddComponent<SkyHopper.Goal>();
            SetReference(goalScript, "clearSound", pickupSound);
        }

        // ───────────── 장치 (3주차 프리팹) ─────────────

        public void MovingPlatform(Vector3 position, Vector3 moveOffset, float speed = 2.5f)
        {
            var platform = PlacePrefab(GimmickFolder + "SH_MovingPlatform.prefab", gimmicks, position);
            if (platform == null)
                return;
            var mover = platform.GetComponent<SkyHopper.MovingPlatform>();
            SetVector3(mover, "moveOffset", moveOffset);
            SetFloat(mover, "speed", speed);
        }

        public void FallingPlatform(Vector3 position) =>
            PlacePrefab(GimmickFolder + "SH_FallingPlatform.prefab", gimmicks, position);

        public void JumpPad(Vector3 position, float launchHeight = 7f)
        {
            var pad = PlacePrefab(GimmickFolder + "SH_JumpPad.prefab", gimmicks, position);
            if (pad != null)
                SetFloat(pad.GetComponent<SkyHopper.JumpPad>(), "launchHeight", launchHeight);
        }

        /// <summary>가시 2×2m 한 칸</summary>
        public void Spikes(Vector3 position) => PlacePrefab(GimmickFolder + "SH_Spikes.prefab", gimmicks, position);

        /// <summary>가시를 first부터 step 간격으로 count칸</summary>
        public void SpikeRow(Vector3 first, Vector3 step, int count)
        {
            for (int i = 0; i < count; i++)
                Spikes(first + step * i);
        }

        /// <summary>회전 막대 (길이 10m). degreesPerSecond가 음수면 반대로 돎</summary>
        public void RotatingBar(Vector3 position, float degreesPerSecond = 90f)
        {
            var bar = PlacePrefab(GimmickFolder + "SH_RotatingBar.prefab", gimmicks, position);
            if (bar != null)
                SetFloat(bar.GetComponent<Rotator>(), "degreesPerSecond", degreesPerSecond);
        }

        // ───────────── 적 (4주차 프리팹) ─────────────

        public void Patrol(Vector3 position, Vector3 moveOffset, float speed = 2f)
        {
            var enemy = PlacePrefab(EnemyFolder + "SH_PatrolEnemy.prefab", enemies, position);
            if (enemy == null)
                return;
            var patrol = enemy.GetComponent<PatrolEnemy>();
            SetVector3(patrol, "moveOffset", moveOffset);
            SetFloat(patrol, "speed", speed);
        }

        /// <summary>yaw = 처음에 바라보는 방향 (180 = 시작 지점 쪽)</summary>
        public void Chaser(Vector3 position, float yaw = 180f)
        {
            var enemy = PlacePrefab(EnemyFolder + "SH_ChaserEnemy.prefab", enemies, position);
            if (enemy != null)
                enemy.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        static GameObject PlacePrefab(string path, Transform parent, Vector3 position)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError("[Sky Hopper] 프리팹이 없습니다: " + path +
                               "  → 먼저 Sky Hopper 메뉴의 3주차/4주차 테스트 맵 만들기를 실행해 주세요.");
                return null;
            }
            return Place(prefab, parent, position);
        }

        // ───────────── 마무리 ─────────────

        /// <summary>플레이어·카메라·화면 UI를 넣고 저장, 스테이지 목록에 등록</summary>
        public void Finish(Vector3 spawnPosition, float spawnYaw)
        {
            var player = CreatePlayer(spawnPosition, Quaternion.Euler(0f, spawnYaw, 0f));
            CreateCamera(player);
            StageUIBuilder.CreateStageSystems(player);

            SaveScene(scene, scenePath);
            RegisterStageInCatalog(catalogIndex, sceneName);
            Selection.activeGameObject = player.gameObject;
            Debug.Log($"[Sky Hopper] {sceneName}을(를) 만들었습니다 (코인 {coins.childCount}개, 별 {stars.childCount}개): {scenePath}");
        }
    }
}
