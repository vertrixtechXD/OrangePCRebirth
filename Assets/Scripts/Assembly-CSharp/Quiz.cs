using UnityEngine;
using UnityEngine.EventSystems;

public class Quiz : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private string[] links;

	private static int count;

	private void Start()
	{
		var ads = AdManager.Instance;
		if (ads == null) return;
		if (ads.NoAds)
		{
			var go = gameObject;
			if (go != null) go.SetActive(false);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		var cb = new System.Action(Play);
		if (ConfirmationDialog.Instance != null) ConfirmationDialog.Instance.Show(@"[AD] Time to Win Now!

Play Bollywood Quizzes  & win coins daily", cb);
	}

	public void Count()
	{
		var ads = AdManager.Instance;
		if (ads == null || ads.NoAds) return;
		count++;
		if (count > 2)
		{
			count = 0;
			var dlg = ConfirmationDialog.Instance;
			var cb = new System.Action(Play);
			if (dlg != null) ConfirmationDialog.Instance.Show(@"[AD] Time to Win Now!

Play Bollywood Quizzes  & win coins daily", cb);
		}
	}

	public void Play()
	{
		if (links == null || links.Length == 0) return;
		int i = Random.Range(0, links.Length);
		Application.OpenURL(links[i]);
	}
}
