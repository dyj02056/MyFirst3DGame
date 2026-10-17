using UnityEngine;

namespace SkyHopper
{
    /// <summary>닿으면 실패하고 마지막 체크포인트에서 다시 시작하게 하는 함정 (가시 등).</summary>
    [RequireComponent(typeof(Collider))]
    public class Hazard : MonoBehaviour
    {
        [SerializeField] AudioClip hitSound;

        void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null)
                return;

            if (hitSound != null)
                AudioSource.PlayClipAtPoint(hitSound, player.transform.position);
            player.Die();
        }
    }
}
