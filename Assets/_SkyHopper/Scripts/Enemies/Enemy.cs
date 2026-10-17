using UnityEngine;

namespace SkyHopper
{
    /// <summary>
    /// 적의 공통 동작: 위에서 밟히면 납작해지며 사라지고, 다른 방향으로 닿으면 플레이어가 실패합니다.
    /// 이동 방식은 PatrolEnemy(순찰병), ChaserEnemy(추적병)가 각자 Move()에서 정합니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class Enemy : MonoBehaviour
    {
        [Header("밟기")]
        [Tooltip("플레이어 발이 적의 발밑에서 이 높이보다 위에 있으면 '밟기'로 판정 (m)")]
        [SerializeField] float stompHeight = 0.6f;
        [Tooltip("밟은 뒤 플레이어가 튀어 오르는 높이 (m)")]
        [SerializeField] float stompBounceHeight = 3f;

        const float DefeatDuration = 0.3f;

        public bool IsDefeated { get; private set; }

        Vector3 baseScale;
        float defeatTimer;

        protected virtual void Awake()
        {
            baseScale = transform.localScale;
        }

        void Update()
        {
            if (IsDefeated)
            {
                PlayDefeatAnimation();
                return;
            }
            Move();
        }

        /// <summary>매 프레임 적을 움직이는 부분 (적 종류마다 다름)</summary>
        protected abstract void Move();

        void OnTriggerEnter(Collider other)
        {
            if (IsDefeated)
                return;
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null)
                return;

            bool fromAbove = player.transform.position.y >= transform.position.y + stompHeight
                             && player.VerticalVelocity <= 0.1f;
            if (fromAbove)
            {
                Defeat();
                player.Launch(stompBounceHeight);
            }
            else
            {
                player.Die();
            }
        }

        void Defeat()
        {
            IsDefeated = true;
            foreach (var c in GetComponentsInChildren<Collider>())
                c.enabled = false;
            GameFeedback.Play(Feedback.Stomp, transform.position + Vector3.up * 0.6f);
        }

        void PlayDefeatAnimation()
        {
            // 납작하게 찌그러졌다가 사라짐
            defeatTimer += Time.deltaTime;
            float t = Mathf.Clamp01(defeatTimer / DefeatDuration);
            transform.localScale = Vector3.Scale(baseScale, new Vector3(1f + 0.4f * t, Mathf.Lerp(1f, 0.15f, t), 1f + 0.4f * t));
            if (t >= 1f)
                gameObject.SetActive(false);
        }

        /// <summary>direction 쪽을 바라보도록 천천히 회전</summary>
        protected void FaceTowards(Vector3 direction, float turnSpeed)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
                return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
        }
    }
}
