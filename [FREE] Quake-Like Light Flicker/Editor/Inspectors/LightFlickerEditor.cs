namespace QuietNoize.QuakeLikeLightFlicker.Editor
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom inspector for LightFlicker with preview controls and preset saving.
    /// </summary>
    [CustomEditor(typeof(LightFlicker), true)]
    public class LightFlickerEditor : Editor
    {
        #region References

        /// <summary>
        /// Cached target component being edited.
        /// </summary>
        private LightFlicker m_targetScript;

        #endregion

        #region Serialized Properties

        /// <summary>
        /// Serialized script property.
        /// </summary>
        private SerializedProperty m_scriptProp;

        /// <summary>
        /// Serialized preset reference.
        /// </summary>
        private SerializedProperty m_presetProp;

        /// <summary>
        /// Serialized pattern string.
        /// </summary>
        private SerializedProperty m_patternProp;

        /// <summary>
        /// Serialized cached intensity buffer.
        /// </summary>
        private SerializedProperty m_intensityBufferProp;

        /// <summary>
        /// Serialized step interval value.
        /// </summary>
        private SerializedProperty m_stepIntervalProp;

        #endregion

        #region Preview State

        /// <summary>
        /// Accumulated preview time.
        /// </summary>
        private float m_stepTimer;

        /// <summary>
        /// Current preview step index.
        /// </summary>
        private int m_step;

        /// <summary>
        /// Whether the preview section is expanded.
        /// </summary>
        private bool m_isPreviewExpanded;

        /// <summary>
        /// Whether the preview animation is playing.
        /// </summary>
        private bool m_isPreviewPlaying;

        /// <summary>
        /// Current preview intensity value.
        /// </summary>
        private float m_previewIntensity;

        #endregion

        #region Styles

        /// <summary>
        /// Style used for the preview header label.
        /// </summary>
        private GUIStyle m_headerStyle;

        /// <summary>
        /// Centered label style used in the preview area.
        /// </summary>
        private GUIStyle m_centerLabelStyle;

        /// <summary>
        /// GUI style used for the foldout toggle button (expand/collapse arrow) in the preview header.
        /// </summary>
        private GUIStyle m_foldoutButtonStyle;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Initializes cached references and subscribes to the editor update loop.
        /// </summary>
        void OnEnable()
        {
            m_targetScript = (LightFlicker)target;

            m_scriptProp = serializedObject.FindProperty("m_Script");
            m_presetProp = serializedObject.FindProperty("m_preset");
            m_patternProp = serializedObject.FindProperty("m_pattern");
            m_intensityBufferProp = serializedObject.FindProperty("m_intensityBuffer");
            m_stepIntervalProp = serializedObject.FindProperty("m_stepInterval");

            EditorApplication.update += PreviewUpdate;
        }

        /// <summary>
        /// Resets preview state and unsubscribes from the editor update loop.
        /// </summary>
        private void OnDisable()
        {
            ResetPreview();
            m_isPreviewExpanded = false;

            EditorApplication.update -= PreviewUpdate;
        }

        /// <summary>
        /// Draws the custom inspector UI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            InitStyles();

            serializedObject.Update();

            if (m_scriptProp == null ||
                m_presetProp == null ||
                m_patternProp == null)
            {
                EditorGUILayout.HelpBox(
                    "SerializedProperty not found. Check field names in FindProperty() or reload the Inspector.",
                    MessageType.Error
                );
                return;
            }

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_scriptProp);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(m_presetProp);
            bool hasPreset = m_presetProp.objectReferenceValue != null;

            if (hasPreset)
            {
                // If a preset is assigned, lock the pattern field to prevent manual edits
                GUI.enabled = false;
                EditorGUILayout.PropertyField(m_patternProp);
                GUI.enabled = true;
            }
            else
            {
                // Allow editing the pattern string and saving it as a preset
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(m_patternProp);
                if (GUILayout.Button("Save", GUILayout.Width(60)))
                {
                    SavePatternAsPreset();
                }
                EditorGUILayout.EndHorizontal();
            }

            DrawPropertiesExcluding(
            serializedObject,
                "m_Script",
                "m_preset",
                "m_pattern"
            );

            serializedObject.ApplyModifiedProperties();

            LightFlickerEditorHelper.DrawSeparatorLine(6);

            EditorGUILayout.HelpBox(
                "Use a preset or define a pattern string to drive the lighting flicker.",
                MessageType.Info
            );

            LightFlickerEditorHelper.DrawSeparatorLine(6);
            DrawPreviewMenu();
        }

        #endregion

        #region Preview Update

        /// <summary>
        /// Advances the preview animation while the inspector is active.
        /// </summary>
        private void PreviewUpdate()
        {
            if (!UnityEngine.Application.isPlaying && m_isPreviewPlaying)
            {
                m_stepTimer += Time.unscaledDeltaTime;

                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();

                float frameInterval = m_stepIntervalProp.floatValue;

                if (m_stepTimer >= frameInterval)
                {
                    m_stepTimer -= frameInterval;
                    MoveStep(+1);
                }
            }
        }

        #endregion

        #region GUI Drawing

        /// <summary>
        /// Draws the preview container.
        /// </summary>
        private void DrawPreviewMenu()
        {
            var oldColor = GUI.color;
            GUI.color = Color.aliceBlue;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Space(3);

                DrawPreviewMenuHeader();

                if (m_isPreviewExpanded)
                {
                    DrawPreviewMenuBody();
                }

                GUILayout.Space(3);
            }

            GUI.color = oldColor;
        }

        /// <summary>
        /// Draws the preview menu header.
        /// </summary>
        private void DrawPreviewMenuHeader()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(3);

                bool isPrevExpanded = m_isPreviewExpanded;

                m_isPreviewExpanded = GUILayout.Toggle(
                    m_isPreviewExpanded,
                    "Q",
                    m_foldoutButtonStyle,
                    GUILayout.Width(20)
                );

                if (isPrevExpanded && !m_isPreviewExpanded)
                {
                    ResetPreview();
                }

                Rect rect = GUILayoutUtility.GetRect(1, 20, GUILayout.ExpandWidth(true));

                EditorGUI.LabelField(
                    rect,
                    "PREVIEW MENU",
                    m_headerStyle
                );
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(
                    "Simulates flicker sequence for tuning and debugging.",
                    EditorStyles.wordWrappedMiniLabel
                );
                GUILayout.FlexibleSpace();
            }
        }

        /// <summary>
        /// Draws the main preview body.
        /// </summary>
        private void DrawPreviewMenuBody()
        {
            LightFlickerEditorHelper.DrawSeparatorLine(3);

            int len = m_patternProp.stringValue.Length;

            if (UnityEngine.Application.isPlaying)
            {
                DrawPreviewDisabledPlayMode();
                return;
            }

            if (len == 0)
            {
                DrawPreviewEmptyState();
                return;
            }

            DrawPreviewControls();
            DrawPreviewInfo(len);
        }

        /// <summary>
        /// Draws a warning that preview is disabled in Play Mode.
        /// </summary>
        private void DrawPreviewDisabledPlayMode()
        {
            EditorGUILayout.HelpBox(
                "Preview controls are disabled during Play Mode.",
                MessageType.Warning
            );
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

            if (m_isPreviewPlaying)
                ResetPreview();
        }

        /// <summary>
        /// Draws playback navigation controls.
        /// </summary>
        private void DrawPreviewControls()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("⏮", GUILayout.Width(60)))
                    MoveStep(-1);

                GUILayout.Space(4);

                m_isPreviewPlaying = GUILayout.Toggle(
                    m_isPreviewPlaying,
                    m_isPreviewPlaying ? "❚❚ PAUSE" : "▶ PLAY",
                    "Button",
                    GUILayout.Width(120)
                );

                GUILayout.Space(4);

                if (GUILayout.Button("⏭", GUILayout.Width(60)))
                    MoveStep(+1);

                GUILayout.FlexibleSpace();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("■ STOP", GUILayout.Width(248)))
                    ResetPreview();

                GUILayout.FlexibleSpace();
            }
        }

        /// <summary>
        /// Draws the current preview step and intensity information.
        /// </summary>
        /// <param name="len">Pattern length.</param>
        private void DrawPreviewInfo(int len)
        {
            LightFlickerEditorHelper.DrawSeparatorLine(3);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField(
                    $"Step: {m_step + 1}/{len}   •   Intensity {(m_previewIntensity * 100f):0}%",
                    m_centerLabelStyle
                );

                GUILayout.FlexibleSpace();
            }
        }

        #endregion

        #region Preview Control

        /// <summary>
        /// Moves the preview one step forward or backward.
        /// </summary>
        /// <param name="delta">Step offset.</param>
        private void MoveStep(int delta)
        {
            string pattern = m_patternProp.stringValue;

            int len = pattern.Length;
            if (len == 0)
            {
                Debug.LogWarning(
                    $"[LightFlickerEditor] Pattern is empty on '{m_targetScript?.name}'. " +
                    $"Cannot update preview step."
                );
                return;
            }

            m_step = (m_step + delta) % len;
            if (m_step < 0)
            {
                m_step += len;
            }

            SerializedProperty intensityElement = null;

            if (m_intensityBufferProp != null &&
                m_step >= 0 &&
                m_step < m_intensityBufferProp.arraySize)
            {
                intensityElement = m_intensityBufferProp.GetArrayElementAtIndex(m_step);
            }

            m_previewIntensity = intensityElement != null ? intensityElement.floatValue : 0f;

            if (m_targetScript != null)
            {
                m_targetScript.ApplyPreviewIntensity(m_previewIntensity);
            }
        }

        /// <summary>
        /// Stops preview playback and resets preview state.
        /// </summary>
        private void ResetPreview()
        {
            m_isPreviewPlaying = false;
            m_stepTimer = 0f;
            m_step = 0;
            m_previewIntensity = 0f;

            if (m_targetScript != null)
            {
                m_targetScript.ResetFlicker();
            }
        }

        #endregion

        #region Styles Control

        /// <summary>
        /// Initializes editor GUI styles.
        /// </summary>
        private void InitStyles()
        {
            if (m_headerStyle != null && m_centerLabelStyle != null) return;

            var bold = EditorStyles.boldLabel != null ? EditorStyles.boldLabel : EditorStyles.label;
            var mini = EditorStyles.miniLabel != null ? EditorStyles.miniLabel : EditorStyles.label;


            string assetFolder = LightFlickerEditorHelper.GetAssetFolder();
            var headerFont = AssetDatabase.LoadAssetAtPath<Font>($"{assetFolder}/Editor/Inspectors/Fonts/header.ttf");

            m_headerStyle = new GUIStyle(bold)
            {
                alignment = TextAnchor.MiddleCenter,
                font = headerFont,
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };

            m_centerLabelStyle = new GUIStyle(mini)
            {
                alignment = TextAnchor.MiddleCenter
            };

            m_foldoutButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                font = headerFont,
                fontSize = 12,
                fixedWidth = 20,
                fixedHeight = 18,
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0)
            };
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Creates a new preset asset from the current pattern and saves it to the project.
        /// </summary>
        private void SavePatternAsPreset()
        {
            string assetFolder = LightFlickerEditorHelper.GetAssetFolder();
            string presetsFolder = $"{assetFolder}/Runtime/Data/Presets";

            if (!AssetDatabase.IsValidFolder(presetsFolder))
            {
                AssetDatabase.CreateFolder(assetFolder, "Runtime/Data/Presets");
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Light Flicker Preset",
                "New Quake-Like Light Flicker Preset",
                "asset",
                "Choose location for preset",
                presetsFolder
            );

            if (string.IsNullOrEmpty(path)) return;

            var preset = LightFlickerPreset.Create(m_patternProp.stringValue);
            AssetDatabase.CreateAsset(preset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (m_targetScript != null)
            {
                m_targetScript.ChangePreset(preset);
            }
        }

        #endregion
    }
#endif
}