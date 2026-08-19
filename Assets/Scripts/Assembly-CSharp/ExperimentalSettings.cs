using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
public class ExperimentalSettings : MonoBehaviour
{
	[SerializeField]
	private GameObject[] objs;

	[SerializeField]
	private Text randomTest;

	private void Start()
	{
		if (PlayerPrefs.GetInt("Experimental", 0) == 1)
		{
			foreach (var obj in objs) obj.SetActive(true);
			System.Random r = new System.Random(DateTime.UtcNow.DayOfYear);
			randomTest.text = string.Concat(r.Next(),",",r.Next(),",",r.Next());
		} else foreach (var obj in objs) obj.SetActive(false);
	}

	public void EnableExperimental()
	{
		PlayerPrefs.SetInt("Experimental", 1);
		foreach (var obj in objs) obj.SetActive(true);
	}
}
