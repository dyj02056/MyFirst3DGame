using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SkyHopper
{
    /// <summary>
    /// 스테이지 진행 관리: 타이머, 코인 수, 클리어 처리, 기록 저장, R 키로 다시 시작.
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        [SerializeField] PlayerController player;
        [Tooltip("\"다음 스테이지\" 버튼이 어느 씬으로 갈지 찾는 데 사용")]
        [SerializeField] StageCatalog catalog;

        public float ElapsedTime { get; private set; }
        public int CoinCount { get; private set; }
        public int TotalCoins { get; private set; }
        public bool IsCleared { get; private set; }
        /// <summary>저장된 최고 기록 (초). 기록이 없으면 0</summary>
        public float BestTime { get; private set; }
        public bool IsNewBest { get; private set; }

        /// <summary>다음 스테이지 씬 이름. 없거나 아직 만들어지지 않았으면 null</summary>
        public string NextStageScene
        {
            get
            {
                if (catalog == null)
                    return null;
                int next = catalog.IndexOf(SceneName) + 1;
                if (next <= 0 || next >= catalog.Count || !catalog.IsPlayable(next))
                    return null;
                return catalog.Get(next).sceneName;
            }
        }

        static string SceneName => SceneManager.GetActiveScene().name;

        void Awake()
        {
            Instance = this;
            if (player == null)
                player = FindAnyObjectByType<PlayerController>();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Start()
        {
            TotalCoins = Coin.ActiveCount;
            BestTime = SaveData.GetBestTime(SceneName);
        }

        void Update()
        {
            if (!IsCleared)
                ElapsedTime += Time.deltaTime;

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame && !PauseMenu.IsPaused)
                SceneLoader.Restart();
        }

        public void AddCoin()
        {
            CoinCount++;
        }

        public void CompleteStage()
        {
            if (IsCleared)
                return;
            IsCleared = true;

            IsNewBest = SaveData.RecordClear(SceneName, ElapsedTime, CoinCount, TotalCoins);
            BestTime = SaveData.GetBestTime(SceneName);

            // 클리어 후에는 조작을 멈추고 버튼을 누를 수 있게 커서를 보여 줌
            if (player != null)
                player.enabled = false;
            GameCursor.SetLocked(false);
        }
    }
}
