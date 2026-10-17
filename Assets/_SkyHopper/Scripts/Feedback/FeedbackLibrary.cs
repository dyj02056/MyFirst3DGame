using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkyHopper
{
    /// <summary>게임에서 일어나는 일의 종류 (이 일이 생기면 소리와 파티클을 냄)</summary>
    public enum Feedback
    {
        Jump,
        DoubleJump,
        Dash,
        Land,
        Footstep,
        Coin,
        Star,
        Checkpoint,
        Stomp,
        Hurt,
        Respawn,
        JumpPad,
        Knockback,
        PlatformCrack,
        Clear,
        UIClick,
    }

    /// <summary>
    /// 일의 종류별로 어떤 소리와 파티클을 쓸지 정리한 표.
    /// Assets/_SkyHopper/Data/FeedbackLibrary 를 클릭하면 Inspector에서 소리·음량·파티클을 바꿀 수 있습니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Sky Hopper/Feedback Library", fileName = "FeedbackLibrary")]
    public class FeedbackLibrary : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public Feedback type;
            [Tooltip("여러 개를 넣으면 그중 하나를 무작위로 재생 (발소리 등)")]
            public AudioClip[] clips;
            [Range(0f, 1f)] public float volume = 0.7f;
            [Tooltip("재생할 때마다 음높이를 이만큼 무작위로 바꿔서 덜 단조롭게")]
            [Range(0f, 0.3f)] public float pitchVariation = 0.05f;
            [Tooltip("그 자리에 생기는 파티클 (비워 둬도 됨)")]
            public ParticleSystem effect;
        }

        [SerializeField] Entry[] entries = new Entry[0];

        Dictionary<Feedback, Entry> lookup;

        public Entry Get(Feedback type)
        {
            if (lookup == null)
            {
                lookup = new Dictionary<Feedback, Entry>();
                foreach (var entry in entries)
                    lookup[entry.type] = entry;
            }
            return lookup.TryGetValue(type, out var found) ? found : null;
        }

        // Inspector에서 값을 바꾸면 다시 찾도록
        void OnValidate() => lookup = null;
    }
}
