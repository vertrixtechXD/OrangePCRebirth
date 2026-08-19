using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Gallery : MonoBehaviour
{
    [Serializable]
    public class AllListRequest
    {
        public string q;
        public int page;
        public string sort;
    }

    [Serializable]
    public class ListRequest
    {
        public int page;
    }

    [Serializable]
    public class ListResponse
    {
        public int total;
        public SaveInfo[] data;
    }

    [Serializable]
    public class SaveInfo
    {
        public int id;
        public string title;
        public int downloads;
        public int authorId;
        public string authorName;
    }

    [Serializable]
    public class SaveDetails
    {
        public string description;
        public string version;
        public string createdAt;
        public int fileSize;
        public int fav;
        public bool vip;
        public bool isFav;
    }

    private enum ListType
    {
        All,
        Fav,
        MyOwn
    }

    [SerializeField] private GameObject search;
    [SerializeField] private InputField searchInput;
    [SerializeField] private Text totalPagesText;
    [SerializeField] private Text sortText;
    [SerializeField] private GalleryItem galleryItem;
    [SerializeField] private Transform galleryParent;
    [SerializeField] private GameObject loading;
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private MessageBox messageBox;
    [SerializeField] private GalleryFileDetails itemDetailsPrefab;
    [SerializeField] private InputField pageInput;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject sort;

    public ConfirmationDialog deleteConfirmationDialog;

    public const string url = "https://api.yimingzz.com/gallery";
    private const int pageSize = 10;

    private string sortType;
    private ListType listType;
    private int maxPage;
    private int page;

    public int Page
    {
        get => page;
        set {
            page = value;
            pageInput.text = value.ToString();
        }
    }

    private void Start()
    {
        StopAllCoroutines();
        StartCoroutine(ListFiles());
    }

    public void Search(string str)
    {
        searchInput.text = str;
        Page = 0;
        RefreshPage();
    }

    public void Search()
    {
        Page = 0;
        RefreshPage();
    }

    public void ListAll()
    {
        listType = ListType.All;
        Page = 0;
        RefreshPage();
        search.SetActive(true);
        sort.SetActive(true);
    }

    public void ListFav()
    {
        if (!AccountManager.Instance.Ready()) return;
        listType = ListType.Fav;
        search.SetActive(false);
        sort.SetActive(false);
        Page = 0;
        RefreshPage();
    }

    public void ListMyOwn()
    {
        if (!AccountManager.Instance.Ready()) return;
        listType = ListType.MyOwn;
        search.SetActive(false);
        sort.SetActive(false);
        Page = 0;
        RefreshPage();
    }

    public void SortByDate()
    {
        sortType = "date";
        Page = 0;
        RefreshPage();
    }

    public void SortByFav()
    {
        sortType = "fav";
        Page = 0;
        RefreshPage();
    }

    public void SortByDownloads()
    {
        sortType = "downloads";
        Page = 0;
        RefreshPage();
    }

    public void RefreshPage()
    {
        StopAllCoroutines();
        StartCoroutine(ListFiles());
    }

    private IEnumerator ListFiles() 
    {
        for (int i = galleryParent.childCount - 1; i >= 0; i--)
        {
            Destroy(galleryParent.GetChild(i).gameObject);
        }
        loading.SetActive(true);
        object requestData = null;
        string endpoint = url;
        if (listType == ListType.All)
        {
            requestData = new AllListRequest
            {
                page = Page,
                sort = sortType,
                q = searchInput.text
            };
            endpoint += "/list";
        }
        else if (listType == ListType.Fav)
        {
            requestData = new ListRequest
            {
                page = Page
            };
            endpoint += "/fav/list";
        }
        else if (listType == ListType.MyOwn)
        {
            requestData = new ListRequest
            {
                page = Page
            };
            endpoint += "/me/list";
        }
        string json = JsonUtility.ToJson(requestData);
        using (UnityWebRequest request = UnityWebRequest.Post(endpoint, json, "application/json"))
        {
            if (AccountManager.User != null)
                request.SetRequestHeader("Authorization", AccountManager.User.token);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                loading.SetActive(false);
                yield return null;
            }
            else
            {
                string output = request.downloadHandler.text;
                ListResponse response = JsonUtility.FromJson<ListResponse>(output);
                maxPage = (response.total - 1) / pageSize;
                totalPagesText.text = "/ " + maxPage.ToString();
                if (Page > maxPage) Page = maxPage;
                previousButton.interactable = Page > 0;
                nextButton.interactable = Page < maxPage;
                foreach (SaveInfo dat in response.data)
                {
                    var entry = Instantiate(galleryItem, galleryParent);
                    entry.title.text = dat.title;
                    entry.downloadCount.text = dat.downloads.ToString();
                    entry.author.text = dat.authorName;
                    entry.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        var details = Instantiate(itemDetailsPrefab, transform);
                        details.Show(this, dat, messageBox);
                    });
                    string thumbnail = string.Format("{0}/thumbnail/{1}", url, dat.id);
                    StartCoroutine(GameApi.GetTexture(thumbnail, entry.icon));
                }
                loading.SetActive(false);
            }
        }
    }

    public void PreviousPage()
    {
        Page -= 1;
        if (Page < 1) previousButton.interactable = false;
        nextButton.interactable = true;
        RefreshPage();
    }

    public void NextPage()
    {
        Page += 1;
        if (Page >= maxPage) nextButton.interactable = false;
        previousButton.interactable = true;
        RefreshPage();
    }

    public void OnSubmitPage(string value)
    {
        int parse = Int32.Parse(value);
        if (parse > maxPage) Page = maxPage;
        if (parse < 0) Page = 0;
        RefreshPage();
    }

    public static string Size(int i)
    {
        if (i < 1000) return i.ToString() + " bytes";
        if (i < 1000000) return (i / 1000f).ToString("0.#") + "KB";
        return (i / 1000000f).ToString("0.#") + "MB";
    }
}