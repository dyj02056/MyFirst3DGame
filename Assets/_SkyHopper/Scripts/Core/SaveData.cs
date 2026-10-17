using UnityEngine;

namespace SkyHopper
{
    /// <summary>
    /// 스테이지 기록 저장 (PlayerPrefs 사용: PC는 레지스트리, 웹은 브라우저 저장소에 저장됨).
    /// 스테이지마다 클리어 여부, 최고 기록, 가장 많이 모은 코인 수를 기억합니다.
    /// </summary>
    public static class SaveData
    {
        static string Key(string sceneName, string field) => $"Stage.{sceneName}.{field}";

        public static bool IsCleared(string sceneName) => PlayerPrefs.GetInt(Key(sceneName, "Cleared"), 0) == 1;

        /// <summary>최고 기록 (초). 기록이 없으면 0</summary>
        public static float GetBestTime(string sceneName) => PlayerPrefs.GetFloat(Key(sceneName, "BestTime"), 0f);

        public static int GetBestCoins(string sceneName) => PlayerPrefs.GetInt(Key(sceneName, "BestCoins"), 0);

        public static int GetTotalCoins(string sceneName) => PlayerPrefs.GetInt(Key(sceneName, "TotalCoins"), 0);

        /// <summary>한 번에 가장 많이 모은 별 수</summary>
        public static int GetBestStars(string sceneName) => PlayerPrefs.GetInt(Key(sceneName, "BestStars"), 0);

        public static int GetTotalStars(string sceneName) => PlayerPrefs.GetInt(Key(sceneName, "TotalStars"), 0);

        /// <summary>클리어 결과를 저장하고, 최고 기록을 새로 세웠으면 true</summary>
        public static bool RecordClear(string sceneName, float time, int coins, int totalCoins, int stars, int totalStars)
        {
            float bestTime = GetBestTime(sceneName);
            bool isNewBest = bestTime <= 0f || time < bestTime;

            PlayerPrefs.SetInt(Key(sceneName, "Cleared"), 1);
            if (isNewBest)
                PlayerPrefs.SetFloat(Key(sceneName, "BestTime"), time);
            if (coins > GetBestCoins(sceneName))
                PlayerPrefs.SetInt(Key(sceneName, "BestCoins"), coins);
            if (stars > GetBestStars(sceneName))
                PlayerPrefs.SetInt(Key(sceneName, "BestStars"), stars);
            PlayerPrefs.SetInt(Key(sceneName, "TotalCoins"), totalCoins);
            PlayerPrefs.SetInt(Key(sceneName, "TotalStars"), totalStars);
            PlayerPrefs.Save();
            return isNewBest;
        }

        /// <summary>초를 "분:초.백분의1초" 형식으로 (예: 01:23.45)</summary>
        public static string FormatTime(float seconds)
        {
            int minutes = (int)(seconds / 60f);
            return $"{minutes:00}:{seconds - minutes * 60f:00.00}";
        }
    }
}
