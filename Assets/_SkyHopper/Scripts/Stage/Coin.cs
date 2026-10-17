using UnityEngine;

namespace SkyHopper
{
    /// <summary>빙글빙글 돌며 떠 있는 코인. 플레이어가 닿으면 획득됩니다.</summary>
    [RequireComponent(typeof(Collider))]
    public class Coin : MonoBehaviour
    {
        /// <summary>현재 씬에 남아 있는 코인 수 (StageManager가 시작할 때 전체 개수로 사용)</summary>
        public static int ActiveCount { get; private set; }

        [SerializeField] float spinSpeed = 180f;
        [SerializeField] float bobHeight = 0.15f;
        [SerializeField] float bobSpeed = 2f;

        Vector3 startPosition;
        float bobOffset;

        void OnEnable() => ActiveCount++;
        void OnDisable() => ActiveCount--;

        void Start()
        {
            startPosition = transform.position;
            // 코인마다 위아래 움직임 타이밍을 조금씩 다르게
            bobOffset = startPosition.x + startPosition.z;
        }

        void Update()
        {
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
            float y = Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobHeight;
            transform.position = startPosition + Vector3.up * y;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null)
                return;

            if (StageManager.Instance != null)
                StageManager.Instance.AddCoin();
            GameFeedback.Play(Feedback.Coin, transform.position);
            Destroy(gameObject);
        }
    }
}
