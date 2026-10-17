using UnityEngine;

namespace SkyHopper
{
    /// <summary>
    /// 플레이어 이동: 걷기, 점프, 더블점프, 대시, 낙하 시 리스폰.
    /// 움직이는 발판 위에 서 있으면 발판과 함께 움직이고, 점프대·넉백·함정에 반응합니다.
    /// 수치는 GAME_DESIGN.md "5. 캐릭터 능력" 표의 초기 값이며, Inspector에서 플레이 중에도 바꿔 볼 수 있습니다.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerController : MonoBehaviour
    {
        [Header("이동")]
        [Tooltip("최대 이동 속도 (m/s)")]
        [SerializeField] float moveSpeed = 6f;
        [Tooltip("땅에서 속도가 붙고 줄어드는 빠르기")]
        [SerializeField] float groundAcceleration = 60f;
        [Tooltip("공중에서 방향을 바꾸는 빠르기 (작을수록 공중 조작이 둔해짐)")]
        [SerializeField] float airAcceleration = 25f;
        [Tooltip("캐릭터가 이동 방향으로 도는 속도 (도/초)")]
        [SerializeField] float turnSpeed = 720f;

        [Header("점프")]
        [Tooltip("점프 높이 (m)")]
        [SerializeField] float jumpHeight = 2f;
        [Tooltip("더블점프로 추가로 올라가는 높이 (m). 누르는 타이밍과 상관없이 첫 점프 최고점 + 이 높이까지 올라감")]
        [SerializeField] float doubleJumpHeight = 1.8f;
        [Tooltip("중력 (음수)")]
        [SerializeField] float gravity = -25f;
        [Tooltip("최대 낙하 속도 (m/s)")]
        [SerializeField] float maxFallSpeed = 30f;
        [Tooltip("발판에서 떨어진 직후에도 점프를 허용하는 시간 (초)")]
        [SerializeField] float coyoteTime = 0.1f;
        [Tooltip("착지 직전에 누른 점프를 기억하는 시간 (초)")]
        [SerializeField] float jumpBufferTime = 0.1f;

        [Header("대시")]
        [Tooltip("대시 거리 (m)")]
        [SerializeField] float dashDistance = 5f;
        [Tooltip("대시에 걸리는 시간 (초)")]
        [SerializeField] float dashDuration = 0.15f;
        [Tooltip("대시 재사용 대기 시간 (초)")]
        [SerializeField] float dashCooldown = 0.8f;

        [Header("넉백")]
        [Tooltip("회전 막대 등에 맞았을 때 조작이 안 되는 시간 (초)")]
        [SerializeField] float stunDuration = 0.5f;

        [Header("낙하")]
        [Tooltip("이 높이보다 아래로 떨어지면 리스폰")]
        [SerializeField] float fallLimitY = -20f;

        [Header("참조")]
        [Tooltip("이동 방향 기준이 되는 카메라. 비워 두면 Main Camera를 사용")]
        [SerializeField] Transform cameraTransform;

        CharacterController controller;
        PlayerInputHandler input;

        Vector3 horizontalVelocity;
        float verticalVelocity;
        float lastGroundedTime = float.NegativeInfinity;
        float lastJumpPressedTime = float.NegativeInfinity;
        bool hasDoubleJump;
        bool hasAirDash;

        bool wasDashing;
        float dashEndTime = float.NegativeInfinity;
        float nextDashTime;
        Vector3 dashDirection;

        float stunEndTime = float.NegativeInfinity;

        // 지금 밟고 있는 물체 (움직이는 발판이면 함께 이동하기 위해 기억)
        Transform groundTransform;
        Transform groundCandidate;
        Vector3 groundLocalPoint;
        Quaternion lastGroundRotation;

        Vector3 spawnPosition;
        Quaternion spawnRotation;

        public bool IsGrounded { get; private set; }
        public bool IsDashing => Time.time < dashEndTime;
        public bool IsStunned => Time.time < stunEndTime;
        /// <summary>위아래 속도 (양수 = 올라가는 중, 음수 = 떨어지는 중)</summary>
        public float VerticalVelocity => verticalVelocity;
        public float HorizontalSpeed => horizontalVelocity.magnitude;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = GetComponent<PlayerInputHandler>();
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;

            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        void Update()
        {
            // 이번 프레임에 움직인 발판들의 위치를 물리 엔진에 먼저 반영
            Physics.SyncTransforms();
            FollowGround();

            if (input.JumpPressed)
                lastJumpPressedTime = Time.time;

            if (IsGrounded)
            {
                lastGroundedTime = Time.time;
                hasDoubleJump = true;
                hasAirDash = true;
            }

            Vector3 moveDirection = GetCameraRelativeMove();

            if (IsStunned)
            {
                // 넉백 중에는 조작 없이 날아가기만 함
                UpdateGravity();
            }
            else
            {
                if (input.DashPressed)
                    TryStartDash(moveDirection);

                if (IsDashing)
                {
                    horizontalVelocity = dashDirection * (dashDistance / dashDuration);
                    verticalVelocity = 0f;
                }
                else
                {
                    // 대시가 끝나는 순간 속도를 보통 속도로 되돌려 미끄러지지 않게 함
                    if (wasDashing)
                        horizontalVelocity = dashDirection * moveSpeed;

                    UpdateHorizontalVelocity(moveDirection);
                    UpdateJump();
                    UpdateGravity();
                    RotateTowards(moveDirection);
                }
            }
            wasDashing = IsDashing;

            groundCandidate = null;
            Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
            CollisionFlags flags = controller.Move(velocity * Time.deltaTime);
            IsGrounded = controller.isGrounded;

            // 천장에 머리를 부딪치면 바로 떨어지기 시작
            if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
                verticalVelocity = 0f;

            UpdateGround();

            if (transform.position.y < fallLimitY)
                Respawn();
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // 발밑(위를 향한 면)에 닿은 물체만 "밟고 있는 물체"로 기억
            if (hit.normal.y > 0.7f)
                groundCandidate = hit.collider.transform;
        }

        /// <summary>밟고 있는 물체가 지난 프레임부터 움직이거나 돈 만큼 플레이어도 함께 옮김</summary>
        void FollowGround()
        {
            if (groundTransform == null)
                return;

            Vector3 delta = groundTransform.TransformPoint(groundLocalPoint) - transform.position;
            float yawDelta = (groundTransform.rotation * Quaternion.Inverse(lastGroundRotation)).eulerAngles.y;

            if (Mathf.Abs(Mathf.DeltaAngle(0f, yawDelta)) > 0.001f)
                transform.Rotate(0f, yawDelta, 0f);
            if (delta.sqrMagnitude > 0.000001f)
                controller.Move(delta);
        }

        void UpdateGround()
        {
            if (!IsGrounded || groundCandidate == null)
            {
                groundTransform = null;
                return;
            }

            groundTransform = groundCandidate;
            groundLocalPoint = groundTransform.InverseTransformPoint(transform.position);
            lastGroundRotation = groundTransform.rotation;

            // 밟으면 반응하는 장치 (떨어지는 발판, 점프대 등)
            var standable = groundTransform.GetComponentInParent<IStandable>();
            if (standable != null)
                standable.OnPlayerStand(this);
        }

        Vector3 GetCameraRelativeMove()
        {
            Vector2 move = input.Move;
            if (cameraTransform == null)
                return new Vector3(move.x, 0f, move.y);

            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            return forward * move.y + right * move.x;
        }

        void UpdateHorizontalVelocity(Vector3 moveDirection)
        {
            Vector3 targetVelocity = moveDirection * moveSpeed;
            float acceleration = IsGrounded ? groundAcceleration : airAcceleration;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, acceleration * Time.deltaTime);
        }

