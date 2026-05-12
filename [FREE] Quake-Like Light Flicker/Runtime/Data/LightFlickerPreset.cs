namespace QuietNoize.QuakeLikeLightFlicker
{
    using UnityEngine;

    /// <summary>
    /// ScriptableObject preset that stores a light flicker pattern.
    /// </summary>
    [CreateAssetMenu(fileName = "LightFlickerPreset", menuName = "Quake-Like Light Flicker/Preset")]
    public class LightFlickerPreset : ScriptableObject
    {
        [SerializeField] private string m_pattern;

        /// <summary>
        /// Gets the stored flicker pattern.
        /// </summary>
        public string pattern => m_pattern;

        /// <summary>
        /// Creates a new preset instance in memory and assigns the given pattern.
        /// </summary>
        public static LightFlickerPreset Create(string pattern)
        {
            var preset = CreateInstance<LightFlickerPreset>();
            preset.m_pattern = pattern;
            return preset;
        }
    }

}