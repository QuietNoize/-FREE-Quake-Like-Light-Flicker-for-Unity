namespace QuietNoize.QuakeLikeLightFlicker.Demo
{
    using UnityEngine;
    using TMPro;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [RequireComponent(typeof(TextMeshPro))]
    public class PresetTextMeshDisplayer : MonoBehaviour
    {
        private TextMeshPro m_textMesh;
        [SerializeField] private LightFlicker m_flicker;

        void Awake()
        {
            SetupTextMesh();
            SetupText();
        }

        private void SetupText()
        {
            if (m_flicker == null)
            {
                Debug.LogError(
                    $"[{nameof(PresetTextMeshDisplayer)}] " +
                    $"{nameof(QuakeLikeLightFlicker)} reference is missing.",
                    this
                );
                return;
            }

            var preset = m_flicker.Preset;
            m_textMesh.text = preset == null ? string.Empty : $"{preset.name}\nPattern:\n{preset.pattern}";
        }

        private void SetupTextMesh()
        {
            if (m_textMesh == null)
            {
                m_textMesh = GetComponent<TextMeshPro>();
            }

            m_textMesh.richText = true;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (EditorUtility.IsPersistent(gameObject))
            {
                return;
            }

            SetupTextMesh();
            SetupText();
        }
#endif
    }
}
