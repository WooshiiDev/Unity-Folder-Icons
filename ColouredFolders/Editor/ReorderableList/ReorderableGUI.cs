using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace FolderIcons
    {
    internal class ReorderableGUI
        {
        // Styling
        private GUIStyle elementStyle;
        private ReorderableList list;

        public ReorderableGUI(IList list, Type type)
            {
            this.list = new ReorderableList (list, type, true, true, true, false);
            }

        public void DrawList()
            {
            list.DoLayoutList ();
            }

        public void DrawList(Rect rect)
            {
            list.DoList (rect);
            }
        }
    }
