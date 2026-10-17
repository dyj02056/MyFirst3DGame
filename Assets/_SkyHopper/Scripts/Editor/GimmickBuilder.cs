using UnityEditor;
using UnityEngine;
using static SkyHopper.EditorTools.SceneBuilderUtility;

namespace SkyHopper.EditorTools
{
    /// <summary>
    /// 상단 메뉴 "Sky Hopper > 3주차 장치 테스트 맵 만들기"
    ///  1) 장치 5종을 프리팹으로 저장 (Assets/_SkyHopper/Prefabs/Gimmicks) → 나중에 스테이지에 끌어다 놓아 재사용
    ///  2) 장치를 하나씩 체험하는 테스트 맵 생성
    ///
    /// 코스 순서 (앞쪽 +Z 방향)
    ///   시작 섬 → 움직이는 발판 → 섬 B (옆에 엘리베이터) → 떨어지는 발판 3개
    ///   → 섬 C (가시 줄 넘기) → 섬 D (회전 막대 피하기, 점프대) → 높은 섬 E
    /// </summary>
    public static class GimmickBuilder
    {
        const string ScenePath = SceneFolder + "/GimmickTest.unity";
        const string PrefabFolder = "Assets/_SkyHopper/Prefabs/Gimmicks";
        const string HitSoundPath = "Assets/SourceFiles/SoundFX/SFX_Respawn.ogg";

        [MenuItem("Sky Hopper/3주차 장치 테스트 맵 만들기", priority = 101)]
        static void Build()
        {
            if (!TryCreateNewScene(ScenePath, "장치 테스트 맵 만들기", out var scene))
                return;

            var movingPlatform = SavePrefab(BuildMovingPlatform());
            var fallingPlatform = SavePrefab(BuildFallingPlatform());
            var jumpPad = SavePrefab(BuildJumpPad());
            var spikes = SavePrefab(BuildSpikeStrip());
            var rotatingBar = SavePrefab(BuildRotatingBar());

            CreateLighting();
            var islands = new GameObject("Islands").transform;
            var gimmicks = new GameObject("Gimmicks").transform;
            var checkpoints = new GameObject("Checkpoints").transform;

            // 시작 섬
            CreateIsland(islands, "A_Start", new Vector3(0f, 0f, 0f), new Vector2(12f, 12f));

            // 움직이는 발판: 앞뒤로 12m 왕복하며 섬 A와 섬 B를 이어 줌
            var mover = Place(movingPlatform, gimmicks, new Vector3(0f, 0f, 9f));
            SetVector3(mover.GetComponent<MovingPlatform>(), "moveOffset", new Vector3(0f, 0f, 12f));

            CreateIsland(islands, "B_AfterMover", new Vector3(0f, 0f, 30f), new Vector2(12f, 12f));
            CreateCheckpoint(checkpoints, new Vector3(0f, 0f, 27f), Quaternion.identity);

            // 엘리베이터 (위아래로 움직이는 발판) → 오른쪽 높은 섬
            var elevator = Place(movingPlatform, gimmicks, new Vector3(8.5f, 0f, 30f));
            elevator.name = "SH_MovingPlatform (Elevator)";
            SetVector3(elevator.GetComponent<MovingPlatform>(), "moveOffset", new Vector3(0f, 5f, 0f));
            CreateIsland(islands, "B_Ledge", new Vector3(14f, 5f, 30f), new Vector2(6f, 6f));

            // 떨어지는 발판 3개
            Place(fallingPlatform, gimmicks, new Vector3(0f, 0f, 39f));
            Place(fallingPlatform, gimmicks, new Vector3(0f, 0f, 43.5f));
            Place(fallingPlatform, gimmicks, new Vector3(0f, 0f, 48f));

            // 가시 줄: 섬을 가로질러 깔려 있어서 점프로 넘어야 함
            CreateIsland(islands, "C_Spikes", new Vector3(0f, 0f, 56f), new Vector2(12f, 12f));
            CreateCheckpoint(checkpoints, new Vector3(0f, 0f, 52f), Quaternion.identity);
            for (int i = 0; i < 6; i++)
                Place(spikes, gimmicks, new Vector3(-5f + i * 2f, 0f, 56f));

            // 회전 막대 + 점프대
            CreateIsland(islands, "D_Bar_JumpPad", new Vector3(0f, 0f, 72f), new Vector2(14f, 14f));
            CreateCheckpoint(checkpoints, new Vector3(-5.5f, 0f, 66.5f), Quaternion.identity);
            Place(rotatingBar, gimmicks, new Vector3(0f, 0f, 70f));
            Place(jumpPad, gimmicks, new Vector3(0f, 0f, 77f));

            // 점프대로만 올라갈 수 있는 5m 높이의 섬
            CreateIsland(islands, "E_High", new Vector3(0f, 5f, 86f), new Vector2(10f, 10f));

            var player = CreatePlayer(new Vector3(0f, 0.1f, -3f), Quaternion.identity);
            CreateCamera(player);

            SaveScene(scene, ScenePath);
            Selection.activeGameObject = player.gameObject;
            Debug.Log("[Sky Hopper] 장치 프리팹 5종과 테스트 맵을 만들었습니다: " + PrefabFolder + ", " + ScenePath);
        }

