using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class data_script : MonoBehaviour
{
    
    void Start()
    {
        StartCoroutine(GetAssetBundle());
    }
    
    /*
    string url = "http://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&VERSION=1.0.0&REQUEST=GetTile&STYLE=normal&LAYER=ORTHOIMAGERY.ORTHOPHOTOS&EXCEPTIONS=text/xml&FORMAT=image/jpeg&TILEMATRIXSET=PM&TILEMATRIX=18&TILEROW=90241&TILECOL=132877&";
    IEnumerator Start()
    {
        while (!Caching.ready)
            yield return null;

        // Start a download of the given URL
        WWW www = WWW.LoadFromCacheOrDownload(url, 1);

        // Wait for download to complete
        yield return www;

        // Load and retrieve the AssetBundle
        AssetBundle bundle = www.assetBundle;

        // Load the TextAsset object
        TextAsset txt = bundle.LoadAsset("myBinaryAsText", typeof(TextAsset)) as TextAsset;

        // Retrieve the binary data as an array of bytes
        byte[] bytes = txt.bytes;
        Debug.Log(bytes);
    }*/

    IEnumerator GetAssetBundle()
    {
        string url = "http://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&VERSION=1.0.0&REQUEST=GetFeatureInfo&STYLE=normal&LAYER=ORTHOIMAGERY.ORTHOPHOTOS&EXCEPTIONS=text/xml&FORMAT=image/jpeg&TILEMATRIXSET=PM&TILEMATRIX=18&TILEROW=90241&TILECOL=132877&INFOFORMAT=text/html&I=1&J=1&";
        UnityWebRequest myRequest = UnityWebRequestAssetBundle.GetAssetBundle(url);
        yield return myRequest.SendWebRequest();

        if (myRequest.isNetworkError || myRequest.isHttpError)
        {
            Debug.Log(myRequest.error);
        }
        else
        {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(myRequest);
            Debug.Log(bundle);
        }
    }

}
