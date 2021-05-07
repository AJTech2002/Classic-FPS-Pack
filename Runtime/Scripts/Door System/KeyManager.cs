using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicFPS.Door_System
{
    [ExecuteInEditMode]
    [CreateAssetMenu(fileName = "Key Manager", menuName = "Classic FPS/Key Manager")]
    public class KeyManager : ScriptableObject
    {
        public List<KeySettings> keys = new List<KeySettings>();
    }

    [System.Serializable]
    public class KeySettings
    {
        public string keyID;
        public bool consumable;
        public bool allowMultiplePickups;
        public Sprite keySprite;
    }

    [System.Serializable]

    public class KeyReference
    {
        public string keyReference;
    }
}