        void UpdateJump()
        {
            bool canGroundJump = Time.time - lastGroundedTime <= coyoteTime;
            bool jumpBuffered = Time.time - lastJumpPressedTime <= jumpBufferTime;

            if (canGroundJump && jumpBuffered)
            {
                // 점프대로 이미 더 빨리 솟구치는 중이면 그 속도를 유지
                verticalVelocity = Mathf.Max(verticalVelocity, VelocityForHeight(jumpHeight));
                lastGroundedTime = float.NegativeInfinity;
                lastJumpPressedTime = float.NegativeInfinity;
            }
            else if (!canGroundJump && hasDoubleJump && input.JumpPressed)
            {
                // 올라가는 중에 일찍 눌러도 손해가 없도록 남은 상승 속도에 더블점프 힘을 더함
                float rising = Mathf.Max(verticalVelocity, 0f);
                verticalVelocity = Mathf.Sqrt(rising * rising + 2f * doubleJumpHeight * -gravity);
                hasDoubleJump = false;
                lastJumpPressedTime = float.NegativeInfinity;
            }
        }

        /// <summary>원하는 높이에 도달하는 데 필요한 속도: v = √(2 × 높이 × 중력)</summary>
        float VelocityForHeight(float height) => Mathf.Sqrt(2f * height * -gravity);

