using System.IO;
using UnityEditor;
using UnityEngine;
using static SkyHopper.EditorTools.SceneBuilderUtility;

namespace SkyHopper.EditorTools
{
    /// <summary>
    /// 효과음·파티클 준비:
    ///  - 파티클 프리팹 (Assets/_SkyHopper/Prefabs/Effects)
    ///  - 일의 종류별 소리·파티클 표 (Assets/_SkyHopper/Data/FeedbackLibrary)
    /// 스테이지·타이틀을 만들 때 자동으로 준비되고, 메뉴 "Sky Hopper > 효과음·파티클 다시 만들기"로 처음 상태로 되돌릴 수 있습니다.
    /// </summary>
    public static class FeedbackBuilder
    {
        const string LibraryPath = "Assets/_SkyHopper/Data/FeedbackLibrary.asset";
        const string EffectFolder = "Assets/_SkyHopper/Prefabs/Effects";
        const string SfxFolder = "Assets/_SkyHopper/Audio/SFX/";
        const string TemplateSfx = "Assets/SourceFiles/SoundFX/";
        const string RobotSfx = "Assets/SourceFiles/TimmyRobot/Sfx/";
        const string ParticleMaterialPath = "Assets/SourceFiles/VFX/GlowMaterial.mat";

        internal const string TitleMusic = TemplateSfx + "Music_Ethereal.ogg";
        internal const string StageMusic = TemplateSfx + "Music_Exciting.ogg";
        internal const string HardStageMusic = TemplateSfx + "Music_Suspenseful.ogg";

        [MenuItem("Sky Hopper/효과음·파티클 다시 만들기", priority = 40)]
        static void RebuildFromMenu()
        {
            if (!EditorUtility.DisplayDialog("효과음·파티클 다시 만들기",
                    "파티클 프리팹과 소리 표(FeedbackLibrary)를 처음 상태로 다시 만듭니다.\nInspector에서 바꾼 소리·음량은 사라집니다. 계속할까요?",
                    "다시 만들기", "취소"))
                return;
            GetOrCreateLibrary(rebuild: true);
            Debug.Log("[Sky Hopper] 효과음·파티클을 다시 만들었습니다: " + LibraryPath);
        }

        /// <summary>씬에 소리·파티클 담당(GameFeedback)을 넣고 배경음악을 지정</summary>
        internal static void CreateGameFeedback(string musicPath, float musicVolume = 0.5f)
        {
            var feedback = new GameObject("GameFeedback").AddComponent<GameFeedback>();
            SetReference(feedback, "library", GetOrCreateLibrary(rebuild: false));
            if (!string.IsNullOrEmpty(musicPath))
                SetReference(feedback, "music", LoadAsset<AudioClip>(musicPath));
            SetFloat(feedback, "musicVolume", musicVolume);
        }

