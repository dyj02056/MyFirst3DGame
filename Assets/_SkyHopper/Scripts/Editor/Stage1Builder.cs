using UnityEditor;
using UnityEngine;

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
        [MenuItem("Sky Hopper/스테이지 1 만들기", priority = 0)]
        static void BuildFromMenu() => Build(askUser: true);

        internal static void Build(bool askUser)
        {
            var kit = StageBuildKit.Begin("Stage1", 0, askUser);
            if (kit == null)
                return;

            // ① 시작 섬: 걸으면서 코인 줍기
            kit.Island("01_Start", new Vector3(0f, 0f, 0f), new Vector2(16f, 16f));
            kit.CoinLine(new Vector3(0f, 1f, -1f), new Vector3(0f, 1f, 5f), 4);

            // ② 3m 틈을 건너며 1m씩 올라가는 작은 섬
            kit.Island("02_Hop_A", new Vector3(0f, 0f, 14f), new Vector2(6f, 6f));
            kit.Island("02_Hop_B", new Vector3(0f, 1f, 23f), new Vector2(6f, 6f));
            kit.Island("02_Hop_C", new Vector3(0f, 2f, 32f), new Vector2(6f, 6f));
            kit.Coin(new Vector3(0f, 2f, 9.5f));
            kit.Coin(new Vector3(0f, 1f, 14f));
            kit.Coin(new Vector3(0f, 3f, 18.5f));
            kit.Coin(new Vector3(0f, 2f, 23f));
            kit.Coin(new Vector3(0f, 4f, 27.5f));
            kit.Coin(new Vector3(0f, 3f, 32f));

            // ③ 체크포인트 1 (다음 길은 오른쪽 +X 방향)
            kit.Island("03_Checkpoint1", new Vector3(0f, 2f, 44f), new Vector2(12f, 12f));
            kit.Checkpoint(new Vector3(0f, 2f, 41f), 90f);
            kit.Coin(new Vector3(-3f, 3f, 46f));
            kit.Coin(new Vector3(3f, 3f, 46f));

            // ④ 1.5m씩 올라가는 계단 섬
            kit.Island("04_Step_A", new Vector3(10f, 3.5f, 44f), new Vector2(4f, 4f));
            kit.Island("04_Step_B", new Vector3(15f, 5f, 44f), new Vector2(4f, 4f));
            kit.Island("04_Step_C", new Vector3(20f, 6.5f, 44f), new Vector2(4f, 4f));
            kit.Coin(new Vector3(10f, 4.5f, 44f));
            kit.Coin(new Vector3(15f, 6f, 44f));
            kit.Coin(new Vector3(20f, 7.5f, 44f));

            // ⑤ 7m 틈: 더블점프 연습 (틈 위 코인은 더블점프 궤적을 따라 배치)
            kit.Island("05_DoubleJump", new Vector3(33f, 6.5f, 44f), new Vector2(8f, 8f));
            kit.Coin(new Vector3(24.5f, 8.5f, 44f));
            kit.Coin(new Vector3(26.5f, 9f, 44f));
            kit.Coin(new Vector3(33f, 7.5f, 44f));

            // ⑥ 3.2m 높은 벽 위의 체크포인트 2 (다음 길은 앞쪽 +Z 방향)
            kit.Island("06_HighWall_Checkpoint2", new Vector3(43f, 9.7f, 44f), new Vector2(12f, 12f));
            kit.Checkpoint(new Vector3(40.5f, 9.7f, 44f), 0f);
            kit.Coin(new Vector3(43f, 10.7f, 47f));
            kit.Coin(new Vector3(46f, 10.7f, 47f));

            // ⑦ 점점 넓어지는 틈: 4m → 5m → 6m
            kit.Island("07_Gap4", new Vector3(43f, 9.7f, 56f), new Vector2(4f, 4f));
            kit.Island("07_Gap5", new Vector3(43f, 10.5f, 65f), new Vector2(4f, 4f));
            kit.Island("07_Gap6", new Vector3(43f, 10.5f, 75f), new Vector2(4f, 4f));
            kit.Coin(new Vector3(43f, 11.7f, 52f));
            kit.Coin(new Vector3(43f, 10.7f, 56f));
            kit.Coin(new Vector3(43f, 12.5f, 61f));
            kit.Coin(new Vector3(43f, 11.5f, 65f));
            kit.Coin(new Vector3(43f, 12.8f, 70.5f));
            kit.Coin(new Vector3(43f, 11.5f, 75f));

            // ⑧ 골
            kit.Island("08_Goal", new Vector3(43f, 10.5f, 85f), new Vector2(10f, 10f));
            kit.Goal(new Vector3(43f, 10.5f, 87f), 0f);

            kit.Clouds(
                new Vector3(-18f, -6f, 10f), new Vector3(15f, -8f, 25f), new Vector3(-12f, -3f, 45f),
                new Vector3(25f, -2f, 60f), new Vector3(60f, 2f, 50f), new Vector3(30f, 4f, 95f),
                new Vector3(60f, 6f, 85f), new Vector3(-5f, -10f, 75f));

            kit.Finish(new Vector3(0f, 0.1f, -4f), 0f);
        }
    }
}
