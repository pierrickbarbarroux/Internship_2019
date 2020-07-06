using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class texture_script : MonoBehaviour
{
    Texture myTexture;
    public GameObject plane;
    UnityWebRequest firstRequest;
    /*
    void Start()
    {
        firstRequest = UnityWebRequestTexture.GetTexture("http://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&VERSION=1.0.0&REQUEST=GetTile&STYLE=normal&LAYER=ORTHOIMAGERY.ORTHOPHOTOS&EXCEPTIONS=text/xml&FORMAT=image/jpeg&TILEMATRIXSET=PM&TILEMATRIX=18&TILEROW=90241&TILECOL=132877&");
    }

    void Update()
    {
        if (firstRequest != null)
        {
            myTexture = ((DownloadHandlerTexture)firstRequest.downloadHandler).texture;
            plane.GetComponent<Renderer>().material.SetTexture("_myTexture", myTexture);
        }
            
    }
    */

    void Start()
    {
        StartCoroutine(GetTexture());
    }

    IEnumerator GetTexture()
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture("http://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&VERSION=1.0.0&REQUEST=GetTile&STYLE=normal&LAYER=ORTHOIMAGERY.ORTHOPHOTOS&EXCEPTIONS=text/xml&FORMAT=image/jpeg&TILEMATRIXSET=PM&TILEMATRIX=18&TILEROW=90241&TILECOL=132877&");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            plane.GetComponent<Renderer>().material.mainTexture=myTexture;
        }
    }


}
