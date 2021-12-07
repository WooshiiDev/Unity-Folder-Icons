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
        public class FolderData
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

        public FolderData[] icons = new FolderData[0];

        public Dictionary<string, FolderData> iconMap;

        public void OnInitalize()
        {
            iconMap = new Dictionary<string, FolderData> ();

            for (int i = 0; i < icons.Length; i++)
            {
                FolderData icon = icons[i];
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

            if (iconMap.TryGetValue(oldGUID, out FolderData icon))
            {
                iconMap.Add (newGUID, icon);
                iconMap.Remove (oldGUID);
            }
        }
    }
}