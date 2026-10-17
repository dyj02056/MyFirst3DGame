using UnityEngine;

namespace SkyHopper
{
    /// <summary>
    /// 추적병: 가만히 있다가 플레이어가 가까이 오면 머리 위에 "!"를 띄우고 쫓아옵니다.
    /// 섬 끝(낭떠러지)이나 벽 앞에서는 멈추고, 플레이어를 놓치거나 집에서 너무 멀어지면 집으로 돌아갑니다.
    /// Scene 창에서 노란 원 = 발견 거리, 하늘색 원 = 집에서 벗어날 수 있는 거리입니다.
    /// </summary>
    public class ChaserEnemy : Enemy
    {
        [Header("추적")]
        [Tooltip("플레이어를 발견하는 거리 (m)")]
        [SerializeField] float detectRange = 8f;
        [Tooltip("이 거리보다 멀어지면 추적을 포기 (m)")]
        [SerializeField] float loseRange = 12f;
        [Tooltip("집(처음 위치)에서 이 거리 이상 벗어나지 않음 (m)")]
        [SerializeField] float leashRange = 12f;
        [Tooltip("높이 차이가 이보다 크면 발견하지 못함 (m)")]
        [SerializeField] float maxHeightDifference = 2.5f;
        [Tooltip("쫓아가는 속도 (m/s). 플레이어 속도(6)보다 느리게 해서 도망칠 수 있게 함")]
        [SerializeField] float chaseSpeed = 4.5f;
        [Tooltip("집으로 돌아가는 속도 (m/s)")]
        [SerializeField] float returnSpeed = 2.5f;
        [SerializeField] float turnSpeed = 540f;
        [Tooltip("추적 중에만 보이는 \"!\" 표시")]
        [SerializeField] GameObject alertVisual;

        enum State { Idle, Chasing, Returning }

        State state;
        Transform player;
        Vector3 home;
        Quaternion homeRotation;

        protected override void Awake()
        {
            base.Awake();
            home = transform.position;
            homeRotation = transform.rotation;
            SetAlert(false);
        }

        void Start()
        {
            var playerController = FindAnyObjectByType<PlayerController>();
            if (playerController != null)
                player = playerController.transform;
        }

        protected override void Move()
        {
            if (player == null)
                return;

            Vector3 toPlayer = player.position - transform.position;
            float playerDistance = new Vector2(toPlayer.x, toPlayer.z).magnitude;
            bool sameLevel = Mathf.Abs(toPlayer.y) <= maxHeightDifference;
            float homeDistance = FlatDistance(transform.position, home);

            switch (state)
            {
                case State.Idle:
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, homeRotation, turnSpeed * Time.deltaTime);
                    if (playerDistance <= detectRange && sameLevel)
                        ChangeState(State.Chasing);
                    break;

                case State.Chasing:
                    if (playerDistance > loseRange || !sameLevel || homeDistance > leashRange)
                        ChangeState(State.Returning);
                    else
                        Walk(player.position, chaseSpeed);
                    break;

                case State.Returning:
                    if (homeDistance < 0.1f)
                        ChangeState(State.Idle);
                    else
                        Walk(home, returnSpeed);
                    break;
            }
        }

        void ChangeState(State next)
        {
            state = next;
            SetAlert(next == State.Chasing);
        }

        void Walk(Vector3 target, float speed)
        {
            Vector3 direction = target - transform.position;
            direction.y = 0f;
            float distance = direction.magnitude;
            if (distance < 0.05f)
                return;
            direction /= distance;

            FaceTowards(direction, turnSpeed);
            if (!HasGroundAhead(direction) || IsBlocked(direction))
                return;

            transform.position += direction * Mathf.Min(speed * Time.deltaTime, distance);
        }

        /// <summary>앞쪽 발밑에 땅이 있는지 (낭떠러지로 떨어지지 않게)</summary>
        bool HasGroundAhead(Vector3 direction)
        {
            Vector3 origin = transform.position + direction * 0.8f + Vector3.up * 0.5f;
            return Physics.Raycast(origin, Vector3.down, 1.5f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        }

        /// <summary>바로 앞이 벽으로 막혀 있는지 (플레이어는 벽으로 치지 않음)</summary>
        bool IsBlocked(Vector3 direction)
        {
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            if (!Physics.Raycast(origin, direction, out var hit, 0.8f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                return false;
            return hit.collider.GetComponentInParent<PlayerController>() == null;
        }

        void SetAlert(bool visible)
        {
            if (alertVisual != null)
                alertVisual.SetActive(visible);
        }

        static float FlatDistance(Vector3 a, Vector3 b) => new Vector2(a.x - b.x, a.z - b.z).magnitude;

        void OnDrawGizmosSelected()
        {
            Vector3 center = Application.isPlaying ? home : transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectRange);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(center, leashRange);
        }
    }
}
