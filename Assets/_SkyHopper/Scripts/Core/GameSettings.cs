using UnityEngine;

namespace SkyHopper
{
    /// <summary>설정 화면에서 바꾸는 값 (마우스 감도, 음량). 바꾸는 즉시 저장됩니다.</summary>
    public static class GameSettings
    {
        const string SensitivityKey = "Settings.MouseSensitivity";
        const string VolumeKey = "Settings.Volume";

        public const float MinSensitivity = 0.2f;
        public const float MaxSensitivity = 3f;

        static float? sensitivity;
        static float? volume;

        /// <summary>마우스·스틱 감도 배율 (1 = 기본)</summary>
        public static float MouseSensitivity
        {
            get => sensitivity ??= PlayerPrefs.GetFloat(SensitivityKey, 1f);
            set
            {
                sensitivity = Mathf.Clamp(value, MinSensitivity, MaxSensitivity);
                PlayerPrefs.SetFloat(SensitivityKey, sensitivity.Value);
                PlayerPrefs.Save();
            }
        }

        /// <summary>전체 음량 (0 ~ 1)</summary>
        public static float Volume
        {
            get => volume ??= PlayerPrefs.GetFloat(VolumeKey, 0.8f);
            set
            {
                volume = Mathf.Clamp01(value);
                AudioListener.volume = volume.Value;
                PlayerPrefs.SetFloat(VolumeKey, volume.Value);
                PlayerPrefs.Save();
            }
        }

        // 게임이 시작될 때 저장된 음량을 적용
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ApplyOnStart()
        {
            sensitivity = null;
            volume = null;
            AudioListener.volume = Volume;
        }
    }
}
