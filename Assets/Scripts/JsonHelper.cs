using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cette classe est censée améliorer le sytème de lecture JSON. Mais aucune fonction n'est référencée. 
/// Je devais en avoir besoin à un moment, mais ce n'est plus le cas maintenant (j'ai dû trouver un autre moyen).
/// </summary>
public class JsonHelper : MonoBehaviour
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}
