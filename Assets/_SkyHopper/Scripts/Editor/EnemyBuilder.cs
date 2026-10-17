using UnityEditor;
using UnityEngine;
using static SkyHopper.EditorTools.SceneBuilderUtility;

namespace SkyHopper.EditorTools
{
    /// <summary>
    /// 상단 메뉴 "Sky Hopper > 4주차 적 테스트 맵 만들기"
    ///  1) 적 2종을 프리팹으로 저장 (Assets/_SkyHopper/Prefabs/Enemies)
    ///  2) 적을 체험하는 테스트 맵 생성
    ///
    /// 코스 순서 (앞쪽 +Z 방향)
    ///   시작 섬 → 섬 B (순찰병 1마리) → 섬 C (순찰병 2마리)
    ///   → 넓은 섬 D (추적병 1마리) → 섬 E (추적병 2마리)
    /// </summary>
    public static class EnemyBuilder
    {
        const string ScenePath = SceneFolder + "/EnemyTest.unity";
        const string PrefabFolder = "Assets/_SkyHopper/Prefabs/Enemies";

        [MenuItem("Sky Hopper/4주차 적 테스트 맵 만들기", priority = 102)]
        static void Build()
        {
            if (!TryCreateNewScene(ScenePath, "적 테스트 맵 만들기", out var scene))
                return;

            var patrol = SavePrefab(BuildPatrolEnemy(), PrefabFolder);
            var chaser = SavePrefab(BuildChaserEnemy(), PrefabFolder);

            CreateLighting();
            var islands = new GameObject("Islands").transform;
            var enemies = new GameObject("Enemies").transform;
            var checkpoints = new GameObject("Checkpoints").transform;

            CreateIsland(islands, "A_Start", new Vector3(0f, 0f, 0f), new Vector2(12f, 12f));

            // 순찰병 1마리: 길을 가로질러 왕복 → 밟거나 피해서 지나가기
            CreateIsland(islands, "B_Patrol", new Vector3(0f, 0f, 16f), new Vector2(12f, 12f));
            var p1 = Place(patrol, enemies, new Vector3(-4f, 0f, 16f));
            SetVector3(p1.GetComponent<PatrolEnemy>(), "moveOffset", new Vector3(8f, 0f, 0f));

            // 순찰병 2마리: 서로 반대 방향으로 왕복
            CreateIsland(islands, "C_TwoPatrols", new Vector3(0f, 0f, 32f), new Vector2(12f, 12f));
            CreateCheckpoint(checkpoints, new Vector3(0f, 0f, 27.5f), Quaternion.identity);
            var p2 = Place(patrol, enemies, new Vector3(-4f, 0f, 31f));
            SetVector3(p2.GetComponent<PatrolEnemy>(), "moveOffset", new Vector3(8f, 0f, 0f));
            var p3 = Place(patrol, enemies, new Vector3(4f, 0f, 35f));
            SetVector3(p3.GetComponent<PatrolEnemy>(), "moveOffset", new Vector3(-8f, 0f, 0f));

            // 추적병 1마리: 넓은 섬에서 쫓아옴 → 도망치거나 점프해서 밟기
            CreateIsland(islands, "D_Chaser", new Vector3(0f, 0f, 50f), new Vector2(16f, 16f));
            CreateCheckpoint(checkpoints, new Vector3(-6f, 0f, 43.5f), Quaternion.identity);
            Place(chaser, enemies, new Vector3(0f, 0f, 53f)).transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            // 추적병 2마리
            CreateIsland(islands, "E_TwoChasers", new Vector3(0f, 0f, 68f), new Vector2(12f, 12f));
            CreateCheckpoint(checkpoints, new Vector3(0f, 0f, 63.5f), Quaternion.identity);
            // (체크포인트에서 다시 시작하자마자 발견되지 않도록 8m 넘게 떨어뜨려 둠)
            Place(chaser, enemies, new Vector3(-3f, 0f, 71.5f)).transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            Place(chaser, enemies, new Vector3(3f, 0f, 71.5f)).transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            var player = CreatePlayer(new Vector3(0f, 0.1f, -3f), Quaternion.identity);
            CreateCamera(player);
            FeedbackBuilder.CreateGameFeedback(null); // 효과음·파티클 (테스트 맵은 배경음악 없음)

            SaveScene(scene, ScenePath);
            Selection.activeGameObject = player.gameObject;
            Debug.Log("[Sky Hopper] 적 프리팹 2종과 테스트 맵을 만들었습니다: " + PrefabFolder + ", " + ScenePath);
        }

