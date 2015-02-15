using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class KawaseLightStreak : MonoBehaviour {
	public const int PASS_BRIGHT = 0;
	public const int PASS_STREAK = 1;
	public const int PASS_ADD = 2;

	public const string DIR = "_Dir";
	public const string OFFSET = "_Offset";
	public const string ATTEN = "_Atten";
	public const float TWO_PI = 2f * Mathf.PI;

	public Material kawase;
	public float atten = 0.95f;
	public int n = 3;
	public int lod = 2;
	public int shape = 4;
	public float angle = 45f;

	void OnRenderImage(RenderTexture src, RenderTexture dst) {
		var width = src.width >> lod;
		var height = src.height >> lod;
		var rt0 = RenderTexture.GetTemporary(width, height, 0, src.format, RenderTextureReadWrite.Linear);
		var rt1 = RenderTexture.GetTemporary(width, height, 0, src.format, RenderTextureReadWrite.Linear);

		Graphics.Blit(src, dst);

		var dtheta = TWO_PI / shape;
		for (var i = 0; i < shape; i++) {
			rt0.DiscardContents();
			Graphics.Blit(src, rt0, kawase, PASS_BRIGHT);
			LightStreak(ref rt0, ref rt1, i * dtheta + angle * Mathf.Deg2Rad);
			Graphics.Blit(rt0, dst, kawase, PASS_ADD);
		}

		RenderTexture.ReleaseTemporary(rt0);
		RenderTexture.ReleaseTemporary(rt1);
	}

	void LightStreak(ref RenderTexture rt0, ref RenderTexture rt1, float dirInRad) {
		for (var i = 0; i < n; i++) {
			var texelOffset = 1;
			for (var j = 0; j < i; j++)
				texelOffset *= 4;
			var absorb = Mathf.Pow (atten, texelOffset);
			kawase.SetFloat(OFFSET, texelOffset);
			kawase.SetFloat(ATTEN, absorb);
			kawase.SetVector(DIR, new Vector4(Mathf.Cos(dirInRad), Mathf.Sin(dirInRad), 0, 0));
			rt1.DiscardContents();
			Graphics.Blit (rt0, rt1, kawase, PASS_STREAK);
			var tmp = rt0;
			rt0 = rt1;
			rt1 = tmp;
		}
	}
}