        internal static FeedbackLibrary GetOrCreateLibrary(bool rebuild)
        {
            var library = AssetDatabase.LoadAssetAtPath<FeedbackLibrary>(LibraryPath);
            if (library != null && !rebuild)
                return library;

            if (library == null)
            {
                library = ScriptableObject.CreateInstance<FeedbackLibrary>();
                Directory.CreateDirectory(Path.GetDirectoryName(LibraryPath));
                AssetDatabase.CreateAsset(library, LibraryPath);
            }

            var gold = new Color(1f, 0.85f, 0.25f);
            var white = Color.white;
            var dust = new Color(0.85f, 0.82f, 0.75f, 0.8f);

            var coinFx = CreateBurst("FX_CoinSparkle", gold, white, 14, 3f, 0.25f, 0.5f, 0.3f, 0.2f);
            var starFx = CreateBurst("FX_StarBurst", gold, white, 40, 6f, 0.4f, 0.9f, 0.2f, 0.3f);
            var dustFx = CreateBurst("FX_Dust", dust, white, 10, 1.5f, 0.5f, 0.45f, 0f, 0.3f);
            var stompFx = CreateBurst("FX_Stomp", white, new Color(0.9f, 0.9f, 1f), 18, 4f, 0.45f, 0.4f, 0f, 0.3f);
            var hurtFx = CreateBurst("FX_Hurt", new Color(1f, 0.3f, 0.2f), new Color(1f, 0.7f, 0.2f), 24, 5f, 0.35f, 0.6f, 1f, 0.3f);
            var respawnFx = CreateBurst("FX_Respawn", new Color(0.4f, 0.9f, 1f), white, 30, 2.5f, 0.35f, 0.8f, -0.5f, 0.6f);
            var checkpointFx = CreateBurst("FX_Checkpoint", new Color(0.3f, 1f, 0.4f), new Color(1f, 0.95f, 0.3f), 30, 5f, 0.3f, 1.2f, 1f, 0.3f);
            var jumpPadFx = CreateBurst("FX_JumpPad", new Color(0.75f, 0.4f, 1f), new Color(1f, 0.6f, 0.9f), 20, 4f, 0.35f, 0.5f, 0.5f, 0.6f);
            var crackFx = CreateBurst("FX_Crack", new Color(0.55f, 0.4f, 0.25f), new Color(0.8f, 0.6f, 0.4f), 12, 1.5f, 0.4f, 0.5f, 0.5f, 0.8f);
            var clearFx = CreateBurst("FX_Clear", white, white, 80, 9f, 0.35f, 2f, 1f, 1f, Confetti());

            var footsteps = new string[10];
            for (int i = 0; i < footsteps.Length; i++)
                footsteps[i] = RobotSfx + $"Player_Footstep_{i + 1:00}.wav";

            var serialized = new SerializedObject(library);
            var entries = serialized.FindProperty("entries");
            entries.arraySize = 0;
            AddEntry(entries, Feedback.Jump, new[] { SfxFolder + "sfx_jump.wav" }, 0.5f, 0.06f, dustFx);
            AddEntry(entries, Feedback.DoubleJump, new[] { SfxFolder + "sfx_double_jump.wav" }, 0.5f, 0.06f, dustFx);
            AddEntry(entries, Feedback.Dash, new[] { SfxFolder + "sfx_dash.wav" }, 0.6f, 0.08f, null);
            AddEntry(entries, Feedback.Land, new[] { RobotSfx + "Player_Land.wav" }, 0.5f, 0.08f, dustFx);
            AddEntry(entries, Feedback.Footstep, footsteps, 0.25f, 0.08f, null);
            AddEntry(entries, Feedback.Coin, new[] { SfxFolder + "sfx_coin.wav" }, 0.45f, 0.03f, coinFx);
            AddEntry(entries, Feedback.Star, new[] { SfxFolder + "sfx_star.wav" }, 0.7f, 0f, starFx);
            AddEntry(entries, Feedback.Checkpoint, new[] { SfxFolder + "sfx_checkpoint.wav" }, 0.7f, 0f, checkpointFx);
            AddEntry(entries, Feedback.Stomp, new[] { SfxFolder + "sfx_stomp.wav" }, 0.8f, 0.08f, stompFx);
            AddEntry(entries, Feedback.Hurt, new[] { SfxFolder + "sfx_hurt.wav" }, 0.6f, 0.03f, hurtFx);
            AddEntry(entries, Feedback.Respawn, new[] { TemplateSfx + "SFX_Respawn.ogg" }, 0.6f, 0f, respawnFx);
            AddEntry(entries, Feedback.JumpPad, new[] { SfxFolder + "sfx_jump_pad.wav" }, 0.7f, 0.05f, jumpPadFx);
            AddEntry(entries, Feedback.Knockback, new[] { SfxFolder + "sfx_knockback.wav" }, 0.7f, 0.06f, stompFx);
            AddEntry(entries, Feedback.PlatformCrack, new[] { SfxFolder + "sfx_crack.wav" }, 0.5f, 0.1f, crackFx);
            AddEntry(entries, Feedback.Clear, new[] { SfxFolder + "sfx_clear.wav" }, 0.8f, 0f, clearFx);
            AddEntry(entries, Feedback.UIClick, new[] { SfxFolder + "sfx_click.wav" }, 0.5f, 0.05f, null);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            return library;
        }

        static void AddEntry(SerializedProperty entries, Feedback type, string[] clipPaths, float volume,
            float pitchVariation, ParticleSystem effect)
        {
            int index = entries.arraySize;
            entries.arraySize = index + 1;
            var entry = entries.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("type").enumValueIndex = (int)type;
            entry.FindPropertyRelative("volume").floatValue = volume;
            entry.FindPropertyRelative("pitchVariation").floatValue = pitchVariation;
            entry.FindPropertyRelative("effect").objectReferenceValue = effect;

            var clips = entry.FindPropertyRelative("clips");
            clips.arraySize = clipPaths.Length;
            for (int i = 0; i < clipPaths.Length; i++)
                clips.GetArrayElementAtIndex(i).objectReferenceValue = LoadAsset<AudioClip>(clipPaths[i]);
        }

        /// <summary>색종이처럼 여러 색 중 무작위</summary>
        static Gradient Confetti()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.35f, 0.35f), 0f),
                    new GradientColorKey(new Color(1f, 0.85f, 0.25f), 0.33f),
                    new GradientColorKey(new Color(0.35f, 0.9f, 0.45f), 0.66f),
                    new GradientColorKey(new Color(0.4f, 0.6f, 1f), 1f),
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return gradient;
        }

        /// <summary>
        /// 한 번 "펑" 터지고 스스로 사라지는 파티클 프리팹.
        /// palette를 주면 그 색들 중 무작위, 아니면 colorA~colorB 사이 무작위 색
        /// </summary>
        static ParticleSystem CreateBurst(string name, Color colorA, Color colorB, int count, float speed, float size,
            float lifetime, float gravity, float radius, Gradient palette = null)
        {
            var effect = new GameObject(name);
            var particles = effect.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = particles.main;
            main.duration = Mathf.Max(0.1f, lifetime);
            main.loop = false;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.6f, lifetime);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.4f, speed);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.6f, size);
            main.startColor = palette != null
                ? new ParticleSystem.MinMaxGradient(palette) { mode = ParticleSystemGradientMode.RandomColor }
                : new ParticleSystem.MinMaxGradient(colorA, colorB);
            main.gravityModifier = gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy; // 다 터지면 스스로 사라짐
            main.maxParticles = count;

            var emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;

            // 점점 작아지고 투명해지며 사라짐
            var sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = fade;

            var renderer = effect.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(ParticleMaterialPath);

            return SavePrefab(effect, EffectFolder).GetComponent<ParticleSystem>();
        }
    }
}
