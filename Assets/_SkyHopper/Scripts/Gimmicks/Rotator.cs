using UnityEngine;

namespace SkyHopper
{
    /// <summary>오브젝트를 계속 회전시킵니다 (회전 막대 등).</summary>
    [DefaultExecutionOrder(-100)]
    public class Rotator : MonoBehaviour
    {
        [Tooltip("회전 축 (기본: 위쪽 = 바닥과 평행하게 빙글빙글)")]
        [SerializeField] Vector3 axis = Vector3.up;
        [Tooltip("회전 속도 (도/초). 음수면 반대 방향")]
        [SerializeField] float degreesPerSecond = 90f;

        /// <summary>월드 기준 회전 축</summary>
        public Vector3 WorldAxis => transform.TransformDirection(axis).normalized;
        public float DegreesPerSecond => degreesPerSecond;

        void Update()
        {
            transform.Rotate(axis, degreesPerSecond * Time.deltaTime, Space.Self);
        }
    }
}
