using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class KawaseLightStreak : MonoBehaviour {
	public const int PASS_BRIGHT = 0;
	public const int PASS_STREAK = 1;
	public const int PASS_ADD = 2;

	public const string PROP_GAIN = "_Gain";
	public const string PROP_DIR = "_Dir";
	public const string PROP_OFFSET = "_Offset";
	public const string PROP_ATTEN = "_Atten";
	public const float TWO_PI = 2f * Mathf.PI;

	public enum ShapeEnum { Cross = 0, Star = 1, Snow = 2, Octet = 3 }
	public static readonly string[] ShapeLabels = new string[]{ "Cross", "Star", "Snow", "Octet" };
	public static readonly int[] ShapeNums = new int[]{ 4, 5, 6, 8 };
	
	public Material kawase;
	public float atten = 0.95f;
	public int n = 3;
	public int lod = 2;
	public ShapeEnum shape = ShapeEnum.Cross;
	public float angle = 45f;
	public bool guiOn = false;

	private Rect _win = new Rect(10, 10, 0, 0);

	void OnRenderImage(RenderTexture src, RenderTexture dst) {
		var width = src.width >> lod;
		var height = src.height >> lod;
		var rt0 = RenderTexture.GetTemporary(width, height, 0, src.format, RenderTextureReadWrite.Linear);
		var rt1 = RenderTexture.GetTemporary(width, height, 0, src.format, RenderTextureReadWrite.Linear);

		Graphics.Blit(src, dst);

		var shapeNum = ShapeNums[(int)shape];
		var dtheta = TWO_PI / shapeNum;
		for (var i = 0; i < shapeNum; i++) {
			rt0.DiscardContents();
			Graphics.Blit(src, rt0, kawase, PASS_BRIGHT);
			LightStreak(ref rt0, ref rt1, i * dtheta + angle * Mathf.Deg2Rad);
			Graphics.Blit(rt0, dst, kawase, PASS_ADD);
		}

		RenderTexture.ReleaseTemporary(rt0);
		RenderTexture.ReleaseTemporary(rt1);
	}
	void OnGUI() {
		if (guiOn)
			_win = GUILayout.Window(0, _win, Window, "UI");
	}

	void Window(int id) {
		GUILayout.BeginVertical(GUILayout.Width(200));

		var gain = kawase.GetFloat(PROP_GAIN);
		GUILayout.Label(string.Format("Gain:{0:f2}", gain));
		gain = GUILayout.HorizontalSlider(gain, 0f, 1f);
		kawase.SetFloat(PROP_GAIN, gain);

		GUILayout.Label(string.Format("Angle:{0:f1}", angle));
		angle = GUILayout.HorizontalSlider(angle, 0f, 360f);

		shape = (ShapeEnum)GUILayout.Toolbar((int)shape, ShapeLabels);

		GUILayout.EndVertical();
	}

	void LightStreak(ref RenderTexture rt0, ref RenderTexture rt1, float dirInRad) {
		for (var i = 0; i < n; i++) {
			var texelOffset = 1;
			for (var j = 0; j < i; j++)
				texelOffset *= 4;
			var absorb = Mathf.Pow (atten, texelOffset);
			kawase.SetFloat(PROP_OFFSET, texelOffset);
			kawase.SetFloat(PROP_ATTEN, absorb);
			kawase.SetVector(PROP_DIR, new Vector4(Mathf.Cos(dirInRad), Mathf.Sin(dirInRad), 0, 0));
			rt1.DiscardContents();
			Graphics.Blit (rt0, rt1, kawase, PASS_STREAK);
			var tmp = rt0;
			rt0 = rt1;
			rt1 = tmp;
		}
	}
}
