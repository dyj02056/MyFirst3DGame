using UnityEngine;
using UnityEngine.InputSystem;

namespace SkyHopper
{
    /// <summary>
    /// 마우스(또는 오른쪽 스틱)로 플레이어 주위를 도는 3인칭 카메라.
    /// 벽에 가려지면 카메라가 플레이어 쪽으로 당겨집니다.
    /// 게임 화면을 클릭하면 마우스 커서가 숨습니다. 감도는 설정 화면의 배율(GameSettings)이 곱해집니다.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("대상")]
        [SerializeField] Transform target;
        [SerializeField] PlayerInputHandler input;
        [Tooltip("플레이어 발밑에서 카메라가 바라볼 지점까지의 높이")]
        [SerializeField] Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

        [Header("거리")]
        [SerializeField] float distance = 6f;
        [SerializeField] float minDistance = 1f;

        [Header("회전")]
        [Tooltip("마우스 감도 (1픽셀당 회전 각도)")]
        [SerializeField] float mouseSensitivity = 0.15f;
        [Tooltip("게임패드 스틱 회전 속도 (도/초)")]
        [SerializeField] float stickSensitivity = 180f;
        [SerializeField] bool invertY;
        [SerializeField] float minPitch = -30f;
        [SerializeField] float maxPitch = 70f;

        [Header("충돌")]
        [SerializeField] float collisionRadius = 0.2f;
        [SerializeField] LayerMask collisionMask = Physics.DefaultRaycastLayers;
        [Tooltip("가려졌다가 다시 멀어질 때의 속도 (m/s)")]
        [SerializeField] float distanceRecoverySpeed = 10f;

        float yaw;
        float pitch = 20f;
        float currentDistance;
        readonly RaycastHit[] hits = new RaycastHit[16];

        void Start()
        {
            if (target == null)
            {
                var player = FindAnyObjectByType<PlayerController>();
                if (player != null)
                    target = player.transform;
            }
            if (input == null && target != null)
                input = target.GetComponent<PlayerInputHandler>();

            if (target != null)
                yaw = target.eulerAngles.y;
            currentDistance = distance;
            GameCursor.SetLocked(true);
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            // 일시정지 메뉴가 없는 테스트 맵에서는 Esc로 커서만 보이게 함
            if (PauseMenu.Instance == null && keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                GameCursor.SetLocked(false);
            else if (mouse != null && mouse.leftButton.wasPressedThisFrame && !GameCursor.IsLocked && CanControl())
                GameCursor.SetLocked(true);
        }

        /// <summary>일시정지 중이거나 클리어 화면에서는 카메라를 돌리지 않음</summary>
        static bool CanControl() =>
            !PauseMenu.IsPaused && (StageManager.Instance == null || !StageManager.Instance.IsCleared);

        void LateUpdate()
        {
            if (target == null)
                return;

            if (input != null && CanControl())
            {
                float sensitivity = GameSettings.MouseSensitivity;
                Vector2 look = input.StickLook * (stickSensitivity * sensitivity * Time.deltaTime);
                if (GameCursor.IsLocked)
                    look += input.MouseLook * (mouseSensitivity * sensitivity);

                yaw += look.x;
                pitch += invertY ? look.y : -look.y;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + targetOffset;
            Vector3 backward = rotation * Vector3.back;

            float wantedDistance = GetUnblockedDistance(pivot, backward);
            // 가려지면 즉시 당기고, 다시 트이면 부드럽게 멀어짐
            currentDistance = wantedDistance < currentDistance
                ? wantedDistance
                : Mathf.MoveTowards(currentDistance, wantedDistance, distanceRecoverySpeed * Time.deltaTime);

            transform.SetPositionAndRotation(pivot + backward * currentDistance, rotation);
        }

        float GetUnblockedDistance(Vector3 pivot, Vector3 direction)
        {
            int count = Physics.SphereCastNonAlloc(pivot, collisionRadius, direction, hits, distance,
                collisionMask, QueryTriggerInteraction.Ignore);

            float result = distance;
            for (int i = 0; i < count; i++)
            {
                if (hits[i].transform.IsChildOf(target))
                    continue;
                result = Mathf.Min(result, Mathf.Max(minDistance, hits[i].distance));
            }
            return result;
        }
    }
}
