// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// PC.Online.ReportForm
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ReportForm : MonoBehaviour
{
	[Serializable]
	private class ReportRequest
	{
		public int id;
		public string reason;
	}

	[SerializeField]
	private InputField reason;
	[SerializeField]
	private GameObject submit;

	[SerializeField]
	private GameObject loading;

	private int id;

	private MessageBox messageBox;

	public void Show(int id, MessageBox messageBox)
    {
        this.messageBox = messageBox;
        this.id = id;
    }

	public void Submit()
    {
        if (reason.text.Length < 10)
        {
            messageBox.Show("The length of the reason must be no less than 10.");
            return;
        }
        StartCoroutine(SubmitReportForm(reason.text));

    }

	private IEnumerator SubmitReportForm(string reason)
	{
		loading.SetActive(true);
        ReportRequest requestData = new ReportRequest
        {
            id = id,
            reason = reason
        };
        string json = JsonUtility.ToJson(requestData);
        using (UnityWebRequest request = UnityWebRequest.Post("https://api.yimingzz.com/gallery/report", json, "application/json"))
        {
            if (AccountManager.Instance.Ready())
                request.SetRequestHeader("Authorization", AccountManager.User.token);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                messageBox.Show("Report submitted!");
                Close();
            }
            else
            {
                UnityEngine.Debug.LogError(request.error);
                string error = request.downloadHandler.text;
                messageBox.Show(request.responseCode + ": " + request.error + " " + error);
            }
        }
        loading.SetActive(false);
	}

	public void Close()
    {
        Destroy(gameObject);
    }
}
