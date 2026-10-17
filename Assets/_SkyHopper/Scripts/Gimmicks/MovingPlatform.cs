using UnityEngine;

namespace SkyHopper
{
    /// <summary>
    /// 시작 위치와 (시작 위치 + Move Offset) 사이를 왕복하는 발판.
    /// 위에 탄 플레이어는 PlayerController가 알아서 함께 움직입니다.
    /// Scene 창에서 하늘색 선이 이동 경로, 하늘색 상자가 도착 위치입니다.
    /// </summary>
    [DefaultExecutionOrder(-100)] // 플레이어보다 먼저 움직여야 함께 이동할 때 덜컹거리지 않음
    public class MovingPlatform : MonoBehaviour
    {
        [Tooltip("시작 위치에서 얼마나, 어느 방향으로 이동할지 (월드 기준, m)")]
        [SerializeField] Vector3 moveOffset = new Vector3(0f, 0f, 8f);
        [Tooltip("이동 속도 (m/s)")]
        [SerializeField] float speed = 2.5f;
        [Tooltip("양 끝에서 멈춰 있는 시간 (초)")]
        [SerializeField] float waitTime = 0.6f;

        Vector3 startPosition;
        float progress;
        int direction = 1;
        float waitTimer;

        void Awake()
        {
            startPosition = transform.position;
        }

        void Update()
        {
            float length = moveOffset.magnitude;
            if (length < 0.01f)
                return;

            if (waitTimer > 0f)
            {
                waitTimer -= Time.deltaTime;
                return;
            }

            progress += direction * speed * Time.deltaTime / length;
            if (progress >= 1f || progress <= 0f)
            {
                progress = Mathf.Clamp01(progress);
                direction = -direction;
                waitTimer = waitTime;
            }

            // 양 끝에서 부드럽게 출발·정지
            transform.position = startPosition + moveOffset * Mathf.SmoothStep(0f, 1f, progress);
        }

        void OnDrawGizmos()
        {
            Vector3 start = Application.isPlaying ? startPosition : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(start, start + moveOffset);
            Gizmos.DrawWireCube(start + moveOffset + Vector3.down * 0.3f, new Vector3(4f, 0.6f, 4f));
        }
    }
}
