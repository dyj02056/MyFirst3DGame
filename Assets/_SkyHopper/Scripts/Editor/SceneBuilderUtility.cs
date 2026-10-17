using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace SkyHopper.EditorTools
{
    /// <summary>테스트 맵과 스테이지 생성 메뉴가 함께 쓰는 도우미 함수 모음.</summary>
    internal static class SceneBuilderUtility
    {
        internal const string SceneFolder = "Assets/_SkyHopper/Scenes";
        const string MaterialFolder = "Assets/_SkyHopper/Materials";

        /// <summary>
        /// 새 빈 씬을 엽니다. 사용자가 취소하면 false.
        /// askUser가 false면 확인 창 없이 바로 만듦 ("모든 스테이지 다시 만들기"에서 처음에 한 번만 물어볼 때)
        /// </summary>
        internal static bool TryCreateNewScene(string scenePath, string title, out Scene scene, bool askUser = true)
        {
            scene = default;
            if (askUser)
            {
                if (File.Exists(scenePath) &&
                    !EditorUtility.DisplayDialog(title,
                        Path.GetFileNameWithoutExtension(scenePath) + " 씬이 이미 있습니다. 지우고 새로 만들까요?",
                        "새로 만들기", "취소"))
                    return false;

                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    return false;
            }

            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            return true;
        }

        internal static void SaveScene(Scene scene, string scenePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(scenePath));
            EditorSceneManager.SaveScene(scene, scenePath);
            AddSceneToBuildSettings(scenePath);
        }

        internal static void CreateLighting()
        {
            var lightObject = new GameObject("Directional Light");
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            RenderSettings.sun = light;

            var sky = AssetDatabase.LoadAssetAtPath<Material>("Assets/Skyboxes/SkyDay.mat");
            if (sky != null)
                RenderSettings.skybox = sky;

            // 라이트 베이크 없이도 어둡지 않도록 단순한 3색 환경광 사용
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.75f, 0.95f);
            RenderSettings.ambientEquatorColor = new Color(0.55f, 0.6f, 0.65f);
            RenderSettings.ambientGroundColor = new Color(0.35f, 0.33f, 0.3f);
        }

        internal static PlayerController CreatePlayer(Vector3 position, Quaternion rotation)
        {
            var player = new GameObject("Player");
            player.transform.SetPositionAndRotation(position, rotation);

            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.4f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.stepOffset = 0.4f;
            controller.slopeLimit = 45f;

            player.AddComponent<PlayerInputHandler>();
            var playerController = player.AddComponent<PlayerController>();

            // 임시 모양: 캡슐 몸통 + 앞쪽을 알려 주는 노란 코 (나중에 캐릭터 모델로 교체)
            CreateVisual(PrimitiveType.Capsule, "Body", player.transform,
                new Vector3(0f, 0.9f, 0f), new Vector3(0.8f, 0.9f, 0.8f), LoadMaterial("Material_Simple_Pink"));
            CreateVisual(PrimitiveType.Cube, "Nose", player.transform,
                new Vector3(0f, 1.35f, 0.38f), new Vector3(0.3f, 0.15f, 0.2f), LoadMaterial("Material_Simple_Yellow"));

            // 소리·효과 + 대시할 때 뒤에 남는 분홍 꼬리
            var feedback = player.AddComponent<PlayerFeedback>();
            SetReference(feedback, "dashTrail", CreateDashTrail(player.transform));

            return playerController;
        }

        static TrailRenderer CreateDashTrail(Transform player)
        {
            var trailObject = new GameObject("DashTrail");
            trailObject.transform.SetParent(player, false);
            trailObject.transform.localPosition = new Vector3(0f, 0.9f, 0f);

            var trail = trailObject.AddComponent<TrailRenderer>();
            trail.time = 0.25f;
            trail.minVertexDistance = 0.1f;
            trail.widthCurve = AnimationCurve.Linear(0f, 0.9f, 1f, 0f);
            var colors = new Gradient();
            colors.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.55f, 0.8f), 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) });
            trail.colorGradient = colors;
            trail.shadowCastingMode = ShadowCastingMode.Off;
            trail.emitting = false;
            trail.sharedMaterial = GetOrCreateTrailMaterial();
            return trail;
        }

        /// <summary>꼬리용 반투명 머티리얼 (템플릿의 파티클 머티리얼을 복사해 그림만 뺌)</summary>
        static Material GetOrCreateTrailMaterial()
        {
            const string path = MaterialFolder + "/SH_Trail.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            var source = AssetDatabase.LoadAssetAtPath<Material>("Assets/SourceFiles/VFX/GlowMaterial.mat");
            if (source == null)
                return null;
            material = new Material(source);
            material.SetTexture("_BaseMap", null);
            Directory.CreateDirectory(MaterialFolder);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        internal static void CreateCamera(PlayerController player)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 500f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.SetPositionAndRotation(
                player.transform.position + player.transform.rotation * new Vector3(0f, 3.5f, -6f),
                player.transform.rotation * Quaternion.Euler(20f, 0f, 0f));

            var thirdPersonCamera = cameraObject.AddComponent<ThirdPersonCamera>();
            SetReference(thirdPersonCamera, "target", player.transform);
            SetReference(thirdPersonCamera, "input", player.GetComponent<PlayerInputHandler>());
            SetReference(player, "cameraTransform", cameraObject.transform);
        }

        /// <summary>스크립트의 [SerializeField] 칸에 오브젝트를 연결 (Inspector에서 끌어다 놓는 것과 같음)</summary>
        internal static void SetReference(Object component, string fieldName, Object value)
        {
            var serialized = new SerializedObject(component);
            serialized.FindProperty(fieldName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static GameObject CreateBlock(Transform parent, string name, Vector3 center, Vector3 size,
            Material material)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.localPosition = center;
            block.transform.localScale = size;
            if (material != null)
                block.GetComponent<Renderer>().sharedMaterial = material;
            return block;
        }

        /// <summary>바닥(높이 -1)부터 topHeight까지 솟은 기둥</summary>
        internal static GameObject CreatePillar(Transform parent, string name, Vector3 basePosition,
            float topHeight, Vector2 footprint, Material material)
        {
            float height = topHeight + 1f;
            var center = new Vector3(basePosition.x, topHeight - height / 2f, basePosition.z);
            return CreateBlock(parent, name, center, new Vector3(footprint.x, height, footprint.y), material);
        }

        /// <summary>하늘섬 하나: 잔디 윗면 + 아래로 좁아지는 바위 2단. topCenter는 윗면 가운데</summary>
        internal static Transform CreateIsland(Transform parent, string name, Vector3 topCenter, Vector2 size)
        {
            var grass = LoadMaterial("Material_Grass");
            var rock = GetOrCreateColorMaterial("SH_Rock", new Color(0.47f, 0.36f, 0.28f));

            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = topCenter;

            CreateBlock(root, "Top", new Vector3(0f, -0.5f, 0f), new Vector3(size.x, 1f, size.y), grass);
            CreateBlock(root, "Rock", new Vector3(0f, -2.5f, 0f), new Vector3(size.x * 0.8f, 3f, size.y * 0.8f), rock);
            CreateBlock(root, "Rock_Bottom", new Vector3(0f, -5f, 0f), new Vector3(size.x * 0.45f, 2f, size.y * 0.45f), rock);
            return root;
        }

        internal const string PickupSoundPath = "Assets/SourceFiles/SoundFX/SFX_PositiveSound01.ogg";

        /// <summary>체크포인트 깃발. rotation의 앞쪽(+Z)이 리스폰 후 플레이어가 바라보는 방향</summary>
        internal static void CreateCheckpoint(Transform parent, Vector3 position, Quaternion rotation)
        {
            var checkpoint = new GameObject("Checkpoint");
            checkpoint.transform.SetParent(parent, false);
            checkpoint.transform.SetPositionAndRotation(position, rotation);

            var trigger = checkpoint.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 2f, 0f);
            trigger.size = new Vector3(4f, 4f, 4f);

            // 깃대는 진행 방향 왼쪽에 세움
            CreateVisual(PrimitiveType.Cylinder, "Pole", checkpoint.transform,
                new Vector3(-1.5f, 1.5f, 0f), new Vector3(0.15f, 1.5f, 0.15f), LoadMaterial("Material_Simple_Yellow"));
            var flag = CreateVisual(PrimitiveType.Cube, "Flag", checkpoint.transform,
                new Vector3(-1.5f, 2.6f, 0.5f), new Vector3(0.05f, 0.6f, 1f), LoadMaterial("Material_Simple_Red"));

            var checkpointScript = checkpoint.AddComponent<Checkpoint>();
            SetReference(checkpointScript, "flagRenderer", flag.GetComponent<Renderer>());
            SetReference(checkpointScript, "activeMaterial", LoadMaterial("Material_Simple_Green"));
        }

        /// <summary>충돌 없이 보이기만 하는 모양</summary>
        internal static GameObject CreateVisual(PrimitiveType type, string name, Transform parent,
            Vector3 localPosition, Vector3 localScale, Material material)
        {
            var visual = GameObject.CreatePrimitive(type);
            visual.name = name;
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = localPosition;
            visual.transform.localScale = localScale;
            if (material != null)
                visual.GetComponent<Renderer>().sharedMaterial = material;
            return visual;
        }

        /// <summary>템플릿에 들어 있는 Assets/Materials 의 머티리얼을 불러옴</summary>
        internal static Material LoadMaterial(string name)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/" + name + ".mat");
            if (material == null)
                Debug.LogWarning("[Sky Hopper] 머티리얼을 찾지 못했습니다: " + name);
            return material;
        }

        /// <summary>단색 머티리얼을 Assets/_SkyHopper/Materials 에 만들거나, 이미 있으면 불러옴</summary>
        internal static Material GetOrCreateColorMaterial(string name, Color color)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogWarning("[Sky Hopper] URP Lit 셰이더를 찾지 못했습니다.");
                return null;
            }

            material = new Material(shader);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.15f);
            Directory.CreateDirectory(MaterialFolder);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        /// <summary>움직이는 충돌체에는 Kinematic Rigidbody를 붙이는 것이 물리 엔진에 효율적</summary>
        internal static void AddKinematicBody(GameObject target)
        {
            var body = target.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        /// <summary>씬에 만든 오브젝트를 프리팹 파일로 저장하고, 씬에서는 지움</summary>
        internal static GameObject SavePrefab(GameObject source, string folder)
        {
            Directory.CreateDirectory(folder);
            var prefab = PrefabUtility.SaveAsPrefabAsset(source, folder + "/" + source.name + ".prefab");
            Object.DestroyImmediate(source);
            return prefab;
        }

        /// <summary>프리팹을 씬에 배치 (프리팹과 연결된 상태 유지)</summary>
        internal static GameObject Place(GameObject prefab, Transform parent, Vector3 position)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.position = position;
            return instance;
        }

        internal static void SetVector3(Object component, string fieldName, Vector3 value)
        {
            var serialized = new SerializedObject(component);
            serialized.FindProperty(fieldName).vector3Value = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void SetFloat(Object component, string fieldName, float value)
        {
            var serialized = new SerializedObject(component);
            serialized.FindProperty(fieldName).floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>스테이지 목록의 index번째 칸에 씬 이름을 적어 "준비 중"을 해제</summary>
        internal static void RegisterStageInCatalog(int index, string sceneName)
        {
            var catalog = GetOrCreateStageCatalog();
            var serialized = new SerializedObject(catalog);
            var stages = serialized.FindProperty("stages");
            if (index >= stages.arraySize)
                stages.arraySize = index + 1;
            var entry = stages.GetArrayElementAtIndex(index).FindPropertyRelative("sceneName");
            if (entry.stringValue == sceneName)
                return;
            entry.stringValue = sceneName;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }

        internal static T LoadAsset<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
                Debug.LogWarning("[Sky Hopper] 파일을 찾지 못했습니다: " + path);
            return asset;
        }

        static void AddSceneToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == path))
                return;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        /// <summary>게임을 실행하면 가장 먼저 열리는 씬(목록 맨 위)으로 지정</summary>
        internal static void MoveSceneToFront(string path)
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != path).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        internal const string StageCatalogPath = "Assets/_SkyHopper/Data/StageCatalog.asset";

        /// <summary>스테이지 목록 파일을 불러오거나, 없으면 기획서의 스테이지 5개로 새로 만듦</summary>
        internal static StageCatalog GetOrCreateStageCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<StageCatalog>(StageCatalogPath);
            if (catalog != null)
                return catalog;

            catalog = ScriptableObject.CreateInstance<StageCatalog>();
            Directory.CreateDirectory(Path.GetDirectoryName(StageCatalogPath));
            AssetDatabase.CreateAsset(catalog, StageCatalogPath);

            // GAME_DESIGN.md "7. 스테이지 구성" 표. 아직 없는 스테이지는 씬 이름을 비워 둠 ("준비 중")
            var entries = new (string scene, string name)[]
            {
                ("Stage1", "첫 번째 섬"),
                ("", "바람의 다리"),
                ("", "떠도는 발판"),
                ("", "수비대의 섬"),
                ("", "하늘 꼭대기"),
            };
            var serialized = new SerializedObject(catalog);
            var stages = serialized.FindProperty("stages");
            stages.arraySize = entries.Length;
            for (int i = 0; i < entries.Length; i++)
            {
                var element = stages.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("sceneName").stringValue = entries[i].scene;
                element.FindPropertyRelative("displayName").stringValue = entries[i].name;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return catalog;
        }

        internal static void SetReferenceArray(Object component, string fieldName, Object[] values)
        {
            var serialized = new SerializedObject(component);
            var array = serialized.FindProperty(fieldName);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
