using UnityEngine;

namespace SkyHopper
{
    /// <summary>
    /// 플레이어가 지나가면 리스폰 위치가 이곳으로 바뀌고 깃발 색이 바뀝니다.
    /// 플레이어는 이 오브젝트의 위치와 방향(파란 화살표 = 앞)으로 다시 시작합니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] Renderer flagRenderer;
        [SerializeField] Material activeMaterial;
        [SerializeField] AudioClip activateSound;

        bool activated;

        void OnTriggerEnter(Collider other)
        {
            if (activated)
                return;
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null)
                return;

            activated = true;
            player.SetSpawnPoint(transform.position + Vector3.up * 0.1f, transform.rotation);

            if (flagRenderer != null && activeMaterial != null)
                flagRenderer.sharedMaterial = activeMaterial;
            if (activateSound != null)
                AudioSource.PlayClipAtPoint(activateSound, transform.position);
        }
    }
}
