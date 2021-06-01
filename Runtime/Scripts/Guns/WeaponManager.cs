using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ClassicFPS.Controller.Interaction;

/*
 * This is a base class only, all Weapon ScriptableObjects are to be created from this class
*/

namespace ClassicFPS.Guns
{
    [CreateAssetMenu(fileName = "Weapon Manager", menuName = "Classic FPS/Weapon Manager")]
    public class WeaponManager : ScriptableObject
    {
        [Header("Weapons")]
        public List<WeaponReference> weapons = new List<WeaponReference>();

        public Dictionary<string, WeaponReference> weaponDictionary = new Dictionary<string, WeaponReference>();

        public Dictionary<int, string> keybindIndex = new Dictionary<int, string>();

        [HideInInspector]
        public Transform spawnedWeaponInstance;

        [HideInInspector]
        public Weapon spawnedWeapon;

        //Find Weapon given an UID
        public WeaponReference GetWeaponReference(string UID)
        {
            if (weaponDictionary.ContainsKey(UID))
            {
                foreach (WeaponReference w in weapons)
                {
                    if (w.WeaponUniqueID == UID)
                    {
                        return w;
                    }
                }
            }
            else
            {
                return weaponDictionary[UID];
            }

            return null;
        }


        public void Equip(string UID)
        {
            if (spawnedWeapon == null)
            {
                PlayerWeaponController weaponController = GameObject.FindObjectOfType<PlayerWeaponController>();

                Transform weaponPrefab = GetWeaponReference(UID).WeaponPrefab;

                //Create the gun instance
                spawnedWeaponInstance = Transform.Instantiate(weaponPrefab, Vector3.zero, Quaternion.identity);

                //Find the Weapon component on the instance
                spawnedWeapon = spawnedWeaponInstance.GetComponent<Weapon>();



                if (spawnedWeapon == null)
                {
                    Debug.LogError("The weaponPrefab must have a class deriving from Weapon in its root object");
                }

                //Change the Crosshair
                weaponController.ChangeCrosshair(GetWeaponReference(UID).crosshair);

                //Run the Equip function to do all the setup
                spawnedWeapon.Equip(UID);

                GetWeaponReference(UID).isEquipped = true;
            }
        }

        public virtual void Unequip(string UID)
        {
            if (spawnedWeapon != null)
            {
                spawnedWeapon.Unequip();

                GetWeaponReference(UID).isEquipped = false;

                spawnedWeapon = null;
                spawnedWeaponInstance = null;
            }

        }


        public void Setup()
        {

            spawnedWeapon = null;
            spawnedWeaponInstance = null;

            weaponDictionary.Clear();
            keybindIndex.Clear();

            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponReference weapon = weapons[i];

                weapon.index = i;

                if (keybindIndex.ContainsKey(weapon.keyBindNumber) && weapon.keyBindNumber != -1)
                {
                    Debug.LogError("Make sure each weapon has a different Keybind Index");
                }

                if (weapon.keyBindNumber != -1)
                    keybindIndex.Add(weapon.keyBindNumber, weapon.WeaponUniqueID);

                weapon.Default.gunName = weapon.WeaponUniqueID;
                weapon.State = weapon.Default;
                //Setup Default State
                weaponDictionary.Add(weapon.WeaponUniqueID, weapon);
            }

            //Setup Weapons
            foreach (WeaponReference weapon in weapons)
            {

            }

            Debug.Log("Setup Dictionary");

        }


    }

    [System.Serializable]
    public class WeaponReference
    {

        [Header("Core Properties")]
        public string WeaponUniqueID;
        public Transform WeaponPrefab;

        [HideInInspector]
        public bool isEquipped = false;

        [Header("Default State")]
        public WeaponState Default;

        public WeaponState State;

        [Header("UI Properties")]
        public int keyBindNumber = -1;

        //Thumbnail image
        public Sprite thumbnail;
        [Space(10)]
        //What crosshair image to use
        public Sprite crosshair;

        //The size of the crosshair
        public Vector2 crosshairSize;

        //The increase in size of crosshair when the target is locked
        public float crosshairScaleOnTarget;

        public void SetState(WeaponState newState)
        {
            State = newState;
        }

        [HideInInspector]
        public int index;

    }

    //A State made for weapons to track what is important within a gun
    [System.Serializable]
    public struct WeaponState
    {
        public string gunName; //UID of Gun
        public int ammoRemaining; //Ammo left
    }

    public class WeaponIDAttribute : PropertyAttribute
    {
        public WeaponIDAttribute()
        {

        }
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(WeaponIDAttribute))]
    public class WeaponDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect

        int _selected = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            WeaponIDAttribute range = attribute as WeaponIDAttribute;

            // Now draw the property as a Slider or an IntSlider based on whether it's a float or integer.
            if (property.propertyType == SerializedPropertyType.String)
            {
                WeaponManager[] managers = Resources.FindObjectsOfTypeAll<WeaponManager>();
                if (managers.Length == 1)
                {
                    WeaponManager selected = managers[0];

                    List<string> _options = new List<string>();
                    _options.Add("");
                    foreach (WeaponReference weapRef in selected.weapons)
                    {
                        _options.Add(weapRef.WeaponUniqueID);
                    }

                    _selected = _options.IndexOf(property.stringValue);

                    GUI.contentColor = Color.white;
                    _selected = EditorGUI.Popup(position, "Weapon", _selected, _options.ToArray());

                    property.stringValue = _options[_selected];



                    property.serializedObject.ApplyModifiedProperties();
                }
                else if (managers.Length == 0)
                {
                    GUI.contentColor = Color.red;
                    EditorGUI.LabelField(position, "No WeaponManager Exists");
                }
                else
                {
                    GUI.contentColor = Color.red;
                    EditorGUI.LabelField(position, "Too many WeaponManagers Exist - Don't use this Attribute.");
                }

                GUI.contentColor = Color.white;
            }
            else
                EditorGUI.LabelField(position, label.text, "You can only use this attribute with Strings");
        }
    }
    #endif
}