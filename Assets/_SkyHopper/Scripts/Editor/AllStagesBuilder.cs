using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SkyHopper.EditorTools
{
    /// <summary>상단 메뉴 "Sky Hopper > 모든 스테이지 다시 만들기": 스테이지 1~5를 한 번에 다시 만듭니다.</summary>
    public static class AllStagesBuilder
    {
        [MenuItem("Sky Hopper/모든 스테이지 다시 만들기", priority = 20)]
        static void BuildAll()
        {
            if (!EditorUtility.DisplayDialog("모든 스테이지 다시 만들기",
                    "스테이지 1~5를 모두 지우고 새로 만듭니다. 계속할까요?", "다시 만들기", "취소"))
                return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            Stage1Builder.Build(askUser: false);
            Stage2Builder.Build(askUser: false);
            Stage3Builder.Build(askUser: false);
            Stage4Builder.Build(askUser: false);
            Stage5Builder.Build(askUser: false);

            // 다 만든 뒤 바로 Play로 확인할 수 있게 타이틀 씬을 열어 둠
            string titlePath = SceneBuilderUtility.SceneFolder + "/" + SceneLoader.TitleScene + ".unity";
            if (File.Exists(titlePath))
                EditorSceneManager.OpenScene(titlePath);

            Debug.Log("[Sky Hopper] 스테이지 1~5를 모두 다시 만들었습니다.");
        }
    }
}
