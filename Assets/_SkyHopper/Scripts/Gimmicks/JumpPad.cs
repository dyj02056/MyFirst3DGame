using UnityEngine;

namespace SkyHopper
{
    /// <summary>밟으면 플레이어를 높이 튕겨 올리는 점프대. 튕긴 뒤 더블점프와 대시를 다시 쓸 수 있습니다.</summary>
    public class JumpPad : MonoBehaviour, IStandable
    {
        [Tooltip("튕겨 올리는 높이 (m)")]
        [SerializeField] float launchHeight = 7f;
        [Tooltip("튕길 때 납작해지는 모양 (비워 둬도 됨)")]
        [SerializeField] Transform springVisual;

        Vector3 springBaseScale;
        float squashTimer;

        void Awake()
        {
            if (springVisual != null)
                springBaseScale = springVisual.localScale;
        }

        public void OnPlayerStand(PlayerController player)
        {
            player.Launch(launchHeight);
            squashTimer = 0.25f;
            GameFeedback.Play(Feedback.JumpPad, transform.position + Vector3.up * 0.3f);
        }

        void Update()
        {
            if (springVisual == null || squashTimer <= 0f)
                return;

            squashTimer -= Time.deltaTime;
            // 0.25초 동안 납작해졌다가 원래대로
            float squash = Mathf.Sin(Mathf.Clamp01(squashTimer / 0.25f) * Mathf.PI) * 0.5f;
            springVisual.localScale = Vector3.Scale(springBaseScale, new Vector3(1f + squash * 0.3f, 1f - squash, 1f + squash * 0.3f));
        }
    }
}
