using UnityEngine;

namespace SkyHopper
{
    /// <summary>
    /// 닿은 플레이어를 밀쳐 냅니다. Rotator가 연결되어 있으면 막대가 휘두르는 방향으로 밀고,
    /// 없으면 이 오브젝트 중심에서 멀어지는 방향으로 밉니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class KnockbackZone : MonoBehaviour
    {
        [SerializeField] Rotator rotator;
        [Tooltip("옆으로 밀리는 속도 (m/s)")]
        [SerializeField] float pushSpeed = 12f;
        [Tooltip("위로 튀어 오르는 높이 (m)")]
        [SerializeField] float pushUpHeight = 1.2f;

        void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null || player.IsStunned)
                return;

            Vector3 direction;
            if (rotator != null)
            {
                Vector3 fromCenter = player.transform.position - rotator.transform.position;
                direction = Vector3.Cross(rotator.WorldAxis, fromCenter) * Mathf.Sign(rotator.DegreesPerSecond);
            }
            else
            {
                direction = player.transform.position - transform.position;
            }

            direction = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (direction.sqrMagnitude < 0.0001f)
                direction = -player.transform.forward;

            player.Knockback(direction.normalized * pushSpeed, pushUpHeight);
        }
    }
}
