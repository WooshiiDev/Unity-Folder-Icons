using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace FolderIcons
{
    [CustomEditor (typeof (FolderIconSettings))]
    internal class FolderIconSettingsEditor : Editor
    {
        // References
        private FolderIconSettings settings;
        private SerializedProperty serializedIcons;

        // Settings
        private bool showCustomFolders;
        private bool showCustomOverlay;

        private ReorderableList iconList;

        // Texture
        private Texture2D selectedTexture = null;
        private Texture2D previewTexture = null;

        private GUIContent previewContent = new GUIContent ();
        private RenderTexture previewRender;

        private Color replacementColour = Color.gray;

        // Texture Save Settings
        private string textureName = "New Texture";
        private string savePath;

        private int heightIndex;

        // Sizing
        private const float MAX_LABEL_WIDTH = 90f;
        private const float MAX_FIELD_WIDTH = 150f;

        private const float PROPERTY_HEIGHT = 19f;
        private const float PROPERTY_PADDING = 4f;
        private const float SETTINGS_PADDING = 1F;

        // Styling
        private GUIStyle elementStyle;

        private GUIStyle propertyStyle;
        private GUIStyle previewStyle;

        private void OnEnable()
        {
            if (target == null)
            {
                return;
            }

            settings = target as FolderIconSettings;
            serializedIcons = serializedObject.FindProperty ("icons");

            showCustomFolders = settings.showCustomFolder;
            showCustomOverlay = settings.showOverlay;

            if (iconList == null)
            {
                iconList = new ReorderableList (serializedObject, serializedIcons)
                {
                    drawHeaderCallback = OnHeaderDraw,

                    drawElementCallback = OnElementDraw,
                    drawElementBackgroundCallback = DrawElementBackground,

                    elementHeightCallback = GetPropertyHeight,

                    showDefaultBackground = false,
                };
            }

            savePath = Application.dataPath;

            if (selectedTexture != null)
            {
                UpdatePreview ();
            }
        }

        private void OnDisable()
        {
            ClearPreviewData ();
        }

        public override void OnInspectorGUI()
        {
            //Create styles
            if (previewStyle == null)
            {
                previewStyle = new GUIStyle (EditorStyles.label)
                {
                    fixedHeight = 64,
                    //fixedWidth = 64,
                    //stretchWidth = false,
                    alignment = TextAnchor.MiddleCenter
                };

                elementStyle = new GUIStyle (GUI.skin.box)
                {
                };
            }

            // Draw Settings
            EditorGUILayout.LabelField ("Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck ();
            {
                showCustomFolders = EditorGUILayout.ToggleLeft ("Show Folder Textures", showCustomFolders);
                showCustomOverlay = EditorGUILayout.ToggleLeft ("Show Overlay Textures", showCustomOverlay);
            }
            if (EditorGUI.EndChangeCheck ())
            {
                ApplySettings ();
            }

            EditorGUILayout.Space (16f);

            EditorGUI.BeginChangeCheck ();
            iconList.DoLayoutList ();
            if (EditorGUI.EndChangeCheck ())
            {
                serializedObject.ApplyModifiedProperties ();
            }

            DrawTexturePreview ();

            EditorGUI.BeginDisabledGroup (previewTexture == null);
            {
                EditorGUI.BeginChangeCheck ();
                {
                    replacementColour = EditorGUILayout.ColorField (new GUIContent ("Replacement Colour"), replacementColour);
                }
                if (EditorGUI.EndChangeCheck ())
                {
                    SetPreviewColour ();
                }

                DrawTextureSaving ();
            }
            EditorGUI.EndDisabledGroup ();
        }

        private void ApplySettings()
        {
            FolderIconsReplacer.showFolder = settings.showCustomFolder = showCustomFolders;
            FolderIconsReplacer.showOverlay = settings.showOverlay = showCustomOverlay;
        }

        #region Reorderable Array Draw

        private void OnHeaderDraw(Rect rect)
        {
            rect.y += 5f;
            rect.x -= 6f;
            rect.width += 12f;

            Handles.BeginGUI ();
            Handles.DrawSolidRectangleWithOutline (rect,
                new Color (0.15f, 0.15f, 0.15f, 1f),
                new Color (0.15f, 0.15f, 0.15f, 1f));
            Handles.EndGUI ();

            EditorGUI.LabelField (rect, "Folders", EditorStyles.boldLabel);
        }

        private void OnElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty property = serializedIcons.GetArrayElementAtIndex (index);

            float fullWidth = rect.width;

            // Set sizes for correct draw
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            float rectWidth = MAX_LABEL_WIDTH + MAX_FIELD_WIDTH;
            EditorGUIUtility.labelWidth = Mathf.Min (EditorGUIUtility.labelWidth, MAX_LABEL_WIDTH);
            rect.width = Mathf.Min (rect.width, rectWidth);

            //Draw property and settings in a line
            DrawPropertyNoDepth (rect, property);

            // ==========================
            //     Draw Icon Example
            // ==========================
            rect.x += rect.width;
            rect.width = fullWidth - rect.width;

            // References
            SerializedProperty folderTexture = property.FindPropertyRelative ("folderIcon");
            SerializedProperty overlayTexture = property.FindPropertyRelative ("overlayIcon");
            SerializedProperty overlayOffset = property.FindPropertyRelative ("overlayOffset");

            // Object checks
            Object folderObject = folderTexture.objectReferenceValue;
            Object overlayObject = overlayTexture.objectReferenceValue;

            FolderIconGUI.DrawFolderPreview (rect, folderObject as Texture, overlayObject as Texture);

            // Revert width modification
            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        private void DrawPropertyNoDepth(Rect rect, SerializedProperty property)
        {
            rect.width++;
            Handles.BeginGUI ();
            Handles.DrawSolidRectangleWithOutline (rect, Color.clear, new Color (0.15f, 0.15f, 0.15f, 1f));
            Handles.EndGUI ();

            rect.x++;
            rect.width -= 3;
            rect.y += PROPERTY_PADDING;
            rect.height = PROPERTY_HEIGHT;

            SerializedProperty copy = property.Copy ();
            bool enterChildren = true;

            while (copy.Next (enterChildren))
            {
                if (SerializedProperty.EqualContents (copy, property.GetEndProperty ()))
                {
                    break;
                }

                EditorGUI.PropertyField (rect, copy, false);
                rect.y += PROPERTY_HEIGHT + PROPERTY_PADDING;

                enterChildren = false;
            }
        }

        private void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.LabelField (rect, "", elementStyle);

            Color fill = (isFocused) ? FolderIconConstants.SelectedColor : Color.clear;

            Handles.BeginGUI ();
            Handles.DrawSolidRectangleWithOutline (rect, fill, new Color (0.15f, 0.15f, 0.15f, 1f));
            Handles.EndGUI ();
        }

        // ========================
        //
        // ========================
        private int GetPropertyCount(SerializedProperty property)
        {
            return property.CountInProperty () - 1;
        }

        private float GetPropertyHeight(SerializedProperty property)
        {
            if (heightIndex == 0)
            {
                heightIndex = property.CountInProperty ();
            }

            // return (PROPERTY_HEIGHT + PROPERTY_PADDING) * (heightIndex-1) + PROPERTY_PADDING;

            //Property count returning wrong, so just supplying 3 for now
            //TODO: Investigate GetPropertyCount and find issue with invalid value
            return (PROPERTY_HEIGHT + PROPERTY_PADDING) * (3) + PROPERTY_PADDING;
        }

        private float GetPropertyHeight(int index)
        {
            return GetPropertyHeight (serializedIcons.GetArrayElementAtIndex (index));
        }

        #endregion Reorderable Array Draw

        #region Texture Preview/Creation

        /// <summary>
        /// Draw the preview for the texture
        /// </summary>
        private void DrawTexturePreview()
        {
            EditorGUILayout.LabelField ("Texture Colour Replacement", EditorStyles.boldLabel);

            // Draw selection
            EditorGUI.BeginChangeCheck ();
            {
                // Headers
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField ("Original Texture");
                EditorGUILayout.LabelField ("Modified Texture");
                EditorGUILayout.EndHorizontal ();

                // Texture -- Preview
                EditorGUILayout.BeginHorizontal ();
                {
                    selectedTexture = EditorGUILayout.ObjectField (selectedTexture, typeof (Texture2D),
                        false, GUILayout.ExpandWidth (false), GUILayout.ExpandHeight (true), GUILayout.Width (64f)) as Texture2D;

                    EditorGUILayout.LabelField (previewContent, previewStyle, GUILayout.Height (64));
                }
                EditorGUILayout.EndHorizontal ();
            }
            if (EditorGUI.EndChangeCheck ())
            {
                if (selectedTexture == null)
                {
                    ClearPreviewData ();
                    previewTexture = null;
                    return;
                }

                UpdatePreview ();
            }

            EditorGUILayout.Space ();
        }

        /// <summary>
        /// Display settings for saving the modified texture
        /// </summary>
        private void DrawTextureSaving()
        {
            EditorGUILayout.LabelField ("Save Created Texture", EditorStyles.boldLabel);

            // Texture Name
            GUILayout.BeginHorizontal ();
            {
                textureName = EditorGUILayout.TextField ("Texture Name", textureName);

                EditorGUI.BeginDisabledGroup (true);
                GUILayout.TextField (".png", GUILayout.Width (40f));
                EditorGUI.EndDisabledGroup ();
            }
            GUILayout.EndHorizontal ();

            // Save Path
            GUILayout.BeginHorizontal ();
            {
                savePath = EditorGUILayout.TextField ("Save Path", savePath);

                if (GUILayout.Button ("Select", GUILayout.MaxWidth (80f)))
                {
                    savePath = EditorUtility.OpenFolderPanel ("Texture Save Path", "Assets", "");
                    GUIUtility.ExitGUI ();
                }
            }
            GUILayout.EndHorizontal ();

            if (GUILayout.Button ("Save Texture"))
            {
                string fullPath = $"{savePath}/{textureName}.png";
                SaveTextureAsPNG (previewTexture, fullPath);
            }
        }

        /// <summary>
        /// Apply colour to preview texture
        /// </summary>
        private void SetPreviewColour()
        {
            for (int x = 0; x < previewTexture.width; x++)
            {
                for (int y = 0; y < previewTexture.height; y++)
                {
                    Color oldCol = previewTexture.GetPixel (x, y);
                    Color newCol = replacementColour;
                    newCol.a = oldCol.a;

                    previewTexture.SetPixel (x, y, newCol);
                }
            }

            previewTexture.Apply ();
        }

        /// <summary>
        /// Save the preview texture at the given path
        /// </summary>
        /// <param name="texture">The preview texture</param>
        /// <param name="path">The path to save the texture at</param>
        private void SaveTextureAsPNG(Texture2D texture, string path)
        {
            if (string.IsNullOrWhiteSpace (path) || !path.Contains ("Assets"))
            {
                Debug.LogWarning ("Cannot save texture to invalid path.");
                return;
            }

            byte[] bytes = texture.EncodeToPNG ();
            File.WriteAllBytes (path, bytes);

            AssetDatabase.Refresh ();

            int localPathIndex = path.IndexOf ("Assets");
            path = path.Substring (localPathIndex, path.Length - localPathIndex);

            TextureImporter importer = AssetImporter.GetAtPath (path) as TextureImporter;

            importer.textureType = TextureImporterType.GUI;
            importer.isReadable = true;

            AssetDatabase.ImportAsset (path);
            AssetDatabase.Refresh ();

            //Texture2D textureAsset = AssetDatabase.LoadAssetAtPath<Texture2D> (path);
            //textureAsset.alphaIsTransparency = true;
            //textureAsset.Apply ();
        }

        /// <summary>
        /// Clear preview render texture
        /// </summary>
        private void ClearPreviewData()
        {
            if (previewRender != null)
            {
                previewRender.Release ();
            }
        }

        /// <summary>
        /// Update the preview with new texture information
        /// </summary>
        private void UpdatePreview()
        {
            ClearPreviewData ();

            //No real point having a huge texture so limit the size for efficency sake
            int width = Mathf.Min (256, selectedTexture.width);
            int height = Mathf.Min (256, selectedTexture.height);

            //Create a new render texture and preview
            previewRender = new RenderTexture (width, height, 16);
            previewTexture = new Texture2D (previewRender.width, previewRender.height)
            {
                alphaIsTransparency = true
            };
            previewContent.image = previewTexture;

            Graphics.Blit (selectedTexture, previewRender);

            // Get pixels from current render texture and apply
            previewTexture.ReadPixels (new Rect (0, 0, previewRender.width, previewRender.height), 0, 0);
            SetPreviewColour ();
        }

        #endregion Texture Preview/Creation
    }
}