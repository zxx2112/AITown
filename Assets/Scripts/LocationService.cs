using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Locations
{
    public const string Grocery = "Grocery Store";
    public const string Home = "Home";
}
public class LocationService
{
    private static Dictionary<string,GameObject> locationDict = new Dictionary<string, GameObject>();
    
    //Reload static fields
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reload()
    {
        locationDict.Clear();
    }

    public static void RegisterLocation(GameObject location)
    {
        locationDict.Add(location.name,location);
    }
    
    
    public static Vector3 GetLocationPosition(string locationName)
    {
        string id = null;

        switch (locationName)
        {
            case Locations.Grocery:
                id = "Grocery";
                break;
            
            case Locations.Home:
                id = "Home";
                break;
        }
        
        if(!locationDict.ContainsKey(id))
        {
            var location = GameObject.Find(id);
            if(location == null)
            {
                Debug.LogError("LocationService: 未找到位置:" + locationName);
                return Vector3.zero;
            }
            locationDict.Add(id,location);
        }

        return locationDict[id].transform.position;
    }
}
