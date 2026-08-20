using System.Collections.Generic;
using UnityEngine;

public static class InputManager
{
	private static Dictionary<string, float> axis;

	private static Dictionary<string, bool> virtualButtons;

	static InputManager()
	{
		axis = new Dictionary<string, float>();
		virtualButtons = new Dictionary<string, bool>();
	}

	// ПК (Standalone/Editor) — официальное управление:
	// WASD/мышь/колёсико идут через штатную Input-систему Unity (ProjectSettings/InputManager.asset).
	// Мобилка — виртуальные оси от джойстика/тачпада.
	public static bool PcInput
	{
		get
		{
#if UNITY_ANDROID || UNITY_IOS
			return Application.isEditor;
#else
			return !Application.isMobilePlatform;
#endif
		}
	}

	public static void ShowCursor(bool show)
	{
		Cursor.visible = show;
#if !UNITY_ANDROID && !UNITY_IOS
		if (PcInput)
			Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
#endif
	}

	public static void UnregisterAxis(string axisName)
	{
		if (axisName == null) return;

		axis.Remove(axisName);
	}

	public static void UpdateAxis(string axisName, float value)
	{
		if (axisName == null) return;

		axis[axisName] = value;
	}

	public static float GetAxis(string axisName)
	{
		if (axisName == null) return 0f;

		float val;
		if (!axis.TryGetValue(axisName, out val)) val = 0f;

		if (!PcInput)
			return val;

		float unityVal = 0f;
		try
		{
			unityVal = Input.GetAxis(axisName);
		}
		catch (System.Exception)
		{
			unityVal = 0f;
		}

		return Mathf.Abs(val) > Mathf.Abs(unityVal) ? val : unityVal;
	}

	public static bool GetButton(string name)
	{
		if (name == null) return false;

		bool val;
		if (virtualButtons.TryGetValue(name, out val) && val)
			return true;

		if (!PcInput)
			return false;

		try
		{
			return Input.GetButton(name);
		}
		catch (System.Exception)
		{
			return false;
		}
	}

	public static bool GetButtonDown(string name)
	{
		if (name == null) return false;

		bool val;
		if (virtualButtons.TryGetValue(name, out val) && val)
		{
			virtualButtons[name] = false;
			return true;
		}

		if (!PcInput)
			return false;

		try
		{
			return Input.GetButtonDown(name);
		}
		catch (System.Exception)
		{
			return false;
		}
	}

	public static void SetButtonDown(string name)
	{
		if (name == null) return;

		virtualButtons[name] = true;
	}

	public static void SetButtonUp(string name)
	{
		if (name == null) return;

		virtualButtons[name] = false;
	}
}
