namespace QuietNoize.QuakeLikeLightFlicker
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Light))]
    public class LightFlicker : MonoBehaviour
    {
        #region Serialized Fields

        /// <summary>
        /// Preset source. If assigned, it overrides the manual pattern string.
        /// </summary>
        [Header("General")]
        [Tooltip("Preset source. If assigned, overrides pattern string.")]
        [SerializeField] private LightFlickerPreset m_preset;

        /// <summary>
        /// Flicker string pattern, where characters map to intensity values.
        /// </summary>
        [Tooltip("Flicker string pattern (a–z mapped to 0–1).")]
        [SerializeField] private string m_pattern = "mmamammmmammamamaaamammma";

        /// <summary>
        /// Precomputed intensity values derived from the current pattern.
        /// </summary>
        [Tooltip("Precomputed intensity values derived from the pattern string.")]
        [SerializeField] private List<float> m_intensityBuffer = new List<float>();

        /// <summary>
        /// Maximum light intensity multiplier.
        /// </summary>
        [Header("Flicker Sequence Settings")]
        [Tooltip("Maximum light intensity multiplier.")]
        [SerializeField] private float m_maxIntensity = 2f;

        /// <summary>
        /// Time between pattern steps in seconds.
        /// </summary>
        [Tooltip("Time between pattern steps (in seconds).")]
        [SerializeField] private float m_stepInterval = 0.1f;

        /// <summary>
        /// Determines whether the effect should ignore Time.timeScale.
        /// </summary>
        [Tooltip("Ignore TimeScale when updating flicker.")]
        [SerializeField] private bool m_ignoreTimeScale = false;

        #endregion

        #region Private Fields

        /// <summary>
        /// Cached Light component.
        /// </summary>
        private Light m_light;

        /// <summary>
        /// Cached LensFlare component.
        /// </summary>
        private LensFlareComponentSRP m_lensFlare;

        /// <summary>
        /// Accumulated time for stepping through the flicker sequence.
        /// </summary>
        private float m_stepTimer;

        /// <summary>
        /// Current step index in the flicker sequence.
        /// </summary>
        private int m_step = 0;

        #endregion

        #region Public Properties

        public LightFlickerPreset Preset => m_preset;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Initializes component references and builds the flicker pattern.
        /// </summary>
        void Awake()
        {
            SetupLight();
            InitializePattern();
        }

        /// <summary>
        /// Advances the flicker sequence during play mode.
        /// </summary>
        void Update()
        {
            m_stepTimer += m_ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;

            if (m_stepTimer >= m_stepInterval)
            {
                m_stepTimer -= m_stepInterval;
                m_step = m_pattern.Length != 0 ? (m_step + 1) % m_pattern.Length : 0;

                MoveStep(m_step);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Rebuilds cached data when values are changed in the Inspector.
        /// </summary>
        private void OnValidate()
        {
            SetupLight();
            InitializePattern();
        }
#endif

        #endregion

        #region Initialization

        /// <summary>
        /// Caches the Light component if it has not been cached yet.
        /// </summary>
        private void SetupLight()
        {
            if (m_light == null)
            {
                m_light = GetComponent<Light>();
            }
        }

        /// <summary>
        /// Builds the intensity buffer from the current preset or pattern string.
        /// </summary>
        private void InitializePattern()
        {
            m_intensityBuffer.Clear();

            if (m_preset != null)
            {
                m_pattern = m_preset.pattern;
            }

            foreach (var ch in m_pattern)
            {
                float intensity = Utils.GetIntensityFromChar(ch);
                m_intensityBuffer.Add(intensity);
            }

            m_stepTimer = 0f;
            m_step = 0;

            MoveStep(0);
        }

        #endregion

        #region Flicker Logic

        /// <summary>
        /// Applies the intensity for the specified step.
        /// </summary>
        /// <param name="step">Index of the intensity step to apply.</param>
        private void MoveStep(int step)
        {
            if (m_intensityBuffer == null || m_intensityBuffer.Count == 0)
            {
                Debug.LogWarning(
                    $"[QuakeLikeLightFlicker] Invalid state on '{gameObject.name}': " +
                    "intensity buffer is empty. Ensure pattern or preset is initialized."
                );

                ApplyIntensity(0f);
                return;
            }

            step = Mathf.Clamp(step, 0, m_intensityBuffer.Count - 1);

            float intensity = m_intensityBuffer[step] * m_maxIntensity;
            ApplyIntensity(intensity);
        }

        /// <summary>
        /// Applies the given intensity to the Light and optional flare components.
        /// </summary>
        /// <param name="intensity">Intensity value to apply.</param>
        private void ApplyIntensity(float intensity)
        {
            m_light.intensity = intensity;

            var lensFlare = FindLensFlare();
            if (lensFlare != null)
            {
                lensFlare.intensity = intensity;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Assigns a new preset and rebuilds the flicker pattern.
        /// </summary>
        /// <param name="preset">Preset to use.</param>
        public void ChangePreset(LightFlickerPreset preset)
        {
            if (m_preset == preset) return;

            m_preset = preset;
            InitializePattern();
        }

        /// <summary>
        /// Sets a new flicker step interval.
        /// </summary>
        /// <param name="value">Interval between pattern steps in seconds.</param>
        public void ChangeStepInterval(float value)
        {
            m_stepInterval = value;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Applies a preview intensity from the custom editor.
        /// </summary>
        /// <param name="intensity">Normalized preview intensity.</param>
        public void ApplyPreviewIntensity(float intensity) => ApplyIntensity(intensity * m_maxIntensity);

        /// <summary>
        /// Resets the flicker back to the first step.
        /// </summary>
        public void ResetFlicker() => MoveStep(0);
#endif

        #endregion

        #region Lens Flare Finding

        /// <summary>
        /// Finds and caches the LensFlare component.
        /// </summary>
        /// <returns>The cached LensFlare component, or null if none exists.</returns>
        private LensFlareComponentSRP FindLensFlare()
        {
            if (this == null || gameObject == null) return null;

            if (m_lensFlare == null)
            {
                m_lensFlare = GetComponent<LensFlareComponentSRP>();
            }

            return m_lensFlare;
        }
        #endregion
    }
}