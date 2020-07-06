using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Net;
using System;
using System.IO.Compression;
using Ionic.Zlib;

public class FirstScript : MonoBehaviour
{  

    UnityWebRequest firstRequest;

    Vector3 z_plus = new Vector3(0, 0, 1f);
    Vector3 z_moins = new Vector3(0, 0, -1f);

    Ray ray;

    public GameObject myplane;
    public grid_script mygrid;
    public int xSize;
    public int zSize;
    
    Texture myTexture;

    float size = 1f;

    string key = "j0rmqdk335hqz204nk0awcpn";
    string service;
    string version;
    string request;
    string style;
    string layer;
    string exceptions;
    string format;
    string tileMatrixSet;
    string tileMatrix;
    string tileRow;
    string tileCol;
    //string myURL;


    float X0;
    float Y0;
    float X;
    float Y;
    float zoom;

    //string getCapa = "https://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?request=GetCapabilities&service=WMTS";
    //string url_relief = "https://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=ELEVATION.ELEVATIONGRIDCOVERAGE.HIGHRES&TILEMATRIXSET=WGS84G&TILEMATRIX=14&TILEROW=3878&TILECOL=16681&STYLE=normal&FORMAT=image/x-bil;bits=32";

    // Start is called before the first frame update
    void Start()
    {

        /*
        service = "WMTS";
        version = "1.0.0";
        request = "GetTile";
        style = "normal";
        layer = "ORTHOIMAGERY.ORTHOPHOTOS";
        exceptions = "text/xml";
        format = "image/jpeg"; 
        tileMatrixSet = "PM";
        tileMatrix = "18";
        tileRow = "90241";
        tileCol = "132877";*/
        /*
        service = "WMTS";
        version = "1.0.0";
        request = "GetTile";
        style = "normal";
        layer = "GRIDCOVERAGE.HIGHRES";
        exceptions = "text/xml";
        format = "image/jpeg";
        tileMatrixSet = "PM";
        tileMatrix = "3";

        tileRow = "4";
        tileCol = "4";
        */

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
        //Debug.Log(0 % 16);

        //float[] test = { 1,1,1,1,1,1,2,2,2,2,2,2,3,3,3,3,3,3,4,4,4,4,4,4,5,5,5,5,5,5,6,6,6,6,6,6};
        //Reduce(test);
        //RemoveSome(test);

        //StartCoroutine(GetData(myURL));

    }

    private void Update()
    {
    }

    //http://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&VERSION=1.0.0&REQUEST=GetTile&STYLE=normal&LAYER=ORTHOIMAGERY.ORTHOPHOTOS&EXCEPTIONS=text/xml&FORMAT=image/jpeg&TILEMATRIXSET=PM&TILEMATRIX=18&TILEROW=90241&TILECOL=132877&

    //Decompresse un array byte[] en deflate
    //Retourne donc un byte[]
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

    /* Prend la requête ous forme d'url
     * Renvoie un array de bytes (byte[]) contenant l'ensemble des données de la requête
     */
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
            byte[] results_bytes = Inflate(results_compressed);
            float[] results_float = new float[256 * 256];

