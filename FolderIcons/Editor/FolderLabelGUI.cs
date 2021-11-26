using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FolderIcons
{
    public static class FolderLabelGUI
    {
        public static void DrawLabel(Rect rect, string guid, FolderIconSettings.FolderIcon iconSettings)
        {
            float zoom = rect.width / 96;
            GUIStyle labelStyle = new GUIStyle ();

            string shortHand = iconSettings.folder.name;

            if (!IsSideFolder (rect, guid, out _) && !FolderIconGUI.IsTreeView (rect))
            {
                rect.width = Mathf.Clamp (rect.width, 35, 96) + 7;
                rect.x -= 5;
                rect.y += rect.height - 13;
                rect.height = 14;

                labelStyle = new GUIStyle
                {
                    normal = new GUIStyleState { textColor = iconSettings.labelColor },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10,
                };

                CheckOverflow (ref rect, ref shortHand);

                EditorGUI.DrawRect (rect, FolderIconConstants.BackgroundColour);
                if (Selection.assetGUIDs.Contains (guid))
                {
                    DrawLabelSelection (rect, iconSettings);
                }

                rect.x += 1.5f;
                GUI.Label (rect, shortHand, labelStyle);
                return;
            }
            else if (IsSideFolder (rect, guid, out float indent))
            {
                rect.width += 100;
                rect.x += 17;
                rect.height = 16;

                labelStyle = new GUIStyle
                {
                    normal = new GUIStyleState { textColor = iconSettings.labelColor },
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 12,
                };

                CheckOverflow (ref rect, ref shortHand);

                rect.x -= indent;
                EditorGUI.DrawRect (rect, FolderIconConstants.TreeViewBackgroundColour);
                rect.x += indent;
                if (Selection.assetGUIDs.Contains (guid))
                {
                    rect.x -= indent + 18;
                    DrawLabelSelection (rect, iconSettings);
                    rect.x += indent + 18;
                }

                rect.x += 1f;
                GUI.Label (rect, shortHand, labelStyle);
                return;
            }
            else if (FolderIconGUI.IsTreeView (rect))
            {
                rect.width += 100;
                rect.x -= 13;
                rect.height = 16;
                rect.y -= rect.height - 16;

                labelStyle = new GUIStyle
                {
                    normal = new GUIStyleState { textColor = iconSettings.labelColor },
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 12,
                };

                CheckOverflow (ref rect, ref shortHand);

                EditorGUI.DrawRect (rect, FolderIconConstants.BackgroundColour);
                if (Selection.assetGUIDs.Contains (guid))
                {
                    DrawLabelSelection (rect, iconSettings);
                }

                rect.x += 34.5f;
                GUI.Label (rect, shortHand, labelStyle);
                return;
            }
        }

        private static void CheckOverflow(ref Rect rect, ref string shortHand)
        {
            int minLength = (int)rect.width / 5 - 5;
            if (rect.width % 5 > 0)
            {
                minLength++;
            }

            minLength = Mathf.Clamp (minLength, 1, int.MaxValue);

            if (shortHand.Length > minLength)
            {
                shortHand = shortHand.Substring (0, minLength) + "...";
            }
        }

        private static void DrawLabelSelection(Rect rect, FolderIconSettings.FolderIcon iconSettings)
        {
            Gradient usedGradient = iconSettings.selectionGradient;
            if (FolderIconsReplacer.GetFolderIconSettings ().useGlobalSelectionColor)
            {
                usedGradient = FolderIconsReplacer.GetFolderIconSettings ().globalSelectionGradient;
            }

            Texture2D drawTexture = new Texture2D (64, 1);
            for (int x = 0; x < drawTexture.width; x++)
            {
                float time = x / (float)drawTexture.width;
                Color tintColor = usedGradient.Evaluate (time);
                // Debug.Log("tint pixel:" + tintColor);
                drawTexture.SetPixel (x, 1, tintColor);
            }
            drawTexture.Apply ();

            GUI.DrawTexture (rect, drawTexture, ScaleMode.StretchToFill, true, 0, Color.white, 0, 15);
        }

        public static Texture2D GradientToTexture(Gradient grad, int width = 32)
        {
            Texture2D gradTex = new Texture2D (width, 1);
            gradTex.filterMode = FilterMode.Bilinear;
            float inv = 1f / (width);
            for (int x = 0; x < width; x++)
            {
                System.Single t = x * inv;
                Color col = grad.Evaluate (t);
                gradTex.SetPixel (x, 1, col);
            }
            gradTex.Apply ();
            return gradTex;
        }

        public static bool IsSideFolder(Rect rect, string guid, out float indent)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath (guid);
            string[] splits = assetPath.TrimStart (Application.dataPath.ToCharArray ()).Split ('/').Skip (1).ToArray ();
            indent = (14 * splits.Length) + 30;
            return rect.x == indent;
        }
    }
}