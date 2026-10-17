using UnityEngine;
using UnityEngine.UI;

namespace SkyHopper
{
    /// <summary>버튼을 누르면 "톡" 소리</summary>
    [RequireComponent(typeof(Button))]
    public class ButtonClickSound : MonoBehaviour
    {
        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => GameFeedback.PlaySound(Feedback.UIClick));
        }
    }
}
