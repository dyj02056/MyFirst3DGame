using UnityEngine;

namespace SkyHopper
{
    /// <summary>설정 화면에서 바꾸는 값 (마우스 감도, 전체 음량, 배경음악 음량). 바꾸는 즉시 저장됩니다.</summary>
    public static class GameSettings
    {
        const string SensitivityKey = "Settings.MouseSensitivity";
        const string VolumeKey = "Settings.Volume";
        const string MusicVolumeKey = "Settings.MusicVolume";

        public const float MinSensitivity = 0.2f;
        public const float MaxSensitivity = 3f;

        static float? sensitivity;
        static float? volume;
        static float? musicVolume;

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

        /// <summary>배경음악 음량 (0 ~ 1). 전체 음량과 곱해짐</summary>
        public static float MusicVolume
        {
            get => musicVolume ??= PlayerPrefs.GetFloat(MusicVolumeKey, 0.7f);
            set
            {
                musicVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume.Value);
                PlayerPrefs.Save();
            }
        }

        // 게임이 시작될 때 저장된 음량을 적용
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ApplyOnStart()
        {
            sensitivity = null;
            volume = null;
            musicVolume = null;
            AudioListener.volume = Volume;
        }
    }
}
