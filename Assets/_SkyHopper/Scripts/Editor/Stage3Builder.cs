using UnityEditor;
using UnityEngine;

namespace SkyHopper.EditorTools
{
    /// <summary>
    /// 스테이지 3 "떠도는 발판" — 움직이는 발판, 떨어지는 발판 (GAME_DESIGN.md 7번 표)
    ///
    /// 코스 순서
    ///   ① 시작 섬 → 앞뒤로 움직이는 발판 → ② 체크포인트 1
    ///   → 엘리베이터로 6m 위 ③ 높은 섬 → 떨어지는 발판 3개 → ④ 체크포인트 2
    ///   → 움직이는 발판 → ⑤ 작은 섬 → 내려가는 떨어지는 발판 3개 → ⑥ 골
    ///
    /// 별 3개: ③ 오른쪽 작은 섬 / ④ 왼쪽 아래 작은 섬 / 움직이는 발판 위 공중
    /// </summary>
    public static class Stage3Builder
    {
        [MenuItem("Sky Hopper/스테이지 3 만들기", priority = 2)]
        static void BuildFromMenu() => Build(askUser: true);

        internal static void Build(bool askUser)
        {
            var kit = StageBuildKit.Begin("Stage3", 2, askUser);
            if (kit == null)
                return;

            // ① 시작 섬 → 앞뒤로 10m 움직이는 발판
            kit.Island("01_Start", new Vector3(0f, 0f, 0f), new Vector2(12f, 12f));
            kit.CoinLine(new Vector3(0f, 1f, -2f), new Vector3(0f, 1f, 4f), 3);
            kit.MovingPlatform(new Vector3(0f, 0f, 9f), new Vector3(0f, 0f, 10f));
            kit.CoinLine(new Vector3(0f, 1.2f, 10f), new Vector3(0f, 1.2f, 18f), 4);

            // ② 체크포인트 1
            kit.Island("02_Checkpoint1", new Vector3(0f, 0f, 26f), new Vector2(10f, 10f));
            kit.Checkpoint(new Vector3(0f, 0f, 23f), 0f);

            // 엘리베이터: 6m 위로 (탄 채로 코인 줍기)
            kit.MovingPlatform(new Vector3(7f, 0f, 28f), new Vector3(0f, 6f, 0f), 2f);
            kit.CoinLine(new Vector3(7f, 2f, 28f), new Vector3(7f, 6f, 28f), 3);

            // ③ 높은 섬
            kit.Island("03_High", new Vector3(13f, 6f, 28f), new Vector2(6f, 6f));

            // 별 1: ③ 오른쪽, 조금 높은 작은 섬
            kit.Island("Star1_Islet", new Vector3(21f, 7f, 28f), new Vector2(2.5f, 2.5f));
            kit.Star(new Vector3(21f, 8.5f, 28f));

            // 떨어지는 발판 3개 (밟고 0.6초 안에 다음으로!)
            kit.FallingPlatform(new Vector3(13f, 6f, 33f));
            kit.FallingPlatform(new Vector3(13f, 6f, 37.5f));
            kit.FallingPlatform(new Vector3(13f, 6f, 42f));
            kit.Coin(new Vector3(13f, 7f, 33f));
            kit.Coin(new Vector3(13f, 7f, 37.5f));
            kit.Coin(new Vector3(13f, 7f, 42f));

            // ④ 체크포인트 2
            kit.Island("04_Checkpoint2", new Vector3(13f, 6f, 50f), new Vector2(10f, 10f));
            kit.Checkpoint(new Vector3(13f, 6f, 47f), 0f);

            // 별 2: ④ 왼쪽 3m 아래의 작은 섬 (더블점프로 돌아오기)
            kit.Island("Star2_Islet", new Vector3(4f, 3f, 50f), new Vector2(3f, 3f));
            kit.Star(new Vector3(4f, 4.5f, 50f));

            // 앞뒤로 10m 움직이는 발판 (조금 빠름)
            kit.MovingPlatform(new Vector3(13f, 6f, 59f), new Vector3(0f, 0f, 10f), 3f);
            kit.CoinLine(new Vector3(13f, 7.2f, 60f), new Vector3(13f, 7.2f, 68f), 3);

            // 별 3: 움직이는 발판 위에서 점프해야 닿는 공중
            kit.Star(new Vector3(13f, 9f, 64f));

            // ⑤ 작은 섬
            kit.Island("05_Rest", new Vector3(13f, 6f, 78f), new Vector2(6f, 6f));

            // 1m씩 내려가는 떨어지는 발판 3개
            kit.FallingPlatform(new Vector3(13f, 5f, 85f));
            kit.FallingPlatform(new Vector3(13f, 4f, 89.5f));
            kit.FallingPlatform(new Vector3(13f, 3f, 94f));
            kit.Coin(new Vector3(13f, 6f, 85f));
            kit.Coin(new Vector3(13f, 5f, 89.5f));
            kit.Coin(new Vector3(13f, 4f, 94f));

            // ⑥ 골
            kit.Island("06_Goal", new Vector3(13f, 3f, 103f), new Vector2(10f, 10f));
            kit.Goal(new Vector3(13f, 3f, 105f), 0f);

            kit.Clouds(
                new Vector3(-16f, -6f, 10f), new Vector3(28f, -4f, 20f), new Vector3(-8f, -2f, 45f),
                new Vector3(30f, 2f, 65f), new Vector3(-6f, -4f, 90f), new Vector3(28f, -1f, 105f));

            kit.Finish(new Vector3(0f, 0.1f, -3f), 0f);
        }
    }
}
