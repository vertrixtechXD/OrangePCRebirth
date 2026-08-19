using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AccountManager : MonoBehaviour
{
	[Serializable]
	private class LoginRequest
	{
		public string username;
		public string password;
	}

	[Serializable]
	private class LoginResponse
	{
		public string token;
		public int expires;
		public string username;
	}

	[Serializable]
	private class MeResponse
	{
		public int id;
		public string username;
		public string email;
		public string registrationDate;
		public bool vip;
	}

	[SerializeField]
	private RawImage mainAvatar;

	[SerializeField]
	private Text mainUsernameText;

	[SerializeField]
	private Texture defaultAvatar;

	[SerializeField]
	private GameObject loading;

	[SerializeField]
	private Button loginButton;

	[Header("Login")]
	[SerializeField]
	private InputField usernameInput;

	[SerializeField]
	private InputField passwordInput;

	[SerializeField]
	[Header("Manage")]
	private RawImage avatar;

	[SerializeField]
	private Text displayText;

	[SerializeField]
	private Text emailText;

	[SerializeField]
	private Text registrationDateText;

	[SerializeField]
	private GameObject vip;

	private const string userUrl = "https://api.yimingzz.com/user";

	[SerializeField]
	private MessageBox messageBox;

	private MenuManager menuManager;

	private bool isLoading;

	public static User User {get; private set;}

	public static AccountManager Instance {get; private set;}

	private void Awake()
    {
        Instance = this;
    }

	public bool Ready()
    {
        if (User != null)
            return true;
        if (!isLoading)
            menuManager.ShowMenu("Login");
        else
            messageBox.Show("Logging in, please wait.");
        return false;
    }

	private void Start()
    {
        menuManager = GetComponentInParent<MenuManager>();
        Refresh();
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("Token"))) return;
        StartCoroutine(AccountInfo(PlayerPrefs.GetString("Token")));
    }

	private void Refresh()
    {
        var dat = User;
        if (dat == null) return;
        mainUsernameText.text = dat.username;
        displayText.text = dat.username;
        emailText.text = dat.email;
        registrationDateText.text = DateTime.Parse(dat.registrationDate).ToShortDateString();
        vip.SetActive(dat.vip);
        StartCoroutine(RefreshAvatar());
    }

	private IEnumerator AccountInfo(string token)
	{
		isLoading = true;
        yield return new WaitForSeconds(1f);

        using (UnityWebRequest request = UnityWebRequest.Get("https://api.yimingzz.com/user/me"))
        {
            request.SetRequestHeader("Authorization", token);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                MeResponse response = JsonUtility.FromJson<MeResponse>(json);

                User = new User
                {
                    userId = response.id,
                    username = response.username,
                    email = response.email,
                    registrationDate = response.registrationDate,
                    vip = response.vip,
                    token = token
                };

                Refresh();
            }
            else
            {
                UnityEngine.Debug.Log(request.error);
                PlayerPrefs.DeleteKey("Token");
            }
        }

        isLoading = false;
	}

	public void OpenAccountMenu()
    {
        if (isLoading)
        {
            messageBox.Show("Logging in, please wait.");
            return;
        }

        if (User != null) menuManager.ShowMenu("Manage");
        else menuManager.ShowMenu("Login");
    }

	public static string GetAvatarUrlById(string id)
	{
		return "https://api.yimingzz.com/user/avatars/" + id;
	}

	private IEnumerator RefreshAvatar()
    {
        var id = User.userId.ToString();
        var url = GetAvatarUrlById(id);

        yield return GameApi.GetTexture(url, mainAvatar);
        if (mainAvatar.texture == null)
            mainAvatar.texture = defaultAvatar;
        avatar.texture = mainAvatar.texture;        
    }

	public void Login()
    {
        if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            messageBox.Show("Username and password cannot be empty!");
            return;
        }
        StartCoroutine(LoginCoroutine(usernameInput.text, passwordInput.text));
    }

	private IEnumerator LoginCoroutine(string username, string password)
	{
		loading.SetActive(true);
        var request = new LoginRequest
        {
            username = username,
            password = password
        };

        string json = JsonUtility.ToJson(request);
        using (UnityWebRequest rq = UnityWebRequest.Post("https://api.yimingzz.com/user/login", json, "application/json"))
        {
            yield return rq.SendWebRequest();

            if (rq.result == UnityWebRequest.Result.Success)
            {
                string res = rq.downloadHandler.text;
                PlayerPrefs.SetString("Token", res);
                StartCoroutine(AccountInfo(res));
                menuManager.Back();
            }
            else
            {
                passwordInput.text = "";
                UnityEngine.Debug.LogError(rq.error);
                string error = rq.downloadHandler.text;
                messageBox.Show(error);
                loading.SetActive(false);
            }
        }
        loading.SetActive(false);
	}
    
	public void Logout()
    {
        PlayerPrefs.DeleteKey("Token");
        mainAvatar.texture = defaultAvatar;
        avatar.texture = defaultAvatar;
        User = null;
        mainUsernameText.text = "Login";
        displayText.text = "Username";
        emailText.text = "user@gmail.com";
        registrationDateText.text = "-";
        vip.SetActive(false);
        menuManager.Back();
    }
}
