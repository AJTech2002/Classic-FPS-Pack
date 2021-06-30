using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicFPS.Door_System
{
    //This is a KeyManager which is a scriptable object to hold all the different keys within the game 
    [ExecuteInEditMode]
    [CreateAssetMenu(fileName = "Key Manager", menuName = "Classic FPS/Key Manager")]
    public class KeyManager : ScriptableObject
    {
        public List<KeySettings> keys = new List<KeySettings>();
    }

    //The KeySettings holds the id of the key which identifies it
    [System.Serializable]
    public class KeySettings
    {
        public string keyID; //ID for the Key
        public bool consumable; //Whether or not the key dissapears after using it
        public bool allowMultiplePickups; //Whether or not you can pickup multiple keys of the same type
        public Sprite keySprite; //What the key looks like in UI
    }

    [System.Serializable]

    public class KeyReference
    {
        public string keyReference;
    }
}