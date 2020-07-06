using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Net;
using System;
using System.IO.Compression;
using Ionic.Zlib;
using System.Globalization;
using SimpleJSON;

/// <summary>
/// Classe gérant la réception, le décryptage, la manipulation des données récupérées via les requêtes WMS, WFS, ...
/// </summary>
public class DataController : MonoBehaviour
{
    //string getCapa = "https://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?request=GetCapabilities&service=WMTS";
    //string url_relief = "http://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=ELEVATION.ELEVATIONGRIDCOVERAGE.HIGHRES&TILEMATRIXSET=WGS84G&TILEMATRIX=14&TILEROW=3878&TILECOL=16681&STYLE=normal&FORMAT=image/x-bil;bits=32";
    //string url_wfs = "https://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wfs?SERVICE=WFS&REQUEST=GetFeature&VERSION=2.0.0&CRS=EPSG:2154&LAYER=BDTOPO_BDD_WLD_WGS84G:route&TYPENAME=&OUTPUTFORMAT=GeoJSON&MAXFEATURE=&FILTER=&";
    //string url_wfs = "http://wxs-i.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wfs?SERVICE=WFS&VERSION=1.1.0&REQUEST=GetFeature&TYPENAME=BDTOPO_BDD_WLD_WGS84G:route&SRSNAME=EPSG:2154&BBOX=650769.6754608535,6862000.895466741,651281.6754608535,6862512.895466741,EPSG:2154&STARTINDEX=0&MAXFEATURES=1000";
    //http://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&VERSION=1.0.0&REQUEST=GetTile&STYLE=normal&LAYER=ORTHOIMAGERY.ORTHOPHOTOS&EXCEPTIONS=text/xml&FORMAT=image/jpeg&TILEMATRIXSET=PM&TILEMATRIX=18&TILEROW=90241&TILECOL=132877&

    //Rougiers
    //      6258474
    //      930587.3

    //Toulon
    //      952254.94
    //      6219373.22

    public grid_script mygrid;

    float size = 1f;
    string key = "j0rmqdk335hqz204nk0awcpn";
    string layer;

    //Float nécessaire pour la barre de chargement dans le menu principale
    [HideInInspector]
    public static float loading;

    void Start()
    {
        loading = 0;
    }

    /// <summary>
    /// Decompresse un array byte[] en deflate.
    /// Cette fonction n'est pas de moi.
    /// </summary>
    /// <param name="data">byte[] décompressé</param>
    /// <returns></returns>
    public static byte[] Inflate(byte[] data)
    {
        int outputSize = 1024;
        byte[] output = new Byte[outputSize];
        bool expectRfc1950Header = true;
        using (MemoryStream ms = new MemoryStream())
        {
            ZlibCodec compressor = new ZlibCodec();
            compressor.InitializeInflate(expectRfc1950Header);

            compressor.InputBuffer = data;
            compressor.AvailableBytesIn = data.Length;
            compressor.NextIn = 0;
            compressor.OutputBuffer = output;

            foreach (var f in new FlushType[] { FlushType.None, FlushType.Finish })
            {
                int bytesToWrite = 0;
                do
                {
                    compressor.AvailableBytesOut = outputSize;
                    compressor.NextOut = 0;
                    compressor.Inflate(f);

                    bytesToWrite = outputSize - compressor.AvailableBytesOut;
                    if (bytesToWrite > 0)
                        ms.Write(output, 0, bytesToWrite);
                }
                while ((f == FlushType.None && (compressor.AvailableBytesIn != 0 || compressor.AvailableBytesOut == 0)) ||
                    (f == FlushType.Finish && bytesToWrite != 0));
            }

            compressor.EndInflate();

            return ms.ToArray();
        }
    }

