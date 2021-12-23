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

        private GUIContentTexture openTexturePreview;
        private GUIContentTexture closedTexturePreview;

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

            closedTexturePreview = new GUIContentTexture (FolderIconConstants.TEX_FOLDER_CLOSED);
            openTexturePreview = new GUIContentTexture (FolderIconConstants.TEX_FOLDER_OPEN);
        }

        public override void OnInspectorGUI()
        {
            //Create styles
            if (elementStyle == null)
            {
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

        }

        private void ApplySettings()
        {
            settings.showCustomFolder = showCustomFolders;
            settings.showOverlay = showCustomOverlay;

            EditorApplication.RepaintProjectWindow ();
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

            EditorGUI.BeginChangeCheck ();

            DrawPropertyNoDepth (rect, property);

            if (EditorGUI.EndChangeCheck())
            {
                SerializedProperty folderProp = property.FindPropertyRelative ("folder");

                if (folderProp.objectReferenceValue != null)
                {
                    SerializedProperty guidProp = property.FindPropertyRelative ("guid");

                    string oldGUID = guidProp.stringValue;
                    string newGUID = AssetUtility.GetGUIDFromAsset (folderProp.objectReferenceValue);

                    if (oldGUID != newGUID)
                    {
                        if (!settings.iconMap.ContainsKey (newGUID))
                        {
                            guidProp.stringValue = newGUID;

                            if (!string.IsNullOrEmpty (oldGUID))
                            {
                                settings.UpdateGUIDMap (oldGUID, newGUID);
                            }
                            else
                            {
                                settings.iconMap.Add (newGUID, settings.icons[index]);
                            }
                        }
                        else
                        {
                            guidProp.stringValue = null;
                            settings.iconMap.Remove (oldGUID);
                        }

                        EditorApplication.RepaintProjectWindow ();
                    }
                }
            }


            // ==========================
            //     Draw Icon Example
            // ==========================
            rect.x += rect.width;
            rect.width = fullWidth - rect.width;

            // References
            SerializedProperty folderTexture = property.FindPropertyRelative ("folderIcon");
            SerializedProperty overlayTexture = property.FindPropertyRelative ("overlayIcon");

            // Object checks
            Object folderObject = folderTexture.objectReferenceValue;
            Object overlayObject = overlayTexture.objectReferenceValue;

            FolderGUI.DrawFolderPreview (rect, folderObject as Texture, overlayObject as Texture);

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

            while (copy.NextVisible (enterChildren))
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
            EditorGUI.BeginChangeCheck ();
            {
                EditorGUILayout.LabelField ("Texture Creator", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal ();
                {

                    EditorGUILayout.BeginVertical ();
                    {
                        EditorGUILayout.LabelField ("Colour", GUILayout.Width (64f));
                        replacementColour = EditorGUILayout.ColorField (new GUIContent (), replacementColour, true, false, false);

                        EditorGUILayout.Space ();

                        // Texture Name
                        GUILayout.Label ("Texture Name");
                        GUILayout.BeginHorizontal ();
                        {
                            textureName = EditorGUILayout.TextField ( textureName);

                            EditorGUI.BeginDisabledGroup (true);
                            GUILayout.TextField (".png", GUILayout.Width (40f));
                            EditorGUI.EndDisabledGroup ();
                        }
                        GUILayout.EndHorizontal ();

                        EditorGUILayout.Space ();

                        // Save Path
                        GUILayout.Label ("Save Path");
                        GUILayout.BeginHorizontal ();
                        {
                            savePath = EditorGUILayout.TextField (savePath);

                            if (GUILayout.Button ("Select", GUILayout.MaxWidth (60f)))
                            {
                                savePath = EditorUtility.OpenFolderPanel ("Save Path", "Assets", "");
                                GUIUtility.ExitGUI ();
                            }

                        }
                        GUILayout.EndHorizontal ();

                        GUILayout.Label (
                          string.Format ("Textures will be saved as\n - {0}_{1}\n - {0}_{2}", textureName, "Closed", "Open"),
                          EditorStyles.miniLabel);

                        GUILayout.FlexibleSpace ();
                    }
                    EditorGUILayout.EndVertical ();

                    EditorGUILayout.BeginVertical ();
                    {
                        closedTexturePreview.DrawGUIContent ("Closed");
                        openTexturePreview.DrawGUIContent ("Open");
                    }
                    EditorGUILayout.EndVertical ();
                }
                EditorGUILayout.EndHorizontal ();

                if (GUILayout.Button ("Save Textures", GUILayout.Height (32f)))
                {
                    SaveTextureAsPNG (closedTexturePreview.Texture, GetTextureSavePath("Closed"));
                    SaveTextureAsPNG (openTexturePreview.Texture, GetTextureSavePath ("Open"));
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                UpdatePreviews ();
            }

        }

        /// <summary>
        /// Display settings for saving the modified texture
        /// </summary>
        private void DrawTextureSaving()
        {
            EditorGUILayout.BeginVertical ();
            // Texture Name

            EditorGUILayout.LabelField ("Texture Name");
            {
                textureName = EditorGUILayout.TextField (textureName);

                EditorGUI.BeginDisabledGroup (true);
                GUILayout.TextField (".png", GUILayout.Width (40f));
                EditorGUI.EndDisabledGroup ();
            }

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

            EditorGUILayout.EndVertical ();


            if (GUILayout.Button ("Save Texture"))
            {
                string fullPath = $"{savePath}/{textureName}.png";
                //SaveTextureAsPNG (previewTexture, fullPath);
            }
        }

        /// <summary>
        /// Apply colour to preview texture
        /// </summary>
        private void UpdatePreviews()
        {
            closedTexturePreview.UpdateTexture (replacementColour);
            openTexturePreview.UpdateTexture (replacementColour);
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
        }

        private string GetTextureSavePath(string textureEnding)
        {
            return string.Format ("{0}/{1}_{2}.png", savePath, textureName, textureEnding);
        }

        #endregion Texture Preview/Creation

        private class GUIContentTexture
        {
            private string previousIcon;

            public GUIContent GUIContent { get; private set; }
            public Texture2D Texture { get; private set; }
            public RenderTexture Renderer { get; private set; }

            private Color[] oldColours;
            private Color[] newColours;

            private bool isDirty = false;

            public GUIContentTexture(string iconName)
            {
                UpdateGUIContent (iconName);
            }

            ~GUIContentTexture()
            {
                if (Renderer != null)
                {
                    Renderer.Release ();
                }
            }

            public void UpdateGUIContent(string iconName)
            {
                // We only need to update data if the icon has changed
                if (previousIcon != iconName)
                {
                    // Get icon
                    GUIContent = EditorGUIUtility.IconContent (iconName);

                    // Check if the renderer and texture need updated
                    Texture tex = GUIContent.image;
                    int width = Mathf.Min (256, tex.width);
                    int height = Mathf.Min (256, tex.height);

                    if (Texture == null || (Texture.width != width || Texture.height != height))
                    {
                        if (Renderer != null)
                        {
                            Renderer.Release ();
                        }

                        Renderer = new RenderTexture (width, height, 16);
                        Texture = new Texture2D (width, height)
                        {
                            alphaIsTransparency = true
                        };

                        newColours = new Color[width * height];
                    }

                    // Update state cache
                    previousIcon = iconName;
                    isDirty = true;
                }

                UpdateTexture (Color.white);
            }

            public void UpdateTexture(Color color)
            {
                int width = Texture.width;
                int height = Texture.height;

                if (isDirty)
                {
                    Graphics.Blit (GUIContent.image, Renderer);
                    Texture.ReadPixels (new Rect (0, 0, width, height), 0, 0);
                }

                oldColours = Texture.GetPixels ();

                int len = width * height;
                int i = 0;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        i = y * width + x;

                        newColours[i] = color;
                        newColours[i].a = oldColours[i].a;
                    }
                }

                Texture.SetPixels (newColours);
                Texture.Apply ();

                if (isDirty)
                {
                    GUIContent.image = Texture;
                    isDirty = false;
                }
            }

            public void DrawGUIContent(string label)
            {
                EditorGUILayout.BeginVertical (GUILayout.Width (64));

                EditorGUILayout.LabelField (label, EditorStyles.label, GUILayout.Width (64));
                EditorGUILayout.LabelField (GUIContent, EditorStyles.objectFieldThumb, GUILayout.Width (64), GUILayout.Height (64f));

                EditorGUILayout.EndVertical ();
            }
        }
    }
}