        static GameObject BuildPatrolEnemy()
        {
            var root = new GameObject("SH_PatrolEnemy");
            AddKinematicBody(root);
            AddTrigger(root, new Vector3(1.2f, 1.1f, 1.2f));
            root.AddComponent<PatrolEnemy>();

            var body = GetOrCreateColorMaterial("SH_EnemyPatrol", new Color(0.9f, 0.32f, 0.25f));
            CreateVisual(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0f, 0.55f, 0f), new Vector3(1.2f, 1f, 1.2f), body);
            CreateEyes(root.transform, 0.75f, 0.55f, 0f);
            CreateFeet(root.transform);
            return root;
        }

        static GameObject BuildChaserEnemy()
        {
            var root = new GameObject("SH_ChaserEnemy");
            AddKinematicBody(root);
            AddTrigger(root, new Vector3(1.4f, 1.3f, 1.4f));
            var enemy = root.AddComponent<ChaserEnemy>();

            var body = GetOrCreateColorMaterial("SH_EnemyChaser", new Color(0.5f, 0.28f, 0.8f));
            var yellow = LoadMaterial("Material_Simple_Yellow");
            CreateVisual(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0f, 0.65f, 0f), new Vector3(1.4f, 1.2f, 1.4f), body);
            CreateEyes(root.transform, 0.85f, 0.64f, 15f); // 눈을 기울여 화난 표정
            CreateFeet(root.transform);

            // 뿔
            var hornL = CreateVisual(PrimitiveType.Cube, "Horn_L", root.transform,
                new Vector3(-0.35f, 1.3f, 0f), new Vector3(0.15f, 0.4f, 0.15f), yellow);
            hornL.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
            var hornR = CreateVisual(PrimitiveType.Cube, "Horn_R", root.transform,
                new Vector3(0.35f, 1.3f, 0f), new Vector3(0.15f, 0.4f, 0.15f), yellow);
            hornR.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);

            // 추적 중에만 보이는 "!"
            var alert = new GameObject("Alert");
            alert.transform.SetParent(root.transform, false);
            CreateVisual(PrimitiveType.Cube, "Bar", alert.transform,
                new Vector3(0f, 2.25f, 0f), new Vector3(0.18f, 0.5f, 0.18f), yellow);
            CreateVisual(PrimitiveType.Cube, "Dot", alert.transform,
                new Vector3(0f, 1.82f, 0f), new Vector3(0.18f, 0.18f, 0.18f), yellow);
            SetReference(enemy, "alertVisual", alert);
            return root;
        }

        static void AddTrigger(GameObject root, Vector3 size)
        {
            var trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, size.y / 2f, 0f);
            trigger.size = size;
        }

        static void CreateEyes(Transform root, float height, float forward, float tilt)
        {
            var white = GetOrCreateColorMaterial("SH_White", Color.white);
            var black = GetOrCreateColorMaterial("SH_Black", new Color(0.05f, 0.05f, 0.05f));
            for (int side = -1; side <= 1; side += 2)
            {
                var eye = CreateVisual(PrimitiveType.Cube, side < 0 ? "Eye_L" : "Eye_R", root,
                    new Vector3(0.25f * side, height, forward), new Vector3(0.25f, 0.3f, 0.1f), white);
                eye.transform.localRotation = Quaternion.Euler(0f, 0f, tilt * side);
                CreateVisual(PrimitiveType.Cube, "Pupil", eye.transform,
                    new Vector3(0f, -0.1f, 0.6f), new Vector3(0.5f, 0.5f, 0.5f), black);
            }
        }

        static void CreateFeet(Transform root)
        {
            var dark = GetOrCreateColorMaterial("SH_DarkMetal", new Color(0.2f, 0.2f, 0.24f));
            CreateVisual(PrimitiveType.Cube, "Foot_L", root, new Vector3(-0.3f, 0.08f, 0.05f), new Vector3(0.35f, 0.16f, 0.5f), dark);
            CreateVisual(PrimitiveType.Cube, "Foot_R", root, new Vector3(0.3f, 0.08f, 0.05f), new Vector3(0.35f, 0.16f, 0.5f), dark);
        }
    }
}
