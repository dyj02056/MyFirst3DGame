using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SkyHopper
{
    /// <summary>설정 창: 마우스 감도, 전체 음량, 배경음악 음량. 타이틀 화면과 일시정지 화면이 함께 씁니다.</summary>
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] Slider sensitivitySlider;
        [SerializeField] TMP_Text sensitivityValue;
        [SerializeField] Slider volumeSlider;
        [SerializeField] TMP_Text volumeValue;
        [SerializeField] Slider musicSlider;
        [SerializeField] TMP_Text musicValue;
        [SerializeField] Button closeButton;

        /// <summary>닫기 버튼을 눌렀을 때 (원래 화면으로 돌아가기 위해)</summary>
        public event Action Closed;

        void Awake()
        {
            sensitivitySlider.minValue = GameSettings.MinSensitivity;
            sensitivitySlider.maxValue = GameSettings.MaxSensitivity;
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;

            sensitivitySlider.onValueChanged.AddListener(value =>
            {
                GameSettings.MouseSensitivity = value;
                RefreshLabels();
            });
            volumeSlider.onValueChanged.AddListener(value =>
            {
                GameSettings.Volume = value;
                RefreshLabels();
            });
            if (musicSlider != null)
            {
                musicSlider.minValue = 0f;
                musicSlider.maxValue = 1f;
                musicSlider.onValueChanged.AddListener(value =>
                {
                    GameSettings.MusicVolume = value;
                    RefreshLabels();
                });
            }
            closeButton.onClick.AddListener(Close);
        }

        void OnEnable()
        {
            sensitivitySlider.SetValueWithoutNotify(GameSettings.MouseSensitivity);
            volumeSlider.SetValueWithoutNotify(GameSettings.Volume);
            if (musicSlider != null)
                musicSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
            RefreshLabels();
        }

        public void Open() => gameObject.SetActive(true);

        public void Close()
        {
            gameObject.SetActive(false);
            Closed?.Invoke();
        }

        void RefreshLabels()
        {
            sensitivityValue.text = $"{GameSettings.MouseSensitivity:0.0}배";
            volumeValue.text = $"{Mathf.RoundToInt(GameSettings.Volume * 100f)}%";
            if (musicValue != null)
                musicValue.text = $"{Mathf.RoundToInt(GameSettings.MusicVolume * 100f)}%";
        }
    }
}
