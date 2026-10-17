using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkyHopper
{
    /// <summary>씬 이동을 한곳에서 처리 (일시정지 상태를 풀고 이동).</summary>
    public static class SceneLoader
    {
        public const string TitleScene = "Title";

        /// <summary>타이틀로 돌아갈 때 바로 스테이지 선택 화면을 열지</summary>
        public static bool OpenStageSelectOnTitle { get; set; }

        // Play 버튼을 누를 때마다 초기화 (이 프로젝트는 Play 시 스크립트 다시 불러오기가 꺼져 있음)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            OpenStageSelectOnTitle = false;
        }

        public static void LoadTitle(bool openStageSelect = false)
        {
            OpenStageSelectOnTitle = openStageSelect;
            Load(TitleScene);
        }

        public static void LoadStage(string sceneName) => Load(sceneName);

        public static void Restart() => Load(SceneManager.GetActiveScene().name);

        static void Load(string sceneName)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
    }
}
