using UnityEngine;

namespace SkyHopper
{
    /// <summary>밟으면 잠시 흔들리다가 떨어지고, 몇 초 뒤 제자리에 다시 나타나는 발판.</summary>
    [DefaultExecutionOrder(-100)]
    public class FallingPlatform : MonoBehaviour, IStandable
    {
        [Tooltip("밟은 뒤 떨어지기 시작할 때까지 흔들리는 시간 (초)")]
        [SerializeField] float shakeTime = 0.6f;
        [Tooltip("흔들림 크기 (m)")]
        [SerializeField] float shakeAmount = 0.06f;
        [Tooltip("떨어지는 가속도")]
        [SerializeField] float fallAcceleration = 20f;
        [Tooltip("떨어지기 시작해서 사라질 때까지의 시간 (초)")]
        [SerializeField] float fallTime = 1.5f;
        [Tooltip("사라진 뒤 다시 나타날 때까지의 시간 (초)")]
        [SerializeField] float respawnDelay = 3f;

        enum State { Idle, Shaking, Falling, Hidden }

        State state;
        float timer;
        float fallSpeed;
        Vector3 startPosition;
        Renderer[] renderers;
        Collider[] colliders;

        void Awake()
        {
            startPosition = transform.position;
            renderers = GetComponentsInChildren<Renderer>();
            colliders = GetComponentsInChildren<Collider>();
        }

        public void OnPlayerStand(PlayerController player)
        {
            if (state != State.Idle)
                return;
            state = State.Shaking;
            timer = 0f;
            GameFeedback.Play(Feedback.PlatformCrack, transform.position);
        }

        void Update()
        {
            timer += Time.deltaTime;
            switch (state)
            {
                case State.Shaking:
                    Vector2 shake = Random.insideUnitCircle * shakeAmount;
                    transform.position = startPosition + new Vector3(shake.x, 0f, shake.y);
                    if (timer >= shakeTime)
                    {
                        transform.position = startPosition;
                        fallSpeed = 0f;
                        timer = 0f;
                        state = State.Falling;
                    }
                    break;

                case State.Falling:
                    fallSpeed += fallAcceleration * Time.deltaTime;
                    transform.position += Vector3.down * (fallSpeed * Time.deltaTime);
                    if (timer >= fallTime)
                    {
                        SetVisible(false);
                        timer = 0f;
                        state = State.Hidden;
                    }
                    break;

                case State.Hidden:
                    if (timer >= respawnDelay)
                    {
                        transform.position = startPosition;
                        SetVisible(true);
                        state = State.Idle;
                    }
                    break;
            }
        }

        void SetVisible(bool visible)
        {
            foreach (var r in renderers)
                r.enabled = visible;
            foreach (var c in colliders)
                c.enabled = visible;
        }
    }
}
