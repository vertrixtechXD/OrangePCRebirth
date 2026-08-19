using System;
using System.Collections;
using System.IO;
using SaveManagement;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GalleryFileDetails : MonoBehaviour
{
    [System.Serializable]
    private class FavRequest
    {
        public int id;
    }

    [System.Serializable]
    private class EditRequest
    {
        public int id;
        public string title;
        public string description;
    }

    [SerializeField] private RawImage icon;
    [SerializeField] private RawImage authorAvatar;
    [SerializeField] private Text authorText;

    [SerializeField] private InputField titleInput;
    [SerializeField] private Image titleImage;

    [SerializeField] private InputField descriptionInput;
    [SerializeField] private Image descriptionImage;

    [SerializeField] private Text infoText;
    [SerializeField] private Image fav;

    [SerializeField] private GameObject vip;
    [SerializeField] private GameObject manage;
    [SerializeField] private GameObject edit;
    [SerializeField] private GameObject applyEdit;
    [SerializeField] private GameObject download;

    [SerializeField] private ReportForm reportForm;
    [SerializeField] private Transform downloadBar;
    [SerializeField] private GameObject loading;

    private Gallery gallery;
    private Gallery.SaveInfo info;
    private Gallery.SaveDetails details;
    private MessageBox messageBox;

    private string oldTitle;
    private string oldDesc;

    public void Show(Gallery gallery, Gallery.SaveInfo info, MessageBox messageBox)
    {
        this.gallery = gallery;
        this.info = info;
        this.messageBox = messageBox;
        gameObject.SetActive(true);
        titleInput.text = info.title;
        authorText.text = info.authorName;
        Start();
        bool canEdit = AccountManager.User != null && info.authorId == AccountManager.User.userId;
        manage.SetActive(canEdit);
        StartCoroutine(GameApi.GetTexture($"{GameApi.baseUrl}/gallery/thumbnail/{info.id}", icon));
        StartCoroutine(GameApi.GetTexture(AccountManager.GetAvatarUrlById(info.authorId.ToString()), authorAvatar));
    }

    private void UpdateInfo()
    {
        string version = "Version " + details.version;
        var current = new Version(Application.version);
        var save = new Version(details.version);
        if (!FileMenu.CompareVersionIgnoringBuild(current, save))
            version = "<color=red>" + version + "</color>";
        infoText.text = string.Format(@"Created: {0}
{1} downloads
{2} favs
{3}
{4}",       DateTime.Parse(details.createdAt).ToShortDateString(), 
        info.downloads,
        details.fav,
        Gallery.Size(details.fileSize),
        version);
    }

    private IEnumerator Start()
    {
        bool loggedIn = AccountManager.User != null;
        string url = "https://api.yimingzz.com/gallery";
        string route = loggedIn ? "me/info" : "info";
        string endpoint = $"{url}/{route}/{info.id}";
        using (UnityWebRequest request = UnityWebRequest.Get(endpoint))
        {
            if (loggedIn) request.SetRequestHeader("Authorization", AccountManager.User.token);
            loading.SetActive(true);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                messageBox.Show(request.responseCode + ": " + request.error + " " + request.downloadHandler.text);
                loading.SetActive(false);
                yield break;
            }
            else
            {
                string output = request.downloadHandler.text;
                details = JsonUtility.FromJson<Gallery.SaveDetails>(output);
                fav.color = details.isFav ? new Color(1f, 0f, 0f, 1f) : Color.white;
            }
            loading.SetActive(false);
            if (AccountManager.User != null && info.authorId == AccountManager.User.userId)
            {
                manage.SetActive(true);
            } else
            {
                manage.SetActive(false);
            }
            bool isVip = details.vip;
            vip.SetActive(isVip);

            descriptionInput.text = string.IsNullOrEmpty(details.description) ? "No description." : details.description;
            string version = "Version " + details.version;
            var current = new Version(Application.version);
            var save = new Version(details.version);
            if (!FileMenu.CompareVersionIgnoringBuild(current, save))
                version = "<color=red>" + version + "</color>";
            infoText.text = string.Format(@"Created: {0}
{1} downloads
{2} favs
{3}
{4}",       DateTime.Parse(details.createdAt).ToShortDateString(), 
            info.downloads,
            details.fav,
            Gallery.Size(details.fileSize),
            version);
        }
    }

    public void Download()
    {
        bool loggedIn = AccountManager.User != null;
        string endpoint = loggedIn ? "download" : "visitor/download";
        string url = $"https://api.yimingzz.com/gallery/{endpoint}/{info.id}";
        StartCoroutine(DownloadFile(info.title, url));
    }

    public void Fav()
    {
        if (!AccountManager.Instance.Ready()) return;
        if (details.isFav)
            StartCoroutine(UnfavCouroutine());
        else
            StartCoroutine(FavCoroutine());
    }

    private IEnumerator FavCoroutine()
    {
        fav.GetComponent<Button>().interactable = false;
        var payload = new FavRequest
        {
            id = info.id
        };
        
        string json = JsonUtility.ToJson(payload);
        var request = UnityWebRequest.Post("https://api.yimingzz.com/gallery/fav", json, "application/json");
        var auth = AccountManager.User?.token;
        if (!string.IsNullOrEmpty(auth)) 
            request.SetRequestHeader("Authorization", auth);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            messageBox.Show(request.error);
        }
        else
        {
            details.isFav = true;
            details.fav += 1;
            fav.color = Color.red;
            UpdateInfo();
        }
        fav.GetComponent<Button>().interactable = true;
    }

    private IEnumerator UnfavCouroutine()
    {
        fav.GetComponent<Button>().interactable = false;
        var payload = new FavRequest
        {
            id = info.id
        };
        
        string json = JsonUtility.ToJson(payload);
        var request = UnityWebRequest.Post("https://api.yimingzz.com/gallery/unfav", json, "application/json");
        var auth = AccountManager.User?.token;
        if (!string.IsNullOrEmpty(auth)) 
            request.SetRequestHeader("Authorization", auth);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            messageBox.Show(request.error);
        }
        else
        {
            details.isFav = false;
            details.fav -= 1;
            fav.color = Color.white;
            UpdateInfo();
        }
        fav.GetComponent<Button>().interactable = true;
    }

    public void AskDeleteMessage()
    {
        gallery.deleteConfirmationDialog.Show("Are you sure want to delete?", Delete, "[Yes]", "[No]");
    }

    private void Delete()
    {
        string url = "https://api.yimingzz.com/gallery/" + info.id;
        StartCoroutine(DeleteCoroutine(url));
    }

    private IEnumerator DeleteCoroutine(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            var auth = AccountManager.User?.token;
            if (!string.IsNullOrEmpty(auth)) 
                request.SetRequestHeader("Authorization", auth);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                messageBox.Show(request.error);
                Debug.Log(request.error);
            } else
            {
                Close();
                gallery.Page = 0;
                gallery.RefreshPage();
            }
        }
        yield break;
    }

    private IEnumerator DownloadFile(string name, string url)
    {
        download.SetActive(false);
        downloadBar.gameObject.SetActive(true);
        downloadBar.parent.gameObject.SetActive(true);
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            var auth = AccountManager.User?.token;
            if (!string.IsNullOrEmpty(auth))
               request.SetRequestHeader("Authorization", auth);
            var op = request.SendWebRequest();
            while (!op.isDone)
            {
                float p = request.downloadProgress;
                downloadBar.localScale = new Vector3(p, 1f, 1f);
                yield return null;
            }
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
                messageBox.Show(request.responseCode + ": " + request.error + " " + request.downloadHandler.text);
                downloadBar.gameObject.SetActive(false);
                downloadBar.parent.gameObject.SetActive(false);
                download.SetActive(true);
                yield break;
            }
            else
            {
                byte[] data = GameApi.Decompress(request.downloadHandler.data);
                string text = System.Text.Encoding.UTF8.GetString(data);
                string path = SaveUtility.GetNewPath(SaveUtility.GetSafeFileName(name));
                File.WriteAllText(path, text);
                var loader = new DataLoader(path);
                loader.LoadFromPath();
                MainMenu.Instance.LoadFile(loader);
                downloadBar.gameObject.SetActive(false);
                downloadBar.parent.gameObject.SetActive(false);
                download.SetActive(true);
            }
        }
    }

    public void Report()
    {
        if (!AccountManager.Instance.Ready()) return;
        Instantiate(reportForm, transform).Show(info.id, messageBox);
    }

    public void SearchUser()
    {
        string search = "author:" + info.authorName;
        gallery.Search(search);
        Close();
    }

    public void Edit()
    {
        oldTitle = titleInput.text;
        oldDesc = descriptionInput.text;
        applyEdit.SetActive(true);
        edit.SetActive(false);
        descriptionImage.enabled = true;
        titleImage.enabled = true;
        descriptionInput.readOnly = false;
        titleInput.readOnly = false;
    }

    public void ApplyEdit()
    {
        applyEdit.SetActive(false);
        edit.SetActive(true);
        descriptionImage.enabled = false;
        titleImage.enabled = false;
        descriptionInput.readOnly = true;
        titleInput.readOnly = true;
        if (descriptionInput.text != oldDesc || titleInput.text != oldTitle)
            StartCoroutine(EditCoroutine(titleInput.text, descriptionInput.text));
        else
        {
            descriptionInput.text = oldDesc;
            titleInput.text = oldTitle;
        }
    }

    private IEnumerator EditCoroutine(string title, string description)
    {
        loading.SetActive(true);
        var requestPayload = new EditRequest
        {
            id = info.id,
            title = title,
            description = description
        };
        string json = JsonUtility.ToJson(requestPayload);
        var request = UnityWebRequest.Post("https://api.yimingzz.com/gallery/edit", json, "application/json");
        var auth = AccountManager.User?.token;
        if (!string.IsNullOrEmpty(auth))
            request.SetRequestHeader("Authorization", auth);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            string err = request.responseCode + ": " + request.downloadHandler.text;
            messageBox.Show(err);
            loading.SetActive(false);
            yield break;
        }
        loading.SetActive(false);
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}