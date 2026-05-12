namespace QuietNoize.QuakeLikeLightFlicker.Demo
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Slider))]
    public class StepIntervalSlider : MonoBehaviour
    {
        private Slider m_slider;
        private float m_cachedInterval = float.MinValue;
        [SerializeField] private TextMeshProUGUI m_textMeshPro;
        [SerializeField] private LightFlicker[] m_lightFlickerList;

        void Awake()
        {
            m_slider = GetComponent<Slider>();

            if (m_textMeshPro == null)
            {
                m_textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
            }

            m_lightFlickerList = FindObjectsByType<LightFlicker>(FindObjectsInactive.Include);
        }

        void Start()
        {
            float sliderValue = m_slider.value;
            ChangeGlobalInterval(sliderValue);
        }

        void OnEnable()
        {
            if (m_slider != null)
            {
                m_slider.onValueChanged.AddListener(ChangeGlobalInterval);
            }
        }

        void OnDisable()
        {
            if (m_slider != null)
            {
                m_slider.onValueChanged.RemoveListener(ChangeGlobalInterval);
            }
        }

        private void ChangeGlobalInterval(float value)
        {
            float roundedValue = Mathf.Round(value * 100f) / 100f;

            if (roundedValue != m_cachedInterval)
            {
                foreach (var lightFlicker in m_lightFlickerList)
                {
                    lightFlicker.ChangeStepInterval(roundedValue);
                }

                if (m_textMeshPro != null)
                {
                    m_textMeshPro.text = $"Global Step Interval: {roundedValue}";
                }

                m_cachedInterval = roundedValue;
            }
        }
    }
}
