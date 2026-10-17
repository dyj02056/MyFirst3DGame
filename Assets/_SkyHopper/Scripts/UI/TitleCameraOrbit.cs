using UnityEngine;

namespace SkyHopper
{
    /// <summary>타이틀 화면 배경: 카메라가 섬 주위를 천천히 돕니다.</summary>
    public class TitleCameraOrbit : MonoBehaviour
    {
        [SerializeField] Vector3 center = Vector3.zero;
        [SerializeField] float radius = 14f;
        [SerializeField] float height = 5f;
        [SerializeField] float degreesPerSecond = 6f;
        [SerializeField] Vector3 lookOffset = new Vector3(0f, 1f, 0f);

        float angle;

        void Update()
        {
            angle += degreesPerSecond * Time.unscaledDeltaTime;
            Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, height, -radius);
            transform.position = center + offset;
            transform.LookAt(center + lookOffset);
        }
    }
}
