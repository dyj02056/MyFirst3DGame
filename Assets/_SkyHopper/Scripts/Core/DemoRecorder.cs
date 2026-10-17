using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace SkyHopper
{
    /// <summary>
    /// README용 GIF를 만들기 위한 자동 시연 + 화면 녹화.
    /// 게임을 "-sh-demo (저장 폴더)" 옵션으로 실행했을 때만 켜지며, 평소 플레이에는 영향이 없습니다.
    ///
    ///   SkyHopper.exe -screen-fullscreen 0 -screen-width 640 -screen-height 360 -sh-demo C:\frames
    ///
    /// 타이틀 → 스테이지 선택 → 스테이지 1에서 자동으로 달리기·점프·대시·더블점프로
    /// 높은 벽 위 체크포인트 2까지 간 뒤 종료합니다. 매 프레임을 PNG로 저장하고 소리는 끕니다.
    /// (Tools/make_gif.py 로 PNG들을 GIF 하나로 합칩니다.)
    /// </summary>
    [DefaultExecutionOrder(-200)] // PlayerController보다 먼저 입력을 정해야 함
    public class DemoRecorder : MonoBehaviour
    {
        const int CaptureFps = 30;
        const float MaxDuration = 45f;

        enum Phase { Title, StageSelect, Stage, Finished }

        string outputDirectory;
        int frameIndex;
        Phase phase;
        float phaseStartTime;

        PlayerController player;
        int segment;
        bool dashedAtStart;
        readonly bool[] forwardJumpsDone = new bool[4];
        readonly bool[] stepJumpsDone = new bool[3];
        float gapJumpTime = -1f;
        float wallJumpTime = -1f;
        float endTime = -1f;

        // 스테이지 1 코스에서 점프할 위치 (Stage1Builder의 섬 배치 기준)
        static readonly float[] ForwardJumpZ = { 7.5f, 16.5f, 25.5f, 34.5f };
        static readonly float[] StepJumpX = { 5.2f, 11.2f, 16.2f };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void StartIfRequested()
        {
            PlayerInputHandler.DemoMode = false;
            string directory = GetCommandLineValue("-sh-demo");
            if (string.IsNullOrEmpty(directory))
                return;

            var recorderObject = new GameObject("DemoRecorder");
            DontDestroyOnLoad(recorderObject);
            recorderObject.AddComponent<DemoRecorder>().outputDirectory = directory;
            PlayerInputHandler.DemoMode = true;
        }

        static string GetCommandLineValue(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == name)
                    return args[i + 1];
            return null;
        }

        void Start()
        {
            Directory.CreateDirectory(outputDirectory);
            // 실제 시간과 상관없이 매 프레임 1/30초씩 진행 → 녹화가 끊기지 않고 항상 같은 결과
            Time.captureFramerate = CaptureFps;
            phaseStartTime = Time.time;
        }

        void Update()
        {
            AudioListener.volume = 0f;
            PlayerInputHandler.DemoJump = false;
            PlayerInputHandler.DemoDash = false;
            PlayerInputHandler.DemoMove = Vector2.zero;

            float elapsed = Time.time - phaseStartTime;
            switch (phase)
            {
                case Phase.Title:
                    if (elapsed > 2.5f)
                    {
                        ClickButton("StartButton");
                        NextPhase(Phase.StageSelect);
                    }
                    break;

                case Phase.StageSelect:
                    if (elapsed > 1.5f)
                    {
                        SceneLoader.LoadStage("Stage1");
                        NextPhase(Phase.Stage);
                    }
                    break;

                case Phase.Stage:
                    if (player == null)
                        player = FindAnyObjectByType<PlayerController>();
                    else
                        DriveStage1();
                    break;
            }

            ScreenCapture.CaptureScreenshot(Path.Combine(outputDirectory, $"frame_{frameIndex++:00000}.png"));

            if (Time.time > MaxDuration || (endTime > 0f && Time.time > endTime))
                Finish();
        }

        void NextPhase(Phase next)
        {
            phase = next;
            phaseStartTime = Time.time;
        }

        static void ClickButton(string name)
        {
            var buttonObject = GameObject.Find(name);
            if (buttonObject != null && buttonObject.TryGetComponent<Button>(out var button))
                button.onClick.Invoke();
        }

        /// <summary>스테이지 1: 앞(+Z)으로 섬 4개 → 체크포인트 1에서 오른쪽(+X) → 계단 → 7m 틈 → 높은 벽</summary>
        void DriveStage1()
        {
            Vector3 p = player.transform.position;
            bool grounded = player.IsGrounded;

            if (segment == 0)
            {
                // 가운데 줄(x = 0)을 따라 앞으로
                PlayerInputHandler.DemoMove = new Vector2(Mathf.Clamp(-p.x * 0.8f, -1f, 1f), 1f);
                if (!dashedAtStart && p.z > -1f)
                {
                    PlayerInputHandler.DemoDash = true;
                    dashedAtStart = true;
                }
                for (int i = 0; i < ForwardJumpZ.Length; i++)
                    TryJumpAt(ref forwardJumpsDone[i], p.z >= ForwardJumpZ[i], grounded);
                if (p.z >= 41.5f)
                    segment = 1;
                return;
            }

            // 오른쪽으로, 계단 섬들이 있는 줄(z = 44)을 따라
            float steer = Mathf.Clamp((44f - p.z) * 0.8f, -1f, 1f);
            PlayerInputHandler.DemoMove = endTime > 0f ? new Vector2(0.4f, steer) : new Vector2(1f, steer);

            for (int i = 0; i < StepJumpX.Length; i++)
                TryJumpAt(ref stepJumpsDone[i], p.x >= StepJumpX[i], grounded);

            // 7m 틈: 점프 → 꼭대기에서 더블점프 → 대시
            if (gapJumpTime < 0f && p.x >= 21.6f && grounded)
            {
                PlayerInputHandler.DemoJump = true;
                gapJumpTime = Time.time;
            }
            else if (gapJumpTime > 0f && wallJumpTime < 0f)
            {
                float t = Time.time - gapJumpTime;
                if (Mathf.Abs(t - 0.38f) < 0.5f / CaptureFps)
                    PlayerInputHandler.DemoJump = true;
                if (Mathf.Abs(t - 0.58f) < 0.5f / CaptureFps)
                    PlayerInputHandler.DemoDash = true;

                // 높은 벽(3.2m): 점프 → 더블점프로 오르기
                if (t > 1f && p.x >= 33.5f && grounded && p.y < 8f)
                {
                    PlayerInputHandler.DemoJump = true;
                    wallJumpTime = Time.time;
                }
            }
            else if (wallJumpTime > 0f)
            {
                float t = Time.time - wallJumpTime;
                if (Mathf.Abs(t - 0.3f) < 0.5f / CaptureFps)
                    PlayerInputHandler.DemoJump = true;

                // 벽 위에 올라서면 체크포인트 2를 지나 조금 더 걷다가 끝
                if (endTime < 0f && grounded && p.y > 9.5f)
                    endTime = Time.time + 2f;
            }
        }

        static void TryJumpAt(ref bool done, bool reached, bool grounded)
        {
            if (done || !reached || !grounded)
                return;
            PlayerInputHandler.DemoJump = true;
            done = true;
        }

        void Finish()
        {
            if (phase == Phase.Finished)
                return;
            phase = Phase.Finished;
            Debug.Log($"[Sky Hopper] 자동 시연 녹화 완료: 프레임 {frameIndex}장 → {outputDirectory}");
            Application.Quit();
        }
    }
}
