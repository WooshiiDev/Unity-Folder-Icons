using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FolderIcons
{
    [CreateAssetMenu (fileName = "Folder Icon Manager", menuName = "Scriptables/Others/Folder Manager")]
    public class FolderIconSettings : ScriptableObject
    {
        [Serializable]
        public class FolderIcon
        {
            [SerializeField][HideInInspector]
            private string guid;
            public string GUID => guid;

            public DefaultAsset folder;

            public Texture2D folderIcon;
            public Texture2D overlayIcon;

            public void SetGUID(string guid)
            {
                this.guid = guid;
            }
        }

        //Global Settings
        public bool showOverlay = true;
        public bool showCustomFolder = true;

        public FolderIcon[] icons = new FolderIcon[0];

        public Dictionary<string, FolderIcon> iconMap;

        public void OnInitalize()
        {
            iconMap = new Dictionary<string, FolderIcon> ();

            for (int i = 0; i < icons.Length; i++)
            {
                FolderIcon icon = icons[i];
                string guid = icon.GUID;

                if (string.IsNullOrEmpty(guid))
                {
                    icon.SetGUID(AssetUtility.GetGUIDFromAsset(icon.folder));
                }

                if (iconMap.ContainsKey(guid))
                {
                    continue;
                }

                iconMap.Add (guid, icons[i]);
            }
        }

        public void UpdateGUIDMap(string oldGUID, string newGUID)
        {
            if (oldGUID == newGUID)
            {
                return;
            }

            if (iconMap.ContainsKey(newGUID))
            {
                return;
            }

            if (iconMap.TryGetValue(oldGUID, out FolderIcon icon))
            {
                iconMap.Add (newGUID, icon);
                iconMap.Remove (oldGUID);
            }
        }
    }
}