        void UpdateGravity()
        {
            if (IsGrounded && verticalVelocity < 0f)
            {
                // 땅에 붙어 있도록 살짝 아래로 누름 (경사로·계단에서 뜨지 않게)
                verticalVelocity = -2f;
                return;
            }

            verticalVelocity = Mathf.Max(verticalVelocity + gravity * Time.deltaTime, -maxFallSpeed);
        }

        void TryStartDash(Vector3 moveDirection)
        {
            if (Time.time < nextDashTime)
                return;
            if (!IsGrounded && !hasAirDash)
                return;

            dashDirection = moveDirection.sqrMagnitude > 0.01f ? moveDirection.normalized : transform.forward;
            dashEndTime = Time.time + dashDuration;
            nextDashTime = Time.time + dashCooldown;
            if (!IsGrounded)
                hasAirDash = false;

            transform.rotation = Quaternion.LookRotation(dashDirection);
        }

        void RotateTowards(Vector3 moveDirection)
        {
            if (moveDirection.sqrMagnitude < 0.01f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        void LeaveGround()
        {
            IsGrounded = false;
            groundTransform = null;
            lastGroundedTime = float.NegativeInfinity;
        }

        /// <summary>점프대: 지정한 높이만큼 위로 쏘아 올림. 더블점프와 공중 대시도 다시 쓸 수 있게 됨</summary>
        public void Launch(float height)
        {
            verticalVelocity = VelocityForHeight(height);
            hasDoubleJump = true;
            hasAirDash = true;
            dashEndTime = float.NegativeInfinity;
            LeaveGround();
        }

        /// <summary>회전 막대 등: 옆으로 밀쳐 내고 잠깐 조작을 막음</summary>
        public void Knockback(Vector3 horizontalPush, float upHeight)
        {
            if (IsStunned)
                return;

            horizontalVelocity = Vector3.ProjectOnPlane(horizontalPush, Vector3.up);
            verticalVelocity = VelocityForHeight(upHeight);
            stunEndTime = Time.time + stunDuration;
            dashEndTime = float.NegativeInfinity;
            wasDashing = false;
            LeaveGround();
        }

        /// <summary>가시 등 함정에 닿았을 때</summary>
        public void Die()
        {
            Respawn();
        }

        /// <summary>체크포인트에서 호출해 리스폰 위치를 바꿈</summary>
        public void SetSpawnPoint(Vector3 position, Quaternion rotation)
        {
            spawnPosition = position;
            spawnRotation = rotation;
        }

        public void Respawn()
        {
            // CharacterController가 켜져 있으면 위치를 직접 옮길 수 없어서 잠깐 끔
            controller.enabled = false;
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            controller.enabled = true;

            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            dashEndTime = float.NegativeInfinity;
            stunEndTime = float.NegativeInfinity;
            wasDashing = false;
            groundTransform = null;
        }
    }
}
