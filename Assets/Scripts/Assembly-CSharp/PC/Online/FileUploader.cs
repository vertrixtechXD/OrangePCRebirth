// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// PC.Online.FileUploader
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using SaveManagement;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FileUploader : MonoBehaviour
{
	[SerializeField]
	private RawImage thumbnail;

	[SerializeField]
	private Texture2D defaultThumbnail;

	[SerializeField]
	private InputField titleInput;

	[SerializeField]
	private InputField descriptionInput;

	[SerializeField]
	private MessageBox messageBox;

	[SerializeField]
	private MenuManager menuManager;

	[SerializeField]
	private GameObject loading;

	private DataLoader loader;
	public void Show(DataLoader loader)
    {
        this.loader = loader;
        titleInput.text = loader.GameData.roomName;
        Texture2D tex = string.IsNullOrEmpty(loader.GameData.icon) ? defaultThumbnail : FormatConverter.StringToTexture(loader.GameData.icon);
        thumbnail.texture = tex;
        menuManager.ShowMenu("FileUploader");
    }

	public void Upload()
    {
        if (titleInput.text == "") {messageBox.Show(Localization.GetText("Title cannot be empty!"));return;};
        if (ConfirmationDialog.Instance != null) ConfirmationDialog.Instance.Show(Localization.GetText("disclaimer_upload"), Agree, Localization.GetText("I Agree"), Localization.GetText("Cancel"));
    }

    public void Agree()
    {
        byte[] file = GameApi.Compress(File.ReadAllBytes(loader.Path));
        byte[] image = !string.IsNullOrEmpty(loader.GameData.icon) ? Convert.FromBase64String(loader.GameData.icon) : ImageConversion.EncodeToPNG(defaultThumbnail);
        string title = titleInput.text;
        string description = descriptionInput.text;
        string token = AccountManager.User.token;
        StartCoroutine(Upload(token, title, description, file, image));
    }
    
	private IEnumerator Upload(string token, string title, string description, byte[] fileData, byte[] thumbnailData)
    {
        // FileUploader.<Upload> MoveNext
        loading.SetActive(true);
        UnityWebRequest request = null;
        try
        {
            var form = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("title", title),
                new MultipartFormFileSection("file", fileData, Path.GetFileName(loader.Path), "application/gzip"),
                new MultipartFormFileSection("thumbnail", thumbnailData, "thumb.png", "image/png")
            };
            if (!string.IsNullOrEmpty(description)) {
                form.Add(new MultipartFormDataSection("description",description));
            }
            request = UnityWebRequest.Post("https://api.yimingzz.com/gallery/upload", form);
            request.SetRequestHeader("Authorization",token);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                messageBox.Show("File upload successfully!");
                menuManager.Back();
            }
            else
            {
                messageBox.Show(request.responseCode + ": " + request.error + " " + request.downloadHandler.text);
            }
        }
        finally
        {
            loading.SetActive(false);
            if (request != null)
                request.Dispose();
        }
    }
}

