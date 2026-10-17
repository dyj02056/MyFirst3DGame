using UnityEditor;
using UnityEngine;

namespace SkyHopper.EditorTools
{
    /// <summary>
    /// 스테이지 4 "수비대의 섬" — 순찰병, 추적병, 가시 (GAME_DESIGN.md 7번 표)
    ///
    /// 코스 순서 (앞쪽 +Z 방향, 섬 사이 틈 4m)
    ///   ① 시작 섬 → ② 순찰병 1마리 → ③ 체크포인트 1 + 가시 줄(가운데만 빈 길) + 순찰병
    ///   → ④ 넓은 섬의 추적병 → ⑤ 체크포인트 2 + 순찰병 2마리 → ⑥ 추적병 2마리 + 가시 줄 + 골
    ///
    /// 별 3개: ② 위 높은 기둥 / ④ 왼쪽 작은 섬 / ⑥ 가운데 공중 (더블점프)
    /// </summary>
    public static class Stage4Builder
    {
        [MenuItem("Sky Hopper/스테이지 4 만들기", priority = 3)]
        static void BuildFromMenu() => Build(askUser: true);

        internal static void Build(bool askUser)
        {
            var kit = StageBuildKit.Begin("Stage4", 3, askUser);
            if (kit == null)
                return;

            // ① 시작 섬
            kit.Island("01_Start", new Vector3(0f, 0f, 0f), new Vector2(12f, 12f));
            kit.CoinLine(new Vector3(0f, 1f, -2f), new Vector3(0f, 1f, 4f), 3);

            // ② 순찰병 1마리 (밟거나 피해서 지나가기)
            kit.Island("02_Patrol", new Vector3(0f, 0f, 16f), new Vector2(12f, 12f));
            kit.Patrol(new Vector3(-4f, 0f, 16f), new Vector3(8f, 0f, 0f));
            kit.CoinArc(new Vector3(0f, 1f, 6f), new Vector3(0f, 1f, 10f), 1.8f, 2);
            kit.CoinLine(new Vector3(-3f, 1f, 12f), new Vector3(3f, 1f, 12f), 3);

            // 별 1: ② 오른쪽 뒤의 3.2m 기둥 (더블점프)
            kit.Island("Star1_Pillar", new Vector3(4.5f, 3.2f, 20.5f), new Vector2(2f, 2f));
            kit.Star(new Vector3(4.5f, 4.4f, 20.5f));

            // ③ 체크포인트 1 + 가운데만 빈 가시 줄 + 그 뒤를 지나는 순찰병
            kit.Island("03_Checkpoint1_Spikes", new Vector3(0f, 0f, 32f), new Vector2(12f, 12f));
            kit.Checkpoint(new Vector3(0f, 0f, 28f), 0f);
            kit.SpikeRow(new Vector3(-5f, 0f, 33f), new Vector3(2f, 0f, 0f), 2);
            kit.SpikeRow(new Vector3(3f, 0f, 33f), new Vector3(2f, 0f, 0f), 2);
            kit.Patrol(new Vector3(-4f, 0f, 36f), new Vector3(8f, 0f, 0f), 2.5f);
            kit.CoinLine(new Vector3(0f, 1f, 31f), new Vector3(0f, 1f, 35f), 3);

            // ④ 넓은 섬의 추적병 (도망치거나 점프해서 밟기)
            kit.Island("04_Chaser", new Vector3(0f, 0f, 50f), new Vector2(16f, 16f));
            kit.Chaser(new Vector3(0f, 0f, 53f));
            kit.Spikes(new Vector3(-4f, 0f, 47f));
            kit.Spikes(new Vector3(4f, 0f, 47f));
            kit.CoinLine(new Vector3(-6f, 1f, 55f), new Vector3(6f, 1f, 55f), 5);

            // 별 2: ④ 왼쪽 작은 섬
            kit.Island("Star2_Islet", new Vector3(-11f, 0f, 50f), new Vector2(3f, 3f));
            kit.Star(new Vector3(-11f, 1.5f, 50f));

            // ⑤ 체크포인트 2 + 서로 반대로 움직이는 빠른 순찰병 2마리
            kit.Island("05_Checkpoint2_Patrols", new Vector3(0f, 0f, 68f), new Vector2(12f, 12f));
            kit.Checkpoint(new Vector3(-4.5f, 0f, 64f), 0f);
            kit.Patrol(new Vector3(-4f, 0f, 68f), new Vector3(8f, 0f, 0f), 3f);
            kit.Patrol(new Vector3(4f, 0f, 72f), new Vector3(-8f, 0f, 0f), 3f);
            kit.CoinLine(new Vector3(4f, 1f, 64f), new Vector3(4f, 1f, 73f), 4);

            // ⑥ 가시 줄 + 추적병 2마리가 지키는 골
            kit.Island("06_Goal_Guards", new Vector3(0f, 0f, 85f), new Vector2(14f, 14f));
            kit.SpikeRow(new Vector3(-6f, 0f, 81f), new Vector3(2f, 0f, 0f), 3);
            kit.SpikeRow(new Vector3(2f, 0f, 81f), new Vector3(2f, 0f, 0f), 3);
            kit.Chaser(new Vector3(-3f, 0f, 88f));
            kit.Chaser(new Vector3(3f, 0f, 88f));
            kit.CoinArc(new Vector3(0f, 1f, 74f), new Vector3(0f, 1f, 78f), 1.8f, 2);

            // 별 3: ⑥ 가운데 3.2m 공중 (더블점프)
            kit.Star(new Vector3(0f, 3.2f, 85f));

            kit.Goal(new Vector3(0f, 0f, 90.5f), 0f);

            kit.Clouds(
                new Vector3(-18f, -6f, 12f), new Vector3(16f, -8f, 30f), new Vector3(-20f, -3f, 60f),
                new Vector3(20f, -2f, 75f), new Vector3(-12f, 2f, 95f), new Vector3(18f, 4f, 100f));

            kit.Finish(new Vector3(0f, 0.1f, -3f), 0f);
        }
    }
}
