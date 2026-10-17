using UnityEditor;
using UnityEngine;

namespace SkyHopper.EditorTools
{
    /// <summary>
    /// 스테이지 5 "하늘 꼭대기" — 모든 요소 조합 (GAME_DESIGN.md 7번 표)
    ///
    /// 코스 순서 (점점 높이 올라감)
    ///   ① 시작 섬 → ② 회전 막대 섬 → 점프대로 6m 위 ③ 체크포인트 1
    ///   → 움직이는 발판 → ④ 순찰병 섬 → 떨어지는 발판 2개
    ///   → ⑤ 체크포인트 2 + 반대로 도는 회전 막대 → 점프대로 6m 위 ⑥ 추적병 섬 → ⑦ 골 (14m 높이)
    ///
    /// 별 3개: ② 구석의 높은 기둥 / 움직이는 발판 위 공중 / ⑤ 왼쪽 아래 작은 섬
    /// </summary>
    public static class Stage5Builder
    {
        [MenuItem("Sky Hopper/스테이지 5 만들기", priority = 4)]
        static void BuildFromMenu() => Build(askUser: true);

        internal static void Build(bool askUser)
        {
            var kit = StageBuildKit.Begin("Stage5", 4, askUser);
            if (kit == null)
                return;

            // ① 시작 섬
            kit.Island("01_Start", new Vector3(0f, 0f, 0f), new Vector2(12f, 12f));
            kit.CoinLine(new Vector3(0f, 1f, -2f), new Vector3(0f, 1f, 4f), 3);

            // ② 회전 막대 섬 + 점프대
            kit.Island("02_Bar", new Vector3(0f, 0f, 17f), new Vector2(14f, 14f));
            kit.RotatingBar(new Vector3(0f, 0f, 17f), 100f);
            kit.JumpPad(new Vector3(0f, 0f, 23f), 7f);
            kit.CoinArc(new Vector3(0f, 1f, 6f), new Vector3(0f, 1f, 10f), 1.8f, 2);
            kit.CoinLine(new Vector3(5.5f, 1f, 12f), new Vector3(5.5f, 1f, 22f), 4);

            // 별 1: ② 왼쪽 앞 구석의 3.5m 기둥 (회전 막대가 닿지 않는 곳)
            kit.Island("Star1_Pillar", new Vector3(-5.5f, 3.5f, 12f), new Vector2(2f, 2f));
            kit.Star(new Vector3(-5.5f, 4.7f, 12f));

            // ③ 체크포인트 1 (점프대로만 올라올 수 있는 6m 높이)
            kit.Island("03_Checkpoint1", new Vector3(0f, 6f, 31f), new Vector2(8f, 8f));
            kit.Checkpoint(new Vector3(0f, 6f, 28.5f), 0f);
            kit.CoinLine(new Vector3(0f, 2f, 23f), new Vector3(0f, 8f, 26f), 3);

            // 움직이는 발판
            kit.MovingPlatform(new Vector3(0f, 6f, 39f), new Vector3(0f, 0f, 10f), 2.5f);
            kit.CoinLine(new Vector3(0f, 7.2f, 40f), new Vector3(0f, 7.2f, 48f), 3);

            // 별 2: 움직이는 발판 위에서 점프해야 닿는 공중
            kit.Star(new Vector3(0f, 9f, 44f));

            // ④ 순찰병 섬
            kit.Island("04_Patrol", new Vector3(0f, 6f, 56f), new Vector2(8f, 8f));
            kit.Patrol(new Vector3(-3f, 6f, 56f), new Vector3(6f, 0f, 0f), 2f);

            // 떨어지는 발판 2개
            kit.FallingPlatform(new Vector3(0f, 6f, 64f));
            kit.FallingPlatform(new Vector3(0f, 6f, 68.5f));
            kit.Coin(new Vector3(0f, 7f, 64f));
            kit.Coin(new Vector3(0f, 7f, 68.5f));

            // ⑤ 체크포인트 2 + 반대로 도는 빠른 회전 막대 + 점프대
            kit.Island("05_Checkpoint2_Bar", new Vector3(0f, 6f, 77f), new Vector2(12f, 12f));
            kit.Checkpoint(new Vector3(-4.5f, 6f, 72f), 0f);
            kit.RotatingBar(new Vector3(0f, 6f, 76f), -110f);
            kit.JumpPad(new Vector3(0f, 6f, 81.8f), 7.5f);
            kit.CoinLine(new Vector3(4.5f, 7f, 72f), new Vector3(4.5f, 7f, 81f), 3);

            // 별 3: ⑤ 왼쪽 3m 아래의 작은 섬 (더블점프로 돌아오기)
            kit.Island("Star3_Islet", new Vector3(-9.5f, 3f, 77f), new Vector2(3f, 3f));
            kit.Star(new Vector3(-9.5f, 4.5f, 77f));

            // ⑥ 추적병 섬 (12m 높이)
            kit.Island("06_Chaser", new Vector3(0f, 12f, 90f), new Vector2(8f, 8f));
            kit.Chaser(new Vector3(0f, 12f, 92f));
            kit.CoinLine(new Vector3(0f, 9f, 83f), new Vector3(0f, 14f, 86f), 3);

            // ⑦ 골: 4m 틈 + 2m 높이 (더블점프)
            kit.Island("07_Goal", new Vector3(0f, 14f, 103f), new Vector2(10f, 10f));
            kit.CoinArc(new Vector3(0f, 13f, 94f), new Vector3(0f, 15f, 98f), 2f, 2);
            kit.Goal(new Vector3(0f, 14f, 105f), 0f);

            kit.Clouds(
                new Vector3(-18f, -6f, 15f), new Vector3(16f, -4f, 35f), new Vector3(-16f, 2f, 60f),
                new Vector3(18f, 4f, 80f), new Vector3(-14f, 8f, 100f), new Vector3(16f, 10f, 110f),
                new Vector3(0f, -10f, 60f));

            kit.Finish(new Vector3(0f, 0.1f, -3f), 0f);
        }
    }
}