    /// <summary>
    /// Prend la requête ous forme d'url.
    /// Renvoie un array de bytes (byte[]) contenant l'ensemble des données de la requête
    /// </summary>
    /// <param name="myurl">url de la requête</param>
    /// <returns>byte[] contenant l'ensemble des données de la requête</returns>
    public static IEnumerator GetData(string myurl)
    {
        UnityWebRequest www = UnityWebRequest.Get(myurl);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            //retrieve results as binary data
            byte[] results_compressed = www.downloadHandler.data;
            Debug.Log(results_compressed.Length);
            byte[] results_bytes = Inflate(results_compressed);
            float[] results_float = new float[256 * 256];

            for (int i = 0; i < 65536; i++)
            {
                results_float[i] = System.BitConverter.ToSingle(results_bytes, i * 4);
            }

            Debug.Log(results_float[0]);

            yield return results_float;
        }
    }

    /// <summary>
    /// Prend la requête ous forme d'url.
    /// Renvoie un array de bytes (byte[]) contenant l'ensemble des données de la requête
    /// Même version que GetData sauf qu'on décompresse pas : il n'y a pas beosin pour les wms
    /// </summary>
    /// <param name="myurl">url de la requête</param>
    /// <param name="height">longueur de la box</param>
    /// <param name="width">largeur de la box</param>
    /// <returns>byte[] contenant l'ensemble des données de la requête</returns>
    public static IEnumerator GetWmsData(string myurl, int height, int width)
    {
        UnityWebRequest www = UnityWebRequest.Get(myurl);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            //retrieve results as binary data
            byte[] results_bytes = www.downloadHandler.data;
            float[] results_float = new float[height * width];

            for (int i = 0; i < (height * width); i++)
            {
                results_float[i] = System.BitConverter.ToSingle(results_bytes, i * 4);
            }

            yield return results_float;
        }
    }

    /// <summary>
    /// Récupère les données d'une requête et les écrit dans un fichier.
    /// Une des méthodes les plus utilisées de cette classe.
    /// </summary>
    /// <param name="myurl">url de la requête</param>
    /// <param name="path">path du fichier</param>
    /// <returns></returns>
    public static IEnumerator WriteDataFile(string myurl, string path)
    {
        var www = new UnityWebRequest(myurl, UnityWebRequest.kHttpVerbGET);
        www.downloadHandler = new DownloadHandlerFile(path);

        //UnityWebRequest www = UnityWebRequest.Get(myurl);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            www.downloadHandler = new DownloadHandlerFile(path);

            yield return www.SendWebRequest();
        }
        else
        {
            Debug.Log("Requête réussie");
            loading++;
        }
    }

    /// <summary>
    /// Lit un fichier contenant le MNT d'une tuile et renvoie un tableau contenant les points d'altitudes
    /// </summary>
    /// <param name="path">path du fichier</param>
    /// <param name="array_length">taille du tableau contenant les données</param>
    /// <returns>float[] contenant l'altitude de la tuile</returns>
    public static float[] GetMNTFromFile(string path, int array_length)
    {
        BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open));
        float[] results = new float[array_length];
        float altitude;

        for (int i = 0; i < array_length; i++)
        {
            altitude = System.BitConverter.ToSingle(reader.ReadBytes(4), 0);
            if (altitude < 0)
            {
                results[i] = 0;
            }
            else
            {
                results[i] = altitude;
            }
        }
        reader.Close();
        return results;
    }

    /// <summary>
    /// Récupère les données d'une requête WFS
    /// </summary>
    /// <param name="myurl">url de la requête</param>
    /// <returns>le résultat de la requête</returns>
    public static IEnumerator GetWfsData(string myurl)
    {
        UnityWebRequest www = UnityWebRequest.Get(myurl);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            //Debug.Log(www.error);
        }
        else
        {
            //retrieve results as binary data
            string results = www.downloadHandler.text;
            ////Debug.Log(results);

            yield return results;
        }
    }

    /// <summary>
    /// Prend un array de taille divisible par 4 et de taille un carré.
    /// Renvoie un array de taille divisé par 4 contenant la "moyenne" du précédent array
    /// </summary>
    /// <param name="array">array à réduire</param>
    /// <returns>array de taille réduite</returns>
    public static float[] Reduce(float[] array)
    {
        int size = (int)Mathf.Sqrt(array.Length); //= 256
        int new_size = array.Length / 4; //=128*128
        float[] result = new float[new_size];

        int z = 0;
        int x = 0;

        for (int i = 0; i < new_size; i++)
        {
            result[i] = (array[x + z * size] + array[x + 1 + z * size] + array[x + (z + 1) * size] + array[x + 1 + (z + 1) * size]) / 4;
            if (x + 1 == size - 1)
            {
                x = 0;
                z += 2;
            }
            else
            {
                x += 2;
            }
        }

        return result;
    }

    /// <summary>
    /// Réduit un tableau d'array en enlevant certains éléments
    /// </summary>
    /// <param name="array">array à réduire</param>
    /// <returns>array de taille réduite avec des éléments en moins</returns>
    public static float[] RemoveSome(float[] array)
    {
        int size = (int)Mathf.Sqrt(array.Length); //= 256
        int new_size = array.Length / 4; //=128*128
        float[] result = new float[new_size];

        int z = 0;
        int x = 0;

        for (int i = 0; i < new_size; i++)
        {
            result[i] = array[x + z * size];
            if (x + 1 == size - 1)
            {
                x = 0;
                z += 2;
            }
            else
            {
                x += 2;
            }
        }

        return result;
    }

    public static string GetRequest(string layer, string tile_m, string tile_r, string tile_c)
    {
        string url = "http://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=" + layer + "&TILEMATRIXSET=WGS84G&TILEMATRIX=" + tile_m + "&TILEROW=" + tile_r + "&TILECOL=" + tile_c + "&STYLE=normal&FORMAT=image/x-bil;bits=32";
        return url;
    }

    /// <summary>
    /// Retourne une requête WMS 'bien écrite'
    /// </summary>
    /// <param name="layer">la couche</param>
    /// <param name="format">le format</param>
    /// <param name="height">la longueur de la box</param>
    /// <param name="width">la largeur de la box</param>
    /// <param name="left_down">coordonnées du coin inférieur gauche (lambert93)</param>
    /// <param name="right_up">coordonnées du coin supérieur droit (lambert 93)</param>
    /// <returns></returns>
    public static string GetWmsRequest(string layer, string format, string height, string width, (float, float) left_down, (float, float) right_up)
    {
        string specifier;
        // Use standard numeric format specifiers.
        specifier = "G";

        string bbox = left_down.Item1.ToString(specifier, CultureInfo.InvariantCulture) + "," + left_down.Item2.ToString(specifier, CultureInfo.InvariantCulture) + "," + right_up.Item1.ToString(specifier, CultureInfo.InvariantCulture) + "," + right_up.Item2.ToString(specifier, CultureInfo.InvariantCulture);
        string url = "http://wxs.ign.fr/bmv1bzgge2e6zl1i3bj4qr2q/geoportail/r/wms?LAYERS=" + layer + "&EXCEPTIONS=text/xml&FORMAT=" + format + "&SERVICE=WMS&VERSION=1.3.0&REQUEST=GetMap&STYLES=normal&CRS=EPSG:2154&BBOX=" + bbox + "&WIDTH=" + width + "&HEIGHT=" + height;
        ////Debug.Log(url);
        return url;
    }

    /// <summary>
    /// Retourne une requête WFS 'bien écrite'
    /// </summary>
    /// <param name="typename">nom de la couche wfs</param>
    /// <param name="format">format du fichier en sortie</param>
    /// <param name="left_down_lon">easting du coin inférieur gauche</param>
    /// <param name="left_down_lat">northing du coin inférieur gauche</param>
    /// <param name="right_up_lon">easting du coin supérieur droit</param>
    /// <param name="right_up_lat">northing du coin supérieur droit</param>
    /// <returns></returns>
    public static string GetWfsRequest(string typename, string format, float left_down_lon, float left_down_lat, float right_up_lon, float right_up_lat)
    {
        string specifier = "G";
        string url;
        //string typename = BDTOPO_BDD_WLD_WGS84G:commune;
        string bbox = left_down_lon.ToString(specifier, CultureInfo.InvariantCulture) + "," + left_down_lat.ToString(specifier, CultureInfo.InvariantCulture) + "," + right_up_lon.ToString(specifier, CultureInfo.InvariantCulture) + "," + right_up_lat.ToString(specifier, CultureInfo.InvariantCulture);
        url = "http://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wfs?SERVICE=WFS&VERSION=1.1.0&REQUEST=GetFeature&TYPENAME=" + typename + "&SRSNAME=EPSG:2154&BBOX=" + bbox + ",EPSG:2154&STARTINDEX=0&MAXFEATURES=1000&OUTPUTFORMAT=" + format;
        return url;
    }


    public static string GetRequestSkin(string layer, string tile_m, string tile_r, string tile_c)
    {
        string url = "http://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=" + layer + "&EXCEPTIONS=text/xml&TILEMATRIXSET=PM&TILEMATRIX=" + tile_m + "&TILEROW=" + tile_r + "&TILECOL=" + tile_c + "&STYLE=normal&FORMAT=image/jpeg";
        return url;
    }

    /// <summary>
    /// Fonction de test. N'est plus util à la fin du projet.
    /// </summary>
    /// <param name="myjson"></param>
    public static void JsonToArrayRoad(string myjson)
    {
        var bigjson = JSON.Parse(myjson);
        string price = bigjson["features"][0]["geometry"]["coordinates"][0][0][1].Value;
    }

    IEnumerator ApplyTile(float level, float tile_row, float tile_col, GameObject gameObj)
    {
        //Debug.Log("applytile");
        key = "j0rmqdk335hqz204nk0awcpn";
        string service = "WMTS";
        string version = "1.0.0";
        string request = "GetTile";
        string style = "normal";
        layer = "ORTHOIMAGERY.ORTHOPHOTOS";
        string exceptions = "text/xml";
        string format = "image/jpeg";
        string tileMatrixSet = "PM";
        string tileMatrix = level.ToString();
        string tileRow = tile_row.ToString();
        string tileCol = tile_col.ToString();
        string url = "http://wxs.ign.fr/" + key + "/geoportail/wmts?SERVICE=" + service + "&VERSION=" + version + "&REQUEST=" + request + "&STYLE=" + style + "&LAYER=" + layer + "&EXCEPTIONS=" + exceptions + "&FORMAT=" + format + "&TILEMATRIXSET=" + tileMatrixSet + "&TILEMATRIX=" + tileMatrix + "&TILEROW=" + tileRow + "&TILECOL=" + tileCol + "&";

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            gameObj.GetComponent<Renderer>().material.mainTexture = myTexture;
        }
    }

    /// <summary>
    /// Fonction de test. N'est pas utilisée.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="x0"></param>
    /// <param name="z0"></param>
    /// <returns></returns>
    IEnumerator DrawTiles(float level, float x0, float z0)
    {
        Vector3 new_scale = new Vector3(0.1f, 0.1f, 0.1f);
        Vector3 new_rotate = new Vector3(0f, 180f, 0f);

        for (float z = 0; z < 10; z += size)
        {
            for (float x = 0; x < 10; x += size)
            {
                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.transform.localScale = new_scale;
                plane.transform.position = mygrid.GetNearestPointOnGrid(new Vector3(x, 0f, 10 - z));
                plane.transform.Rotate(new_rotate);
                plane.GetComponent<Renderer>().material.color = UnityEngine.Color.white;
                StartCoroutine(ApplyTile(level, z + z0, x + x0, plane));

                yield return null;

            }
        }
    }

    /// <summary>
    /// Retourne la texture correspondant à la requête url (pour l'ortho)
    /// </summary>
    /// <param name="url">requête url</param>
    /// <returns>la texture de l'ortho</returns>
    public static IEnumerator GetTexture(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            yield return myTexture;
        }
    }

    /// <summary>
    /// Méthode de test.
    /// Ne sert pas dans me projet finale.
    /// </summary>
    /// <returns></returns>
    IEnumerator CrossedRequest()
    {
        //Debug.Log("start");
        string request_high;
        string request_skin;
        string layer_high = "ELEVATION.ELEVATIONGRIDCOVERAGE.HIGHRES";
        string layer_skin = "ORTHOIMAGERY.ORTHOPHOTOS";
        for (int i = 5472; i < 9263; i++)
        {
            for (int j = 10653; j < 15839; j++)
            {
                request_high = GetRequest(layer_high, "14", i.ToString(), j.ToString());
                request_skin = GetRequest(layer_skin, "14", i.ToString(), j.ToString());

                UnityWebRequest www1 = UnityWebRequest.Get(request_high);
                yield return www1.SendWebRequest();

                UnityWebRequest www2 = UnityWebRequestTexture.GetTexture(request_skin);
                yield return www2.SendWebRequest();

                if (!www1.isNetworkError && !www1.isHttpError && !www2.isNetworkError && !www2.isHttpError)
                {
                    //Debug.Log("Prendre comme Row : " + i + "-----Prendre comme Col : " + j);
                }
            }
        }
    }

    //https://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=ELEVATION.ELEVATIONGRIDCOVERAGE.HIGHRES&TILEMATRIXSET=WGS84G&TILEMATRIX=14&TILEROW=3878&TILECOL=16681&STYLE=normal&FORMAT=image/x-bil;bits=32

    //http://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&VERSION=1.0.0&REQUEST=GetTile&STYLE=normal&LAYER=ORTHOIMAGERY.ORTHOPHOTOS&EXCEPTIONS=text/xml&FORMAT=image/jpeg&TILEMATRIXSET=PM&TILEMATRIX=6&TILEROW=22&TILECOL=32&%22

    //https://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=ELEVATION.SLOPES&TILEMATRIXSET=PM&TILEMATRIX=6&TILEROW=22&TILECOL=32&STYLE=normal&FORMAT=image/jpeg
    //https://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=ELEVATION.ELEVATIONGRIDCOVERAGE.HIGHRES&TILEMATRIXSET=WGS84G&TILEMATRIX=6&TILEROW=22&TILECOL=32&STYLE=normal&FORMAT=image/x-bil;bits=32
    //myURL = "http://wxs.ign.fr/" + key + "/geoportail/wmts?SERVICE=" + service + "&VERSION=" + version + "&REQUEST=" + request + "&STYLE=" + style + "&LAYER=" + layer + "&EXCEPTIONS=" + exceptions + "&FORMAT=" + format + "&TILEMATRIXSET=" + tileMatrixSet + "&TILEMATRIX=" + tileMatrix + "&TILEROW=" + tileRow + "&TILECOL=" + tileCol + "&";
    //myURL = "http://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&VERSION=1.0.0&REQUEST=GetTile&STYLE=normal&LAYER=ORTHOIMAGERY.ORTHOPHOTOS&EXCEPTIONS=text/xml&FORMAT=image/jpeg&TILEMATRIXSET=PM&TILEMATRIX=3&TILEROW=4&TILECOL=4&";
    //myURL = "http://wxs.ign.fr/"+ key +"/geoportail/wmts?SERVICE = WMTS & VERSION = 1.0.0 & REQUEST = GetTile & STYLE = normal & LAYER = ORTHOIMAGERY.ORTHOPHOTOS & EXCEPTIONS = text / xml & FORMAT = image / jpeg & TILEMATRIXSET = PM & TILEMATRIX = 18 & TILEROW = 90241 & TILECOL = 132877 &";
    //myURL = "http://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=ELEVATION.ELEVATIONGRIDCOVERAGE.HIGHRES&TILEMATRIXSET=WGS84G&TILEMATRIX=14&TILEROW=3878&TILECOL=16681&STYLE=normal&FORMAT=image/x-bil;bits=32";
    //myURL = "https://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=ELEVATION.SLOPES&TILEMATRIXSET=PM&TILEMATRIX=6&TILEROW=22&TILECOL=32&STYLE=normal&FORMAT=image/jpeg";
    //myURL = "https://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=ELEVATION.SLOPES&TILEMATRIXSET=PM&TILEMATRIX=6&TILEROW=22&TILECOL=32&STYLE=normal&FORMAT=image/jpeg";
    //myURL = "https://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=ELEVATION.SLOPES&TILEMATRIXSET=PM&TILEMATRIX=6&TILEROW=22&TILECOL=32&STYLE=normal&FORMAT=image/jpeg";


    /// <summary>
    /// Il s'agit d'une fonction de test pour la fonction CoroutineResult.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Test()
    {
        string myjson;
        CoroutineResult cd = new CoroutineResult(this, GetWfsData(GetWfsRequest("BDTOPO_BDD_WLD_WGS84G:route", "json", 930587.3f, 6258474f, 931099.3f, 6258986f)));
        yield return cd.coroutine;
        myjson = cd.result.ToString();
        JsonToArrayRoad(myjson);
    }
}

