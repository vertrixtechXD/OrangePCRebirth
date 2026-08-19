using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class CustomPaint : MonoBehaviour, ISave
{
	[SerializeField]
	private Renderer rend;
	private byte[] data;

	public void SetTexture(Texture2D tex, byte[] data)
    {
        rend.material.mainTexture = tex;
        this.data = data;
    }

	public void FromData(JObject jObject)
    {
        byte[] dat = Convert.FromBase64String((string)jObject["dat"]);
        SetTexture(FormatConverter.BytesToTexture(dat, true), dat);
    }

	public void ToData(JObject jObject)
    {
        jObject.Add("dat", JToken.FromObject(Convert.ToBase64String(data)));
    }
}
