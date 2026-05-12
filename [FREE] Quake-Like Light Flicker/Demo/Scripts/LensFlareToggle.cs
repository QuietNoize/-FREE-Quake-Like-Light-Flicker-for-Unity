namespace QuietNoize.QuakeLikeLightFlicker.Demo
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.UI;

    [RequireComponent(typeof(Toggle))]
    public class LensFlareToggle : MonoBehaviour
    {
        private Toggle m_toggle;
        [SerializeField] private LensFlareComponentSRP[] m_lensFlareList;

        void Awake()
        {
            m_toggle = GetComponent<Toggle>();

            m_lensFlareList = FindObjectsByType<LensFlareComponentSRP>(FindObjectsInactive.Include);
        }

        void OnEnable()
        {
            if (m_toggle != null)
            {
                m_toggle.onValueChanged.AddListener(Switch);
            }
        }

        void OnDisable()
        {
            if (m_toggle != null)
            {
                m_toggle.onValueChanged.RemoveListener(Switch);
            }
        }

        private void Switch(bool value)
        {
            foreach (var lensFlare in m_lensFlareList)
            {
                lensFlare.enabled = value;
            }
        }
    }
}
