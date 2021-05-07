using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Json;

namespace ClassicFPS.Saving_and_Loading
{
public class SaveUtils
{
    //Turns Vector3 into Serializable format
    public static Vector3State Vector3ToXYZ (Vector3 value)
    {
        Vector3State state = new Vector3State { x = value.x, y = value.y, z = value.z };
        return state;
    }

    //Turns Serialized XYZ back into a Vector3
    public static Vector3 XYZToVector3(Vector3State value)
    {
        Vector3 vector = new Vector3(value.x, value.y, value.z);
        return vector;
    }

    //Get the JSON from an Object
    public static string ReturnJSONFromObject<T> (T state)
    {
        string json = JsonUtility.ToJson(state);
        return json;
    }

    //Get the Object from a JSON
    public static T ReturnStateFromJSON<T> (string json)
    {
        try
        {
            T newState = JsonUtility.FromJson<T>(json);
            return newState;
        }
        catch
        {
            Debug.LogError("Was not able to read in JSON File for : " + json);
            return default(T);
        }
    }

}

//Serialized Struct for storing any Vector3 values
[System.Serializable]
public struct Vector3State
{
    public float x;
    public float y;
    public float z;
}
}