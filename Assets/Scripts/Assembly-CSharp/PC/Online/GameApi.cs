// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// PC.Online.GameApi
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public static class GameApi
{
	public const string baseUrl = "https://api.yimingzz.com";
	public static IEnumerator GetTexture(string url, RawImage image)
	{
		using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            var operation = request.SendWebRequest();

            yield return operation;

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture texture = DownloadHandlerTexture.GetContent(request);
                image.texture = texture;
            }
            else
            {
                Debug.Log(request.error);
            }
        }
	}

	public static byte[] Compress(byte[] data)
	{
		using (var output = new MemoryStream())
        {
            using (var gzip = new GZipStream(output, CompressionMode.Compress, leaveOpen: true))
            {
                gzip.Write(data, 0, data.Length);
            }

            return output.ToArray();
        }
	}

	public static byte[] Decompress(byte[] compressedData)
    {
        using (var input = new MemoryStream(compressedData))
        using (var gzip = new GZipStream(input, CompressionMode.Decompress))
        using (var output = new MemoryStream())
        {
            gzip.CopyTo(output);
            return output.ToArray();
        }
    }
}
