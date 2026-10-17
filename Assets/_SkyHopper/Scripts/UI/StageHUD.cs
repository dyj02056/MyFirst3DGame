using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SkyHopper
{
    /// <summary>게임 화면 위 글자(시간, 코인 수)와 클리어 화면.</summary>
    public class StageHUD : MonoBehaviour
    {
        [SerializeField] TMP_Text timeText;
        [SerializeField] TMP_Text coinText;
        [Tooltip("별이 있는 스테이지에서만 보이는 상자 (비워 둬도 됨)")]
        [SerializeField] GameObject starBox;
        [SerializeField] TMP_Text starText;

        [Header("클리어 화면")]
        [SerializeField] GameObject clearPanel;
        [SerializeField] TMP_Text clearText;
        [SerializeField] Button nextButton;
        [SerializeField] Button retryButton;
        [SerializeField] Button stageSelectButton;

        void Awake()
        {
            clearPanel.SetActive(false);
            nextButton.onClick.AddListener(() =>
            {
                string next = StageManager.Instance != null ? StageManager.Instance.NextStageScene : null;
                if (next != null)
                    SceneLoader.LoadStage(next);
            });
            retryButton.onClick.AddListener(SceneLoader.Restart);
            stageSelectButton.onClick.AddListener(() => SceneLoader.LoadTitle(openStageSelect: true));
        }

        void Update()
        {
            var stage = StageManager.Instance;
            if (stage == null)
                return;

            timeText.text = "시간  " + SaveData.FormatTime(stage.ElapsedTime);
            coinText.text = $"코인  {stage.CoinCount} / {stage.TotalCoins}";

            bool hasStars = stage.TotalStars > 0;
            if (starBox != null && starBox.activeSelf != hasStars)
                starBox.SetActive(hasStars);
            if (starText != null && hasStars)
                starText.text = $"별  {stage.StarCount} / {stage.TotalStars}";

            if (stage.IsCleared && !clearPanel.activeSelf)
                ShowClear(stage);
        }

        void ShowClear(StageManager stage)
        {
            clearPanel.SetActive(true);
            nextButton.interactable = stage.NextStageScene != null;

            string best = stage.IsNewBest
                ? "<color=#FFD54A>최고 기록 달성!</color>"
                : "최고 기록  " + SaveData.FormatTime(stage.BestTime);
            string collected = $"코인  {stage.CoinCount} / {stage.TotalCoins}";
            if (stage.TotalStars > 0)
                collected += $"      별  {stage.StarCount} / {stage.TotalStars}";
            clearText.text =
                "스테이지 클리어!\n" +
                $"<size=60%>클리어 시간  {SaveData.FormatTime(stage.ElapsedTime)}\n{best}\n{collected}</size>";
        }
    }
}
