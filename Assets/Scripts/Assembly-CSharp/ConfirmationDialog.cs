using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationDialog : MonoBehaviour
{
	public static ConfirmationDialog Instance { get; private set; }

	[SerializeField]
	private string parameter;

	[SerializeField]
	private Text messageText;

	[SerializeField]
	private Text yesButton;

	[SerializeField]
	private Text noButton;

	private Animator animator;

	private Action callback;

	private void Awake()
	{
		// 1.8.3: глобальный инстанс для онлайн-функций (без уничтожения дублей — в сценах их несколько)
		if (Instance == null)
			Instance = this;
	}

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	public void Show(Action callback)
	{
		this.callback = callback;
		animator.SetBool(parameter, true);
	}

	public void Show(string text, Action callback, string buttonAgree = "Yes", string buttonCancel = "No")
	{
		if (messageText != null) messageText.text = text;
		if (yesButton != null) yesButton.text = buttonAgree;
		if (noButton != null) noButton.text = buttonCancel;
		this.callback = callback;
		animator.SetBool(parameter, true);
	}

	public void Yes()
	{
		if (callback != null)
		{
			callback.Invoke();
			callback = null;
		}
		animator.SetBool(parameter, false);
	}

	public void No()
	{
		callback = null;
		animator.SetBool(parameter, false);
	}
}
