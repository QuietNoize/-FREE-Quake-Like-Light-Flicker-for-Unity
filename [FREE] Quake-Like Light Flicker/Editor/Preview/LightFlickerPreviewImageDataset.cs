namespace QuietNoize.QuakeLikeLightFlicker.Editor
{
#if UNITY_EDITOR
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Stores preview textures for individual pattern characters.
    /// </summary>
    public class LightFlickerPreviewImageDataset : ScriptableObject
    {
        /// <summary>
        /// Serializable mapping entry between a character and its preview texture.
        /// </summary>
        [Serializable]
        private class Entry
        {
            /// <summary>
            /// Pattern character key.
            /// </summary>
            public char ch;

            /// <summary>
            /// Preview texture associated with the character.
            /// </summary>
            public Texture2D preview;

            /// <summary>
            /// Creates a new mapping entry.
            /// </summary>
            /// <param name="ch">Pattern character.</param>
            /// <param name="preview">Preview texture.</param>
            public Entry(char ch, Texture2D preview)
            {
                this.ch = ch;
                this.preview = preview;
            }
        }

        /// <summary>
        /// Serialized list of character-to-preview mappings.
        /// </summary>
        [SerializeField] private List<Entry> m_entries = new List<Entry>();

        /// <summary>
        /// Adds a new preview mapping or replaces an existing one for the same character.
        /// </summary>
        /// <param name="ch">Pattern character.</param>
        /// <param name="preview">Preview texture to store.</param>
        public void AddEntry(char ch, Texture2D preview)
        {
            var entry = m_entries.FirstOrDefault(e => e.ch == ch);

            if (entry == null)
            {
                entry = new Entry(ch, preview);
                m_entries.Add(entry);
                return;
            }

            entry.preview = preview;
        }

        /// <summary>
        /// Returns the preview texture assigned to the given character.
        /// </summary>
        /// <param name="ch">Pattern character.</param>
        /// <returns>The matching preview texture, or null if none is found.</returns>
        public Texture2D GetPreview(char ch)
        {
            var entry = m_entries.FirstOrDefault(e => e.ch == ch);

            if (entry == null)
            {
                Debug.LogWarning($"[{this.name}] Preview for character '{ch}' was not found in the dataset.");
                return null;
            }

            return entry.preview;
        }
    }
#endif
}