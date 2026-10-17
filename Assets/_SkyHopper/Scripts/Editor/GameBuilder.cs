using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SkyHopper.EditorTools
{
    /// <summary>
    /// 상단 메뉴 "Sky Hopper > 빌드"로 게임을 실행 파일로 만듭니다.
    ///  - Windows 빌드: Builds/Windows/SkyHopper.exe (폴더째 압축해서 친구에게 보내면 됨)
    ///  - 웹 빌드: Builds/WebGL (웹 서버에 올리거나 itch.io 등에 업로드)
    /// 테스트 맵은 빼고 타이틀 + 스테이지 1~5만 넣습니다. Builds 폴더는 git 백업에서 제외되어 있습니다.
    /// </summary>
    public static class GameBuilder
    {
        const string ProductName = "Sky Hopper";
        const string Version = "1.0.0";
        const string WindowsPath = "Builds/Windows/SkyHopper.exe";
        const string WebPath = "Builds/WebGL";

        static readonly string[] SceneNames = { SceneLoader.TitleScene, "Stage1", "Stage2", "Stage3", "Stage4", "Stage5" };

        [MenuItem("Sky Hopper/빌드/Windows 빌드 (exe)", priority = 60)]
        static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, WindowsPath, runAfterBuild: false);

        [MenuItem("Sky Hopper/빌드/웹 빌드 (WebGL)", priority = 61)]
        static void BuildWeb() => Build(BuildTarget.WebGL, WebPath, runAfterBuild: false);

        [MenuItem("Sky Hopper/빌드/웹 빌드 후 브라우저에서 실행", priority = 62)]
        static void BuildWebAndRun() => Build(BuildTarget.WebGL, WebPath, runAfterBuild: true);

        static string[] ScenePaths => SceneNames.Select(name => SceneBuilderUtility.SceneFolder + "/" + name + ".unity").ToArray();

        /// <summary>
        /// 창 없이 명령줄로 웹 빌드 (Unity 편집기를 열지 않고 자동으로 빌드할 때 사용)
        /// Unity.exe -batchmode -quit -projectPath (프로젝트) -executeMethod SkyHopper.EditorTools.GameBuilder.BuildWebFromCommandLine
        /// </summary>
        public static void BuildWebFromCommandLine()
        {
            ApplyPlayerSettings();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = ScenePaths,
                locationPathName = WebPath,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
            });
            Debug.Log($"[Sky Hopper] 명령줄 웹 빌드 결과: {report.summary.result}, 오류 {report.summary.totalErrors}개");
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        static void Build(BuildTarget target, string path, bool runAfterBuild)
        {
            string[] scenes = ScenePaths;
            string[] missing = scenes.Where(scene => !File.Exists(scene)).ToArray();
            if (missing.Length > 0)
            {
                EditorUtility.DisplayDialog("빌드할 수 없음",
                    "아래 씬이 없습니다. 먼저 Sky Hopper 메뉴로 만들어 주세요.\n\n" + string.Join("\n", missing), "확인");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            ApplyPlayerSettings();

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = path,
                target = target,
                targetGroup = BuildPipeline.GetBuildTargetGroup(target),
                options = runAfterBuild ? BuildOptions.AutoRunPlayer : BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                string message = $"{TargetName(target)} 빌드 완료!\n\n" +
                                 $"위치: {Path.GetFullPath(path)}\n" +
                                 $"크기: {summary.totalSize / (1024f * 1024f):0.0} MB\n" +
                                 $"걸린 시간: {summary.totalTime.TotalMinutes:0.0}분";
                Debug.Log("[Sky Hopper] " + message.Replace("\n\n", " / ").Replace("\n", " / "));
                if (!runAfterBuild)
                {
                    EditorUtility.RevealInFinder(path);
                    EditorUtility.DisplayDialog("빌드 완료", message, "확인");
                }
            }
            else
            {
                Debug.LogError($"[Sky Hopper] {TargetName(target)} 빌드 실패: {summary.result}, 오류 {summary.totalErrors}개");
                EditorUtility.DisplayDialog("빌드 실패",
                    $"{TargetName(target)} 빌드에 실패했습니다 (오류 {summary.totalErrors}개).\nConsole 창의 빨간 메시지를 확인해 주세요.", "확인");
            }
        }

        /// <summary>게임 이름, 버전, 화면, 웹 압축 설정</summary>
        static void ApplyPlayerSettings()
        {
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = Version;

            // Windows: 처음엔 전체 화면 창 모드, 창 크기 조절 가능
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false;

            // 웹: gzip 압축 + 압축 해제 대비책 → 서버 설정 없이도 어디에 올려도 동작
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;

            // 웹도 편집기·Windows와 같은 "PC" 그래픽 설정을 사용.
            // 템플릿 기본값인 "Mobile" 설정으로 빌드하면 그림자 텍스처 형식 오류로 3D 물체가 전혀 그려지지 않음
            // (브라우저 콘솔: "Mismatch between texture format and sampler type")
            SetDefaultQualityLevel("WebGL", "PC");
        }

        /// <summary>플랫폼(예: WebGL)이 기본으로 쓸 품질 단계(Project Settings > Quality)를 이름으로 지정</summary>
        static void SetDefaultQualityLevel(string platform, string qualityName)
        {
            int qualityIndex = System.Array.IndexOf(QualitySettings.names, qualityName);
            if (qualityIndex < 0)
            {
                Debug.LogWarning("[Sky Hopper] 품질 단계를 찾지 못했습니다: " + qualityName);
                return;
            }

            var qualityAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset").FirstOrDefault();
            if (qualityAsset == null)
                return;

            var serialized = new SerializedObject(qualityAsset);
            var perPlatform = serialized.FindProperty("m_PerPlatformDefaultQuality");
            for (int i = 0; i < perPlatform.arraySize; i++)
            {
                var pair = perPlatform.GetArrayElementAtIndex(i);
                if (pair.FindPropertyRelative("first").stringValue != platform)
                    continue;
                var level = pair.FindPropertyRelative("second");
                if (level.intValue != qualityIndex)
                {
                    level.intValue = qualityIndex;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    AssetDatabase.SaveAssets();
                }
                return;
            }
        }

        static string TargetName(BuildTarget target) => target == BuildTarget.WebGL ? "웹(WebGL)" : "Windows";
    }
}
