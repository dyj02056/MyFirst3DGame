using TMPro;
using UnityEngine;

namespace SkyHopper
{
    /// <summary>
    /// 게임 화면 위 글자: 시간, 코인 수, 클리어 화면.
    /// (기본 글꼴에 한글이 없어서 지금은 영어로 표시합니다. 한글 글꼴은 5주차 UI 작업 때 추가)
    /// </summary>
    public class StageHUD : MonoBehaviour
    {
        [SerializeField] TMP_Text timeText;
        [SerializeField] TMP_Text coinText;
        [SerializeField] GameObject clearPanel;
        [SerializeField] TMP_Text clearText;

        void Start()
        {
            if (clearPanel != null)
                clearPanel.SetActive(false);
        }

        void Update()
        {
            var stage = StageManager.Instance;
            if (stage == null)
                return;

            timeText.text = "TIME  " + FormatTime(stage.ElapsedTime);
            coinText.text = $"COIN  {stage.CoinCount} / {stage.TotalCoins}";

            if (stage.IsCleared && clearPanel != null && !clearPanel.activeSelf)
            {
                clearPanel.SetActive(true);
                string best = stage.IsNewBest ? "NEW BEST!" : "BEST  " + FormatTime(stage.BestTime);
                clearText.text =
                    "STAGE CLEAR!\n\n" +
                    $"<size=70%>TIME  {FormatTime(stage.ElapsedTime)}\n{best}\n" +
                    $"COIN  {stage.CoinCount} / {stage.TotalCoins}\n\n" +
                    "<size=50%>Press R to play again</size>";
            }
        }

        static string FormatTime(float seconds)
        {
            int minutes = (int)(seconds / 60f);
            return $"{minutes:00}:{seconds - minutes * 60f:00.00}";
        }
    }
}
