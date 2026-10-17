using UnityEngine;

namespace SkyHopper
{
    /// <summary>스테이지마다 3개씩 숨겨진 별. 찾기 어려운 곳에 있는 도전 요소입니다.</summary>
    [RequireComponent(typeof(Collider))]
    public class Star : MonoBehaviour
    {
        /// <summary>현재 씬에 남아 있는 별 수 (StageManager가 시작할 때 전체 개수로 사용)</summary>
        public static int ActiveCount { get; private set; }

        [SerializeField] float spinSpeed = 90f;
        [SerializeField] float bobHeight = 0.25f;
        [SerializeField] float bobSpeed = 1.5f;

        Vector3 startPosition;

        void OnEnable() => ActiveCount++;
        void OnDisable() => ActiveCount--;

        void Start()
        {
            startPosition = transform.position;
        }

        void Update()
        {
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
            transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null)
                return;

            if (StageManager.Instance != null)
                StageManager.Instance.AddStar();
            GameFeedback.Play(Feedback.Star, transform.position);
            Destroy(gameObject);
        }
    }
}
