using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SkyHopper
{
    /// <summary>
    /// Esc(게임패드 Start)로 여는 일시정지 화면: 계속하기, 다시 시작, 설정, 타이틀로.
    /// 일시정지 중에는 시간이 멈추고(Time.timeScale = 0) 마우스 커서가 보입니다.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance { get; private set; }
        public static bool IsPaused => Instance != null && Instance.paused;

        [SerializeField] GameObject pausePanel;
        [SerializeField] SettingsPanel settingsPanel;
        [SerializeField] Button resumeButton;
        [SerializeField] Button restartButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button titleButton;

        bool paused;

        void Awake()
        {
            Instance = this;
            pausePanel.SetActive(false);
            settingsPanel.gameObject.SetActive(false);

            resumeButton.onClick.AddListener(Resume);
            restartButton.onClick.AddListener(SceneLoader.Restart);
            settingsButton.onClick.AddListener(OpenSettings);
            titleButton.onClick.AddListener(() => SceneLoader.LoadTitle(openStageSelect: true));
            settingsPanel.Closed += () => pausePanel.SetActive(true);
        }

        void OnDestroy()
        {
            if (Instance != this)
                return;
            Instance = null;
            Time.timeScale = 1f;
        }

        void Update()
        {
            bool togglePressed = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                                 || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);
            if (!togglePressed)
                return;

            if (settingsPanel.gameObject.activeSelf)
                settingsPanel.Close();
            else if (paused)
                Resume();
            else if (StageManager.Instance == null || !StageManager.Instance.IsCleared)
                Pause();
        }

        public void Pause()
        {
            paused = true;
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
            GameCursor.SetLocked(false);
        }

        public void Resume()
        {
            paused = false;
            Time.timeScale = 1f;
            pausePanel.SetActive(false);
            settingsPanel.gameObject.SetActive(false);
            GameCursor.SetLocked(true);
        }

        void OpenSettings()
        {
            pausePanel.SetActive(false);
            settingsPanel.Open();
        }
    }
}
