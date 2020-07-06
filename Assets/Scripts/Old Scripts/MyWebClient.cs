using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;

public class MyWebClient : WebClient
{
    protected override WebRequest GetWebRequest(Uri address)
    {
        HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
        request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
        //request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        return request;
    }

}