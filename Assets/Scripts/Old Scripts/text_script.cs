using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class text_script : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(GetText());
    }

    IEnumerator GetText()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=ELEVATION.ELEVATIONGRIDCOVERAGE.HIGHRES&TILEMATRIXSET=WGS84G&TILEMATRIX=14&TILEROW=3878&TILECOL=16681&STYLE=normal&FORMAT=image/x-bil;bits=32");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);

            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
            Debug.Log(results);
            Debug.Log(results[1]);
        }
    }
}
