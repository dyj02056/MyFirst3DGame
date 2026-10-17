using UnityEngine;

namespace SkyHopper
{
    /// <summary>
    /// 씬마다 하나씩 있는 소리·파티클 담당. 배경음악을 틀고, 다른 스크립트가
    /// GameFeedback.Play(Feedback.Coin, 위치) 처럼 부르면 효과음과 파티클을 냅니다.
    /// </summary>
    public class GameFeedback : MonoBehaviour
    {
        public static GameFeedback Instance { get; private set; }

        [SerializeField] FeedbackLibrary library;
        [Header("배경음악")]
        [SerializeField] AudioClip music;
        [Tooltip("이 씬의 배경음악 기본 음량 (설정 화면의 배경음악 음량이 곱해짐)")]
        [Range(0f, 1f)] [SerializeField] float musicVolume = 0.5f;

        const int SfxSourceCount = 8;

        AudioSource musicSource;
        AudioSource[] sfxSources;
        int nextSfxSource;

        void Awake()
        {
            Instance = this;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = music;
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = MusicVolume;

            // 효과음은 여러 개가 겹쳐 날 수 있게 스피커를 여러 개 준비 (화면 어디서 나든 같은 크기로 들리는 2D 소리)
            sfxSources = new AudioSource[SfxSourceCount];
            for (int i = 0; i < SfxSourceCount; i++)
            {
                sfxSources[i] = gameObject.AddComponent<AudioSource>();
                sfxSources[i].playOnAwake = false;
            }
        }

        void Start()
        {
            if (music != null)
                musicSource.Play();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            // 설정 화면에서 배경음악 음량을 바꾸면 바로 반영
            musicSource.volume = MusicVolume;
        }

        float MusicVolume => musicVolume * GameSettings.MusicVolume;

        /// <summary>효과음 + 그 위치에 파티클</summary>
        public static void Play(Feedback type, Vector3 position)
        {
            if (Instance != null)
                Instance.PlayInternal(type, position, true);
        }

        /// <summary>효과음만 (발소리, 버튼 클릭 등)</summary>
        public static void PlaySound(Feedback type)
        {
            if (Instance != null)
                Instance.PlayInternal(type, Vector3.zero, false);
        }

        void PlayInternal(Feedback type, Vector3 position, bool withEffect)
        {
            if (library == null)
                return;
            var entry = library.Get(type);
            if (entry == null)
                return;

            if (entry.clips != null && entry.clips.Length > 0)
            {
                var clip = entry.clips[Random.Range(0, entry.clips.Length)];
                if (clip != null)
                {
                    var source = sfxSources[nextSfxSource];
                    nextSfxSource = (nextSfxSource + 1) % sfxSources.Length;
                    source.pitch = 1f + Random.Range(-entry.pitchVariation, entry.pitchVariation);
                    source.PlayOneShot(clip, entry.volume);
                }
            }

            // 파티클 프리팹은 다 재생되면 스스로 사라지도록 만들어져 있음
            if (withEffect && entry.effect != null)
                Instantiate(entry.effect, position, Quaternion.identity);
        }
    }
}
