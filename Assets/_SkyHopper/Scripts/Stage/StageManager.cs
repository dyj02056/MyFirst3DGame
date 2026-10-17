using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SkyHopper
{
    /// <summary>
    /// 스테이지 진행 관리: 타이머, 코인 수, 클리어 처리, 최고 기록 저장, R 키로 다시 시작.
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        [SerializeField] PlayerController player;

        public float ElapsedTime { get; private set; }
        public int CoinCount { get; private set; }
        public int TotalCoins { get; private set; }
        public bool IsCleared { get; private set; }
        /// <summary>저장된 최고 기록 (초). 기록이 없으면 0</summary>
        public float BestTime { get; private set; }
        public bool IsNewBest { get; private set; }

        string BestTimeKey => "BestTime_" + SceneManager.GetActiveScene().name;

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
            BestTime = PlayerPrefs.GetFloat(BestTimeKey, 0f);
        }

        void Update()
        {
            if (!IsCleared)
                ElapsedTime += Time.deltaTime;

            var keyboard = Keyboard.current;
            // 다시 시작 (씬이 Build Profiles의 씬 목록에 들어 있어야 동작)
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

            if (BestTime <= 0f || ElapsedTime < BestTime)
            {
                IsNewBest = true;
                BestTime = ElapsedTime;
                PlayerPrefs.SetFloat(BestTimeKey, BestTime);
                PlayerPrefs.Save();
            }

            // 클리어 후에는 조작을 멈춤
            if (player != null)
                player.enabled = false;
        }
    }
}
