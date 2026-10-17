using UnityEngine;

namespace SkyHopper
{
    /// <summary>플레이어가 들어오면 스테이지 클리어.</summary>
    [RequireComponent(typeof(Collider))]
    public class Goal : MonoBehaviour
    {
        [SerializeField] AudioClip clearSound;

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null)
                return;
            if (StageManager.Instance == null || StageManager.Instance.IsCleared)
                return;

            StageManager.Instance.CompleteStage();
            if (clearSound != null)
                AudioSource.PlayClipAtPoint(clearSound, transform.position);
        }
    }
}
