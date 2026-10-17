using UnityEngine;

namespace SkyHopper
{
    /// <summary>
    /// 순찰병: 시작 위치와 (시작 위치 + Move Offset) 사이를 왕복합니다.
    /// Scene 창에서 빨간 선이 순찰 경로입니다.
    /// </summary>
    public class PatrolEnemy : Enemy
    {
        [Header("순찰")]
        [Tooltip("시작 위치에서 얼마나, 어느 방향으로 왕복할지 (월드 기준, m)")]
        [SerializeField] Vector3 moveOffset = new Vector3(6f, 0f, 0f);
        [Tooltip("걷는 속도 (m/s)")]
        [SerializeField] float speed = 2f;
        [Tooltip("양 끝에서 멈춰 있는 시간 (초)")]
        [SerializeField] float waitTime = 0.5f;
        [Tooltip("방향을 바꾸는 속도 (도/초)")]
        [SerializeField] float turnSpeed = 540f;

        Vector3 startPosition;
        float progress;
        int direction = 1;
        float waitTimer;

        protected override void Awake()
        {
            base.Awake();
            startPosition = transform.position;
        }

        protected override void Move()
        {
            // 가는 방향(끝에서 기다리는 동안엔 돌아갈 방향)을 바라봄
            FaceTowards(moveOffset * direction, turnSpeed);

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
            transform.position = startPosition + moveOffset * progress;
        }

        void OnDrawGizmos()
        {
            Vector3 start = Application.isPlaying ? startPosition : transform.position;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(start + Vector3.up * 0.1f, start + moveOffset + Vector3.up * 0.1f);
            Gizmos.DrawWireSphere(start + moveOffset + Vector3.up * 0.5f, 0.5f);
        }
    }
}
