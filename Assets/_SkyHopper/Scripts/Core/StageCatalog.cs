using UnityEngine;

namespace SkyHopper
{
    /// <summary>
    /// 스테이지 목록 (스테이지 선택 화면과 "다음 스테이지" 버튼이 사용).
    /// 새 스테이지 씬을 만들면 Assets/_SkyHopper/Data/StageCatalog 에서 Scene Name 칸에 씬 이름을 적어 주세요.
    /// </summary>
    [CreateAssetMenu(menuName = "Sky Hopper/Stage Catalog", fileName = "StageCatalog")]
    public class StageCatalog : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            [Tooltip("씬 파일 이름 (예: Stage1). 비워 두면 \"준비 중\"으로 표시")]
            public string sceneName;
            [Tooltip("화면에 보이는 스테이지 이름")]
            public string displayName;
        }

        [SerializeField] Entry[] stages = new Entry[0];

        public int Count => stages.Length;

        public Entry Get(int index) => stages[index];

        public int IndexOf(string sceneName)
        {
            for (int i = 0; i < stages.Length; i++)
                if (stages[i].sceneName == sceneName)
                    return i;
            return -1;
        }

        /// <summary>씬이 만들어져 있고 빌드 목록에 들어 있는지</summary>
        public bool IsPlayable(int index) =>
            !string.IsNullOrEmpty(stages[index].sceneName) && Application.CanStreamedLevelBeLoaded(stages[index].sceneName);

        /// <summary>첫 스테이지는 처음부터 열려 있고, 나머지는 이전 스테이지를 클리어하면 열림</summary>
        public bool IsUnlocked(int index) => index == 0 || SaveData.IsCleared(stages[index - 1].sceneName);
    }
}
