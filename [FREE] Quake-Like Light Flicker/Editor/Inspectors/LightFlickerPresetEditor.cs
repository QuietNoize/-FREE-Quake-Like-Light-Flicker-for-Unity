namespace QuietNoize.QuakeLikeLightFlicker.Editor
{
#if UNITY_EDITOR
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom inspector for LightFlickerPreset with a simple pattern preview.
    /// </summary>
    [CustomEditor(typeof(LightFlickerPreset))]
    public class LightFlickerPresetEditor : Editor
    {
        #region References

        /// <summary>
        /// Cached target preset being edited.
        /// </summary>
        private LightFlickerPreset m_targetScript;

        /// <summary>
        /// Dataset used to resolve preview textures for pattern characters.
        /// </summary>
        private LightFlickerPreviewImageDataset m_previewImageDataset;

        #endregion

        #region Serialized Properties

        /// <summary>
        /// Serialized pattern string property.
        /// </summary>
        private SerializedProperty m_patternProp;

        #endregion

        #region Preview State

        /// <summary>
        /// Accumulated time for preview stepping.
        /// </summary>
        private float m_stepTimer;

        /// <summary>
        /// Current preview step index.
        /// </summary>
        private int m_step;

        /// <summary>
        /// Character currently shown in preview.
        /// </summary>
        private char m_stepChar;

        /// <summary>
        /// Texture currently displayed in the preview area.
        /// </summary>
        private Texture2D m_stepPreviewTexture;

        /// <summary>
        /// Time in seconds between preview frames.
        /// </summary>
        private float m_stepInterval = 0.1f;

        #endregion

        #region Styles

        /// <summary>
        /// Header label style used by the preview section.
        /// </summary>
        private GUIStyle m_headerStyle;

        /// <summary>
        /// Centered label style used by preview text.
        /// </summary>
        private GUIStyle m_centerLabelStyle;
        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Initializes cached references and subscribes to editor updates.
        /// </summary>
        void OnEnable()
        {
            m_targetScript = (LightFlickerPreset)target;
            m_patternProp = serializedObject.FindProperty("m_pattern");

            var folder = LightFlickerEditorHelper.GetAssetFolder();

            m_previewImageDataset =
                AssetDatabase.FindAssets("t:LightFlickerPreviewImageDataset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.StartsWith(folder))
                .Select(p => AssetDatabase.LoadAssetAtPath<LightFlickerPreviewImageDataset>(p))
                .FirstOrDefault();

            EditorApplication.update += PreviewUpdate;
        }

        /// <summary>
        /// Stops preview playback and unsubscribes from editor updates.
        /// </summary>
        private void OnDisable()
        {
            ResetPreview();

            EditorApplication.update -= PreviewUpdate;
        }

        /// <summary>
        /// Draws the custom inspector UI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            InitStyles();

            serializedObject.Update();

            if (m_patternProp == null)
            {
                EditorGUILayout.HelpBox(
                    "SerializedProperty not found. Check field names in FindProperty() or reload the Inspector.",
                    MessageType.Error
                );
                return;
            }

            EditorGUILayout.PropertyField(m_patternProp);

            serializedObject.ApplyModifiedProperties();

            LightFlickerEditorHelper.DrawSeparatorLine(6);

            EditorGUILayout.HelpBox(
                "Edit the pattern string to generate the flicker preview.",
                MessageType.Info
            );

            LightFlickerEditorHelper.DrawSeparatorLine(6);
            DrawPreview();
        }

        #endregion

        #region Preview Update

        /// <summary>
        /// Advances the preview over time and repaints the inspector.
        /// </summary>
        private void PreviewUpdate()
        {
            m_stepTimer += Time.unscaledDeltaTime;

            EditorApplication.QueuePlayerLoopUpdate();

            if (m_stepTimer >= m_stepInterval)
            {
                m_stepTimer -= m_stepInterval;
                MoveStep(+1);
            }

            Repaint();
        }

        #endregion

        #region GUI Drawing

        /// <summary>
        /// Draws the preview panel.
        /// </summary>
        private void DrawPreview()
        {
            var oldColor = GUI.color;
            GUI.color = Color.aliceBlue;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Space(6);
                DrawPreviewHeader();
                DrawPreviewBody();
                GUILayout.Space(6);
            }

            GUI.color = oldColor;
        }

        /// <summary>
        /// Draws the preview header.
        /// </summary>
        private void DrawPreviewHeader()
        {
            EditorGUILayout.LabelField("PREVIEW", m_headerStyle);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(
                    "Simulates flicker sequence for tuning and debugging.",
                    EditorStyles.wordWrappedMiniLabel
                );
                GUILayout.FlexibleSpace();
            }

            LightFlickerEditorHelper.DrawSeparatorLine(3);
        }

        /// <summary>
        /// Draws the main preview body.
        /// </summary>
        private void DrawPreviewBody()
        {
            int len = m_patternProp.stringValue.Length;

            if (len == 0)
            {
                DrawPreviewEmptyState();
                return;
            }

            DrawPreviewImage();
            DrawPreviewInfo(len);
            DrawControls();
        }

        /// <summary>
        /// Draws the empty-pattern warning state.
        /// </summary>
        private void DrawPreviewEmptyState()
        {
            EditorGUILayout.HelpBox(
                "Pattern is empty. Add at least one value to enable preview.",
                MessageType.Warning
            );
        }

        /// <summary>
        /// Draws the preview image area.
        /// </summary>
        private void DrawPreviewImage()
        {
            Rect rect = GUILayoutUtility.GetRect(512, 512);

            if (m_stepPreviewTexture != null)
            {
                EditorGUI.DrawTextureTransparent(
                    rect,
                    m_stepPreviewTexture,
                    ScaleMode.ScaleToFit
                );
            }
            else
            {
                EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f));
                EditorGUI.LabelField(rect, "NO PREVIEW", m_centerLabelStyle);
            }

            LightFlickerEditorHelper.DrawSeparatorLine(3);
        }

        /// <summary>
        /// Draws the current preview step and intensity information.
        /// </summary>
        /// <param name="len">Current length of flicker pattern string.</param>
        private void DrawPreviewInfo(int len)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField(
                    $"Step {m_step + 1}/{len} • Intensity {(Utils.GetIntensityFromChar(m_stepChar) * 100f):0}%",
                    m_centerLabelStyle
                );

                GUILayout.FlexibleSpace();
            }

            LightFlickerEditorHelper.DrawSeparatorLine(3);
        }

        /// <summary>
        /// Draws preview controls.
        /// </summary>
        private void DrawControls()
        {
            m_stepInterval = EditorGUILayout.Slider(
                "Step Interval (in seconds)",
                m_stepInterval,
                0.01f,
                1f
            );
        }

        #endregion

        #region Preview Control

        /// <summary>
        /// Moves the preview forward or backward by one step.
        /// </summary>
        /// <param name="delta">Step direction.</param>
        private void MoveStep(int delta)
        {
            string pattern = m_patternProp.stringValue;

            int len = pattern.Length;
            if (len == 0)
            {
                Debug.LogWarning(
                    $"[LightFlickerPresetEditor] Pattern is empty on '{m_targetScript?.name}'. Cannot update preview step."
                );
                return;
            }

            m_step = (m_step + delta) % len;
            if (m_step < 0)
            {
                m_step += len;
            }

            m_stepChar = pattern[m_step];
            m_stepPreviewTexture = m_previewImageDataset.GetPreview(m_stepChar);
        }

        /// <summary>
        /// Resets the preview state.
        /// </summary>
        private void ResetPreview()
        {
            m_stepTimer = 0f;
            m_step = 0;
        }

        #endregion

        #region Styles Control

        /// <summary>
        /// Initializes editor GUI styles.
        /// </summary>
        private void InitStyles()
        {
            if (m_headerStyle != null && m_centerLabelStyle != null) return;

            var boldStyle = EditorStyles.boldLabel != null ? EditorStyles.boldLabel : EditorStyles.label;
            var miniStyle = EditorStyles.miniLabel != null ? EditorStyles.miniLabel : EditorStyles.label;

            string assetFolder = LightFlickerEditorHelper.GetAssetFolder();
            var headerFont = AssetDatabase.LoadAssetAtPath<Font>($"{assetFolder}/Editor/Inspectors/Fonts/header.ttf");

            m_headerStyle = new GUIStyle(boldStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                font = headerFont,
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };

            m_centerLabelStyle = new GUIStyle(miniStyle)
            {
                alignment = TextAnchor.MiddleCenter
            };
        }

        #endregion
    }
#endif
}