        // ───────────── 프리팹 만들기 ─────────────

        static GameObject BuildMovingPlatform()
        {
            var root = new GameObject("SH_MovingPlatform");
            AddKinematicBody(root);
            root.AddComponent<MovingPlatform>();
            CreateBlock(root.transform, "Platform", new Vector3(0f, -0.3f, 0f), new Vector3(4f, 0.6f, 4f),
                LoadMaterial("Material_Simple_AquaMarine"));
            return root;
        }

        static GameObject BuildFallingPlatform()
        {
            var root = new GameObject("SH_FallingPlatform");
            AddKinematicBody(root);
            root.AddComponent<FallingPlatform>();
            CreateBlock(root.transform, "Platform", new Vector3(0f, -0.3f, 0f), new Vector3(3f, 0.6f, 3f),
                LoadMaterial("Material_Simple_Orange"));

            // 금이 간 무늬로 "떨어질 것 같은 발판"임을 알려 줌
            var crack = GetOrCreateColorMaterial("SH_Crack", new Color(0.35f, 0.2f, 0.1f));
            var crackA = CreateVisual(PrimitiveType.Cube, "Crack_A", root.transform,
                new Vector3(0f, 0.01f, 0f), new Vector3(2.4f, 0.02f, 0.12f), crack);
            crackA.transform.localRotation = Quaternion.Euler(0f, 30f, 0f);
            var crackB = CreateVisual(PrimitiveType.Cube, "Crack_B", root.transform,
                new Vector3(0.3f, 0.01f, -0.2f), new Vector3(1.4f, 0.02f, 0.12f), crack);
            crackB.transform.localRotation = Quaternion.Euler(0f, -50f, 0f);
            return root;
        }

        static GameObject BuildJumpPad()
        {
            var root = new GameObject("SH_JumpPad");
            var pad = root.AddComponent<JumpPad>();

            var collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.15f, 0f);
            collider.size = new Vector3(2.2f, 0.3f, 2.2f);

            CreateVisual(PrimitiveType.Cylinder, "Base", root.transform,
                new Vector3(0f, 0.05f, 0f), new Vector3(2.2f, 0.05f, 2.2f), DarkMetal());
            var spring = CreateVisual(PrimitiveType.Cylinder, "Spring", root.transform,
                new Vector3(0f, 0.2f, 0f), new Vector3(1.7f, 0.1f, 1.7f), LoadMaterial("Material_Simple_Purple"));

            SetReference(pad, "springVisual", spring.transform);
            SetReference(pad, "launchSound", LoadAsset<AudioClip>(PickupSoundPath));
            return root;
        }

        static GameObject BuildSpikeStrip()
        {
            var root = new GameObject("SH_Spikes");
            var trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0.35f, 0f);
            trigger.size = new Vector3(2f, 0.7f, 2f);

            var hazard = root.AddComponent<Hazard>();
            SetReference(hazard, "hitSound", LoadAsset<AudioClip>(HitSoundPath));

            CreateVisual(PrimitiveType.Cube, "Base", root.transform,
                new Vector3(0f, 0.05f, 0f), new Vector3(2f, 0.1f, 2f), DarkMetal());
            var red = LoadMaterial("Material_Simple_Red");
            for (int x = -1; x <= 1; x++)
            for (int z = -1; z <= 1; z++)
            {
                // 모서리가 위로 오게 기울인 정육면체 = 뾰족한 가시
                var spike = CreateVisual(PrimitiveType.Cube, "Spike", root.transform,
                    new Vector3(x * 0.6f, 0.3f, z * 0.6f), Vector3.one * 0.38f, red);
                spike.transform.localRotation = Quaternion.Euler(45f, 0f, 35.26f);
            }
            return root;
        }

        static GameObject BuildRotatingBar()
        {
            var root = new GameObject("SH_RotatingBar");
            AddKinematicBody(root);
            var rotator = root.AddComponent<Rotator>();

            // 가운데 기둥은 단단한 벽
            var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "Post";
            post.transform.SetParent(root.transform, false);
            post.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            post.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f);
            post.GetComponent<Renderer>().sharedMaterial = LoadMaterial("Material_Simple_Yellow");

            // 막대는 닿으면 밀쳐 내는 트리거 (점프해서 넘어야 함)
            var bar = CreateBlock(root.transform, "Bar", new Vector3(0f, 0.7f, 0f), new Vector3(10f, 0.4f, 0.4f),
                LoadMaterial("Material_Simple_Red"));
            bar.GetComponent<BoxCollider>().isTrigger = true;
            var knockback = bar.AddComponent<KnockbackZone>();
            SetReference(knockback, "rotator", rotator);
            SetReference(knockback, "hitSound", LoadAsset<AudioClip>(HitSoundPath));
            return root;
        }

        static Material DarkMetal() => GetOrCreateColorMaterial("SH_DarkMetal", new Color(0.2f, 0.2f, 0.24f));

        static GameObject SavePrefab(GameObject source) => SceneBuilderUtility.SavePrefab(source, PrefabFolder);
    }
}
