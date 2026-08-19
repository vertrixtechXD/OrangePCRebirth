using UnityEngine;
using UnityEngine.UI;

public class CoverImage : MaskableGraphic
{
	[SerializeField]
	private Sprite sprite;

	[SerializeField]
	private bool reverse;

	public Sprite Sprite
	{
		get => sprite;
		set
		{
			if (sprite == value) return;
			sprite = value;
			SetAllDirty();
		}
	}

	public override Texture mainTexture
	{
		get
		{
			var sp = sprite;
			if (sp != null) return sp.texture;

			var mat = material;
			if (mat != null && mat.mainTexture != null) return mat.mainTexture;

			return s_WhiteTexture;
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();

		var rect = GetPixelAdjustedRect();
		var c = color;
		var c32 = new Color32(
			(byte)(c.r * 255f),
			(byte)(c.g * 255f),
			(byte)(c.b * 255f),
			(byte)(c.a * 255f)
		);

		var bl = new Vector3(rect.xMin, rect.yMin, 0f);
		var tl = new Vector3(rect.xMin, rect.yMax, 0f);
		var tr = new Vector3(rect.xMax, rect.yMax, 0f);
		var br = new Vector3(rect.xMax, rect.yMin, 0f);

		Vector2 uvBL = new Vector2(0f, 0f);
		Vector2 uvTL = new Vector2(0f, 1f);
		Vector2 uvTR = new Vector2(1f, 1f);
		Vector2 uvBR = new Vector2(1f, 0f);

		if (sprite != null)
		{
			var outer = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
			var uMin = outer.x;
			var vMin = outer.y;
			var uMax = outer.z;
			var vMax = outer.w;

			if (reverse)
			{
				(uvBL, uvBR) = (uvBR, uvBL);
				(uvTL, uvTR) = (uvTR, uvTL);
			}

			uvBL = new Vector2(reverse ? uMax : uMin, reverse ? vMax : vMin);
			uvTL = new Vector2(reverse ? uMax : uMin, reverse ? vMin : vMax);
			uvTR = new Vector2(reverse ? uMin : uMax, reverse ? vMin : vMax);
			uvBR = new Vector2(reverse ? uMin : uMax, reverse ? vMax : vMin);
		}

		vh.AddVert(bl, c32, uvBL);
		vh.AddVert(tl, c32, uvTL);
		vh.AddVert(tr, c32, uvTR);
		vh.AddVert(br, c32, uvBR);

		vh.AddTriangle(0, 1, 2);
		vh.AddTriangle(2, 3, 0);
	}
}