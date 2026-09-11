using UnityEngine;
using UnityEngine.UI;

namespace PC.Component.Software
{
	public class LuaEditor : App
	{
		[SerializeField]
		private InputField input;

		private string filePath;

		public override void Open(string content)
		{
			base.Open(content);
			var i = input;
			if (i != null) i.text = string.IsNullOrEmpty(content) ? "" : content;
			filePath = "Untitled" + ".lua";
		}

		public void OpenFile()
		{
			var o = system;
			if (o == null) return;

			System.Action<File> cb = file =>
			{
				if (file == null) return;
				var i = input;
				if (i != null) i.text = file.content;
				filePath = file.path;
			};

			o.SelectFile("*.lua", cb);
		}

		public void Save()
		{
			var os = system;
			if (os == null) return;
			var dlg = os.SaveDialog;
			if (dlg == null) return;

			var name = File.NameWithoutExtension(filePath);
			var i = input;
			if (i == null) return;
			var content = i.text;

			var extensions = new[] { ".lua" };
			dlg.ShowDialog(name, content, extensions);
		}

		public void Run()
		{
			var os = system;
			if (os == null) return;

			var i = input;
			if (i == null) return;
			var content = i.text;

			// For now, just open in Terminal to simulate running
			os.OpenTerminal(content);
		}
	}
}
