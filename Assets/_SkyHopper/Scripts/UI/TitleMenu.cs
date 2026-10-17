using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SkyHopper
{
    /// <summary>타이틀 화면: 시작하기(→ 스테이지 선택), 설정, 종료.</summary>
    public class TitleMenu : MonoBehaviour
    {
        [SerializeField] StageCatalog catalog;

        [Header("화면")]
        [SerializeField] GameObject mainPanel;
        [SerializeField] GameObject stageSelectPanel;
        [SerializeField] SettingsPanel settingsPanel;

        [Header("버튼")]
        [SerializeField] Button startButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button quitButton;
        [SerializeField] Button backButton;
        [SerializeField] Button[] stageButtons;
        [SerializeField] TMP_Text[] stageLabels;

        void Awake()
        {
            startButton.onClick.AddListener(ShowStageSelect);
            settingsButton.onClick.AddListener(ShowSettings);
            quitButton.onClick.AddListener(Quit);
            backButton.onClick.AddListener(ShowMain);
            settingsPanel.Closed += ShowMain;

            for (int i = 0; i < stageButtons.Length; i++)
            {
                int index = i;
                stageButtons[i].onClick.AddListener(() => SceneLoader.LoadStage(catalog.Get(index).sceneName));
            }

            // 웹 브라우저에서는 "종료"가 의미 없으므로 숨김
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                quitButton.gameObject.SetActive(false);
        }

        void Start()
        {
            Time.timeScale = 1f;
            GameCursor.SetLocked(false);

            if (SceneLoader.OpenStageSelectOnTitle)
            {
                SceneLoader.OpenStageSelectOnTitle = false;
                ShowStageSelect();
            }
            else
            {
                ShowMain();
            }
        }

        void ShowMain() => ShowOnly(mainPanel);

        void ShowSettings()
        {
            ShowOnly(null);
            settingsPanel.Open();
        }

        void ShowStageSelect()
        {
            RefreshStageButtons();
            ShowOnly(stageSelectPanel);
        }

        void ShowOnly(GameObject panel)
        {
            mainPanel.SetActive(panel == mainPanel);
            stageSelectPanel.SetActive(panel == stageSelectPanel);
            if (settingsPanel.gameObject.activeSelf)
                settingsPanel.gameObject.SetActive(false);
        }

        void RefreshStageButtons()
        {
            for (int i = 0; i < stageButtons.Length; i++)
            {
                bool exists = catalog != null && i < catalog.Count;
                stageButtons[i].gameObject.SetActive(exists);
                if (!exists)
                    continue;

                var entry = catalog.Get(i);
                bool playable = catalog.IsPlayable(i);
                bool unlocked = catalog.IsUnlocked(i);
                stageButtons[i].interactable = playable && unlocked;

                string detail;
                if (!playable)
                    detail = "준비 중";
                else if (!unlocked)
                    detail = "잠김 - 이전 스테이지를 클리어하세요";
                else if (SaveData.IsCleared(entry.sceneName))
                    detail = $"최고 기록 {SaveData.FormatTime(SaveData.GetBestTime(entry.sceneName))}    " +
                             $"코인 {SaveData.GetBestCoins(entry.sceneName)} / {SaveData.GetTotalCoins(entry.sceneName)}";
                else
                    detail = "도전해 보세요!";

                stageLabels[i].text = $"{i + 1}. {entry.displayName}\n<size=65%>{detail}</size>";
            }
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
