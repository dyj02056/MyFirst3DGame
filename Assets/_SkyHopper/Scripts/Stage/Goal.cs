using UnityEngine;

namespace SkyHopper
{
    /// <summary>플레이어가 들어오면 스테이지 클리어 (빰빠밤 소리와 색종이 파티클).</summary>
    [RequireComponent(typeof(Collider))]
    public class Goal : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null)
                return;
            if (StageManager.Instance == null || StageManager.Instance.IsCleared)
                return;

            StageManager.Instance.CompleteStage();
            GameFeedback.Play(Feedback.Clear, transform.position + Vector3.up * 3f);
        }
    }
}
