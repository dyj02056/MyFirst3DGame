using UnityEditor;
using UnityEngine;

namespace SkyHopper.EditorTools
{
    /// <summary>
    /// 스테이지 2 "바람의 다리" — 대시 익히기 (GAME_DESIGN.md 7번 표)
    ///
    /// 코스 순서 (앞쪽 +Z 방향)
    ///   ① 시작 섬 → ② 좁은 다리 → ③ 6m 틈 (더블점프 또는 대시)
    ///   ④ 체크포인트 1 → ⑤ 9.5m 틈 (점프 + 더블점프 + 대시) → ⑥ 9.5m 틈 + 1m 높이
    ///   ⑦ 체크포인트 2 → ⑧ 1m씩 높아지는 작은 기둥 4개 → ⑨ 골
    ///
    /// 별 3개: 다리 오른쪽 작은 섬 / ⑤ 왼쪽 작은 섬 / 체크포인트 2 위 높은 기둥
    /// </summary>
    public static class Stage2Builder
    {
        [MenuItem("Sky Hopper/스테이지 2 만들기", priority = 1)]
        static void BuildFromMenu() => Build(askUser: true);

        internal static void Build(bool askUser)
        {
            var kit = StageBuildKit.Begin("Stage2", 1, askUser);
            if (kit == null)
                return;

            // ① 시작 섬
            kit.Island("01_Start", new Vector3(0f, 0f, 0f), new Vector2(12f, 12f));
            kit.CoinLine(new Vector3(0f, 1f, -2f), new Vector3(0f, 1f, 4f), 4);

            // ② 폭 3m의 좁은 다리
            kit.Island("02_Bridge", new Vector3(0f, 0f, 14f), new Vector2(3f, 10f));
            kit.CoinArc(new Vector3(0f, 1f, 6f), new Vector3(0f, 1f, 9f), 1.5f, 1);
            kit.CoinLine(new Vector3(0f, 1f, 10.5f), new Vector3(0f, 1f, 17.5f), 4);

            // 별 1: 다리 오른쪽 6m 떨어진 작은 섬 (점프 + 대시)
            kit.Island("Star1_Islet", new Vector3(9f, 0f, 14f), new Vector2(2.5f, 2.5f));
            kit.Star(new Vector3(9f, 1.5f, 14f));

            // ③ 6m 틈: 더블점프나 대시로 건너기
            kit.Island("03_Gap6", new Vector3(0f, 0f, 28f), new Vector2(3f, 6f));
            kit.CoinArc(new Vector3(0f, 1f, 19f), new Vector3(0f, 1f, 25f), 2.5f, 3);

            // ④ 체크포인트 1
            kit.Island("04_Checkpoint1", new Vector3(0f, 0f, 40f), new Vector2(10f, 10f));
            kit.Checkpoint(new Vector3(0f, 0f, 37f), 0f);
            kit.CoinArc(new Vector3(0f, 1f, 31f), new Vector3(0f, 1f, 35f), 1.8f, 2);

            // ⑤ 9.5m 틈: 점프 → 더블점프 → 대시를 모두 써야 함
            kit.Island("05_DashGap", new Vector3(0f, 0f, 57.5f), new Vector2(4f, 6f));
            kit.CoinArc(new Vector3(0f, 1f, 45f), new Vector3(0f, 1f, 54.5f), 3f, 5);

            // 별 2: ⑤ 왼쪽, 6.5m 떨어진 조금 높은 섬
            kit.Island("Star2_Islet", new Vector3(-10f, 1f, 57.5f), new Vector2(3f, 3f));
            kit.Star(new Vector3(-10f, 2.5f, 57.5f));

            // ⑥ 9.5m 틈 + 1m 높이
            kit.Island("06_DashGapUp", new Vector3(0f, 1f, 72f), new Vector2(4f, 4f));
            kit.CoinArc(new Vector3(0f, 1f, 60.5f), new Vector3(0f, 2f, 70f), 3f, 5);

            // ⑦ 체크포인트 2
            kit.Island("07_Checkpoint2", new Vector3(0f, 1f, 84f), new Vector2(10f, 10f));
            kit.Checkpoint(new Vector3(0f, 1f, 81f), 0f);
            kit.CoinArc(new Vector3(0f, 2f, 74f), new Vector3(0f, 2f, 79f), 1.5f, 2);

            // 별 3: 체크포인트 2 위에 솟은 3.5m 기둥 (더블점프로 오르기)
            kit.Island("Star3_Pillar", new Vector3(3.5f, 4.5f, 87f), new Vector2(2f, 2f));
            kit.Star(new Vector3(3.5f, 5.7f, 87f));

            // ⑧ 1m씩 높아지는 작은 기둥 (틈 4m)
            kit.Island("08_Step_A", new Vector3(0f, 1.5f, 94.25f), new Vector2(2.5f, 2.5f));
            kit.Island("08_Step_B", new Vector3(0f, 2.5f, 101f), new Vector2(2.5f, 2.5f));
            kit.Island("08_Step_C", new Vector3(0f, 3.5f, 107.75f), new Vector2(2.5f, 2.5f));
            kit.Island("08_Step_D", new Vector3(0f, 4.5f, 114.5f), new Vector2(2.5f, 2.5f));
            kit.Coin(new Vector3(0f, 2.5f, 94.25f));
            kit.Coin(new Vector3(0f, 3.5f, 101f));
            kit.Coin(new Vector3(0f, 4.5f, 107.75f));
            kit.Coin(new Vector3(0f, 5.5f, 114.5f));

            // ⑨ 골
            kit.Island("09_Goal", new Vector3(0f, 4.5f, 125f), new Vector2(10f, 10f));
            kit.Goal(new Vector3(0f, 4.5f, 127f), 0f);

            kit.Clouds(
                new Vector3(-18f, -6f, 15f), new Vector3(16f, -8f, 35f), new Vector3(-14f, -3f, 70f),
                new Vector3(18f, -2f, 90f), new Vector3(-20f, 2f, 115f), new Vector3(15f, 4f, 130f));

            kit.Finish(new Vector3(0f, 0.1f, -3f), 0f);
        }
    }
}
