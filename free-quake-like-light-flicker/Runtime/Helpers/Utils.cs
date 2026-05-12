namespace QuietNoize.QuakeLikeLightFlicker
{
    using UnityEngine;

    /// <summary>
    /// Utility methods for converting flicker pattern characters into intensity values.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Converts a character (a–z) into a normalized intensity value (0–1).
        /// </summary>
        /// <param name="ch">Lowercase letter representing intensity level.</param>
        /// <returns>Normalized intensity value in range [0, 1].</returns>
        public static float GetIntensityFromChar(char ch)
        {
            int charIndex = ch - 'a';
            return Mathf.Clamp01(charIndex / 25f);
        }
    }

}