using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationDialog : MonoBehaviour
{
    public static ConfirmationDialog Instance { get; private set; }

    [SerializeField] private string parameter;
    [SerializeField] private Text messageText;
	[SerializeField] private Text yesButton;
	[SerializeField] private Text noButton;

    private Animator animator;
    private Action callback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        animator = GetComponent<Animator>();
    }

    public void Show(string text, Action callback, string buttonAgree = "Yes", string buttonCancel = "No")
    {
        messageText.text = text;
		yesButton.text = buttonAgree;
		noButton.text = buttonCancel;
        this.callback = callback;
        animator.SetBool(parameter, true);
    }

    public void Yes()
    {
        callback?.Invoke();
        callback = null;
        animator.SetBool(parameter, false);
    }

    public void No()
    {
        callback = null;
        animator.SetBool(parameter, false);
    }
}