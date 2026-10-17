using UnityEditor;
using UnityEngine;
using static SkyHopper.EditorTools.SceneBuilderUtility;

namespace SkyHopper.EditorTools
{
    /// <summary>
    /// 상단 메뉴 "Sky Hopper > 1주차 테스트 맵 만들기"로 조작감 테스트용 씬을 자동 생성합니다.
    ///
    /// 발판 색깔의 의미
    ///   초록  : 시작 섬
    ///   노랑  : 일반 점프로 건널 수 있는 틈 (4m)
    ///   주황  : 더블점프가 필요한 틈 (7m)
    ///   빨강  : 더블점프 + 대시가 필요한 틈 (11m)
    ///   하늘  : 일반 점프로 오르는 계단 (1.5m씩)
    ///   보라  : 더블점프로만 오를 수 있는 벽 (3.2m)
    /// </summary>
    public static class TestSceneBuilder
    {
        const string ScenePath = SceneFolder + "/TestMovement.unity";

        [MenuItem("Sky Hopper/1주차 테스트 맵 만들기", priority = 100)]
        static void Build()
        {
            if (!TryCreateNewScene(ScenePath, "테스트 맵 만들기", out var scene))
                return;

            CreateLighting();
            CreateCourse();
            var player = CreatePlayer(new Vector3(0f, 0.1f, -5f), Quaternion.identity);
            CreateCamera(player);
            FeedbackBuilder.CreateGameFeedback(null); // 효과음·파티클 (테스트 맵은 배경음악 없음)

            SaveScene(scene, ScenePath);
            Selection.activeGameObject = player.gameObject;
            Debug.Log("[Sky Hopper] 테스트 맵을 만들었습니다: " + ScenePath + "  → 위쪽 ▶(Play) 버튼을 눌러 보세요.");
        }

        static void CreateCourse()
        {
            var course = new GameObject("Course").transform;

            var grass = LoadMaterial("Material_Grass");
            var yellow = LoadMaterial("Material_Simple_Yellow");
            var orange = LoadMaterial("Material_Simple_Orange");
            var red = LoadMaterial("Material_Simple_Red");
            var sky = LoadMaterial("Material_Simple_BlueLight");
            var purple = LoadMaterial("Material_Simple_Purple");

            // 시작 섬: 20 x 20, 윗면 높이 0
            CreateBlock(course, "Start_Island", new Vector3(0f, -0.5f, 0f), new Vector3(20f, 1f, 20f), grass);

            // 앞쪽(+Z): 틈 건너기 테스트
            CreateBlock(course, "Gap_4m_Jump", new Vector3(0f, -0.5f, 17f), new Vector3(6f, 1f, 6f), yellow);
            CreateBlock(course, "Gap_7m_DoubleJump", new Vector3(0f, -0.5f, 30f), new Vector3(6f, 1f, 6f), orange);
            CreateBlock(course, "Gap_11m_DoubleJump_Dash", new Vector3(0f, -0.5f, 47f), new Vector3(6f, 1f, 6f), red);

            // 오른쪽(+X): 계단 오르기 테스트
            CreatePillar(course, "Step_1.5m", new Vector3(13f, 0f, 0f), 1.5f, new Vector2(6f, 6f), sky);
            CreatePillar(course, "Step_3.0m", new Vector3(19f, 0f, 0f), 3.0f, new Vector2(6f, 6f), sky);
            CreatePillar(course, "Step_4.5m", new Vector3(25f, 0f, 0f), 4.5f, new Vector2(6f, 6f), sky);

            // 왼쪽(-X): 더블점프로만 오를 수 있는 벽
            CreatePillar(course, "Wall_3.2m_DoubleJump", new Vector3(-13f, 0f, 0f), 3.2f, new Vector2(6f, 6f), purple);

            // 뒤쪽(-Z): 카메라 충돌 확인용 터널
            CreateBlock(course, "Tunnel_Left", new Vector3(-2.5f, 1.5f, -14f), new Vector3(1f, 3f, 8f), sky);
            CreateBlock(course, "Tunnel_Right", new Vector3(2.5f, 1.5f, -14f), new Vector3(1f, 3f, 8f), sky);
            CreateBlock(course, "Tunnel_Roof", new Vector3(0f, 3.25f, -14f), new Vector3(6f, 0.5f, 8f), sky);
            CreateBlock(course, "Tunnel_Floor", new Vector3(0f, -0.5f, -15f), new Vector3(6f, 1f, 10f), grass);
        }
    }
}
