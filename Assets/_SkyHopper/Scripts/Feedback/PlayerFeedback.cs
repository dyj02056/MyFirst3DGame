using UnityEngine;

namespace SkyHopper
{
    /// <summary>
    /// 플레이어의 소리와 효과: 점프·더블점프·대시·착지·발소리·실패·리스폰, 대시 중 꼬리(트레일).
    /// PlayerController가 알려 주는 일(이벤트)을 듣고 GameFeedback으로 소리와 파티클을 냅니다.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerFeedback : MonoBehaviour
    {
        [Tooltip("대시하는 동안만 보이는 꼬리")]
        [SerializeField] TrailRenderer dashTrail;
        [Tooltip("최대 속도로 걸을 때 발소리 간격 (초)")]
        [SerializeField] float footstepInterval = 0.32f;

        PlayerController player;
        float footstepTimer;

        void Awake()
        {
            player = GetComponent<PlayerController>();
            player.Jumped += OnJumped;
            player.DoubleJumped += OnDoubleJumped;
            player.Dashed += OnDashed;
            player.Landed += OnLanded;
            player.KnockedBack += OnKnockedBack;
            player.Died += OnDied;
            player.Respawned += OnRespawned;
            if (dashTrail != null)
                dashTrail.emitting = false;
        }

        void OnDestroy()
        {
            player.Jumped -= OnJumped;
            player.DoubleJumped -= OnDoubleJumped;
            player.Dashed -= OnDashed;
            player.Landed -= OnLanded;
            player.KnockedBack -= OnKnockedBack;
            player.Died -= OnDied;
            player.Respawned -= OnRespawned;
        }

        void Update()
        {
            if (dashTrail != null)
                dashTrail.emitting = player.IsDashing;
            UpdateFootsteps();
        }

        void UpdateFootsteps()
        {
            bool walking = player.enabled && player.IsGrounded && player.HorizontalSpeed > 1f && Time.timeScale > 0f;
            if (!walking)
            {
                // 다시 걷기 시작하면 첫 발소리가 바로 나도록
                footstepTimer = 0.05f;
                return;
            }

            // 빨리 걸을수록 발소리 간격이 짧아짐
            footstepTimer -= Time.deltaTime * Mathf.Clamp01(player.HorizontalSpeed / 6f);
            if (footstepTimer <= 0f)
            {
                GameFeedback.PlaySound(Feedback.Footstep);
                footstepTimer = footstepInterval;
            }
        }

        void OnJumped() => GameFeedback.Play(Feedback.Jump, transform.position);
        void OnDoubleJumped() => GameFeedback.Play(Feedback.DoubleJump, transform.position);
        void OnDashed() => GameFeedback.Play(Feedback.Dash, transform.position + Vector3.up * 0.9f);
        void OnLanded() => GameFeedback.Play(Feedback.Land, transform.position);
        void OnKnockedBack() => GameFeedback.Play(Feedback.Knockback, transform.position + Vector3.up * 0.9f);
        void OnDied(Vector3 position) => GameFeedback.Play(Feedback.Hurt, position + Vector3.up * 0.9f);
        void OnRespawned() => GameFeedback.Play(Feedback.Respawn, transform.position + Vector3.up * 0.9f);
    }
}