            for (int i = 0; i < 65536; i++)
            {
                results_float[i] = System.BitConverter.ToSingle(results_bytes, i*4);
                
            }
            yield return results_float;
        }
    }
    //Prend un array de taille divisible par 4 et de taille un carré
    //Renvoie un array de taille divisé par 4 contenant la "moyenne" du précédent array
    public static float[] Reduce(float[] array)
    {
        float[] result = new float[array.Length/4];
        int size = (int)Mathf.Sqrt(array.Length);
        int i = 0;

        for (int z = 0; z < size/2; z++)
        {
            for (int x = 0; x < size/2; x++)
            {
                result[i] = (array[x*2+(z*2*size)] + array[x * 2 + 1 + (z * 2 * size)] + array[x * 2 + (size*(z*2+1))] + array[x * 2 +1 + (size*(z*2+1))])/4;
                i++;
            }
        }

        return result;
    }

    public static float[] RemoveSome(float[] array)
    {
        float[] result = new float[array.Length/4];
        int size = (int)Mathf.Sqrt(array.Length);
        int i = 0;

        for (int z = 0; z < size; z++)
        {
            for (int x = 0; x < size; x++)
            {
                if (x % 4 == 0)
                {
                    Debug.Log(i);
                    result[i] = array[x + (z * size)];
                    i++;
                }
            }
        }
        /*
        for (int j = 0; j < array.Length / 4; j++)
        {
            Debug.Log(result[j]);
        }*/
        return result;
    }

    public static string GetRequest(string layer, string tile_m, string tile_r, string tile_c)
    {
        string url = "http://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER="+ layer +"&TILEMATRIXSET=WGS84G&TILEMATRIX="+ tile_m +"&TILEROW="+ tile_r +"&TILECOL="+ tile_c +"&STYLE=normal&FORMAT=image/x-bil;bits=32";
        return url;
        ;
    }



    IEnumerator ApplyTile(float level, float tile_row, float tile_col, GameObject gameObj)
    {
        Debug.Log("applytile");
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
                plane.transform.position = mygrid.GetNearestPointOnGrid(new Vector3(x, 0f, 10-z));
                plane.transform.Rotate(new_rotate);
                plane.GetComponent<Renderer>().material.color = UnityEngine.Color.white;
                StartCoroutine(ApplyTile(level, z+z0, x+x0, plane));

                yield return null;

            }
        }
    }


    IEnumerator GetTexture(string url)
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
            myplane.GetComponent<Renderer>().material.mainTexture = myTexture;
        }
    }

    public static float[] GetDataFromFile(string path)
    {
        BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open));
        float[] results = new float[256*256];
        Debug.Log("Taille du fichier : " + reader.BaseStream.Length);

        for (int i =0; i<65535; i++)
        {
            results[i] = System.BitConverter.ToSingle(reader.ReadBytes(4), 0);
        }
        Debug.Log(results[0]);
        Debug.Log(results[1]);
        
        reader.Close();
        return results;
    }

    float TileRow(float x0, float x, float zoom)
    {
        float tilerow = (x - x0) / (256 * zoom);
        return tilerow;
    }

    float TileCol(float y0, float y, float zoom)
    {
        float tilecol = (y0 - y) / (256 * zoom);
        return tilecol;
    }




    /*
public IEnumerator GetData2(string myurl)
{
    WWW www = new WWW("https://wxs.ign.fr/j0rmqdk335hqz204nk0awcpn/geoportail/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=ELEVATION.ELEVATIONGRIDCOVERAGE.HIGHRES&TILEMATRIXSET=WGS84G&TILEMATRIX=14&TILEROW=3878&TILECOL=16681&STYLE=normal&FORMAT=image/x-bil;bits=32");
    yield return www;

    if (www.error == null && www.isDone)
    {
        Debug.Log(www.bytesDownloaded);
    }
}
*/

    /*
    void GetData3(string myurl)
    {
        //int a, b, c, d;
        //byte[] t;

        WebClient client = new WebClient();
        client.Headers.Add("Accept-Encoding", "gzip");
        byte[] response = client.DownloadData(myurl);
        Debug.Log(client.ResponseHeaders);
        Debug.Log(response[0]);

        //Stream dataStream = client.OpenRead(myurl);
        //DeflateStream deflateStream = new DeflateStream(dataStream, CompressionMode.Decompress);
        //MemoryStream output = new MemoryStream();
        //deflateStream.CopyTo(output);
        //byte[] arr = output.ToArray(); 
        //byte[] arr = new byte[4];
        //int test = deflateStream.Read(arr, 0, 4);
        //Debug.Log(arr.Length);

        //BinaryReader binaryReader = new BinaryReader(deflateStream);
        //byte[] arr= binaryReader.ReadBytes(4);
        //float toto = System.BitConverter.ToSingle(arr, 0);
        //Debug.Log(toto);

        //BinaryReader reader = new BinaryReader(data);

        //byte[] response = new System.Net.WebClient().DownloadData(myurl);
        //mywebclient zob = new mywebclient();
        //byte[] response = zob.downloaddata(myurl);
        //byte[] response = MyWebClient.DownloadData(myurl);
        //Debug.Log(zob.ResponseHeaders);
        //Debug.Log(response.Length);

        //a = reader.Read();
        //b = reader.Read();
        //c = reader.Read();
        //d = reader.Read();
        //t = new byte[] { (byte)a, 0, (byte)c, 0 };
        //float alti = System.BitConverter.ToSingle(t, 0);
        //Debug.Log(a);
        //Debug.Log(b);
        //Debug.Log(c);
        //Debug.Log(d);

        //Debug.Log(alti);
        //data.Close();
        //reader.Close();
    }
    */

    /* 
  void DownloadFile(string myurl, string path)
  {
      WebClient webClient = new WebClient();
      webClient.DownloadFile(myurl, path);
      byte[] results = File.ReadAllBytes(path);
      Debug.Log(results.Length);
      //byte[] results2 = Deflate(results);
      //Debug.Log(results2.Length);
  }

  void GetData5(string myurl)
  {
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(myurl);
      request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
      //request.CookieContainer = PersistentCookies.GetCookieContainerForUrl(myurl);
      //request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
      //request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us");
      request.UserAgent = "My special app";
      request.KeepAlive = true;
      using (var resp = (HttpWebResponse)request.GetResponse())
      {
          using (Stream s = resp.GetResponseStream())
          {
              var reader = new StreamReader(s);
              var textResponse = reader.ReadToEnd();
              Debug.Log(textResponse);
              s.Close();
          }
      }
  }

  void GetData4(string myurl)
  {
      var client = new WebClient();
      client.Headers[HttpRequestHeader.AcceptEncoding] = "deflate";
      var responseStream = new System.IO.Compression.DeflateStream(client.OpenRead(myurl), System.IO.Compression.CompressionMode.Decompress);
      //Debug.Log(client.ResponseHeaders);

      var reader = new StreamReader(responseStream);
      var textResponse = reader.ReadToEnd();
      Debug.Log(textResponse);
      responseStream.Close();

  }

  public static void Decompress(FileInfo fileToDecompress)
  {
      using (FileStream originalFileStream = fileToDecompress.OpenRead())
      {
          string currentFileName = fileToDecompress.FullName;
          string newFileName = "decompressed_file";

          using (FileStream decompressedFileStream = File.Create(newFileName))
          {
              using (Ionic.Zlib.DeflateStream decompressionStream = new Ionic.Zlib.DeflateStream(originalFileStream, Ionic.Zlib.CompressionMode.Decompress))
              {
                  decompressionStream.CopyTo(decompressedFileStream);
                  Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
              }
          }
      }
  }
  */

}
