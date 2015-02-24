using UnityEngine;
using System.Collections;

namespace KawaseLightStreak {

	[ExecuteInEditMode]
	public class KawaseLightStreak : MonoBehaviour {
		public enum OutputModeEnum { Normal = 0, LowSrc }

		public const int PASS_BRIGHT = 0;
		public const int PASS_STREAK = 1;
		public const int PASS_ADD = 2;
		public const int PASS_FLIP_COPY = 3;

		public const string KEYWORD_GAMMA_OFF = "GAMMA_OFF";
		public const string KEYWORD_GAMMA_ON  = "GAMMA_ON";
		public const string KEYWORD_GAMMA_INV = "GAMMA_INV";

		public const string PROP_GAIN = "_Gain";
		public const string PROP_DIR = "_Dir";
		public const string PROP_OFFSET = "_Offset";
		public const string PROP_ATTEN = "_Atten";
		public const float TWO_PI = 2f * Mathf.PI;

		public LightStreakData data;

		public OutputModeEnum output;
		public KeyCode guiKey = KeyCode.K;
		public bool guiOn = false;

		private Rect _win = new Rect(10, 10, 0, 0);

		void OnRenderImage(RenderTexture src, RenderTexture dst) {
			var lowSrc = src;
			switch (data.filter) {
			case LightStreakData.FilterModeEnum.Pyramid:
				if (data.lod > 0) {
					lowSrc = Half(src, true);
					for (var i = 1; i < data.lod; i++) {
						var lowDst = Half(lowSrc, true);
						RenderTexture.ReleaseTemporary(lowSrc);
						lowSrc = lowDst;
					}
				}
				break;
			case LightStreakData.FilterModeEnum.Direct:
				lowSrc = RenderTexture.GetTemporary(src.width >> data.lod, src.height >> data.lod, 0, src.format);
				data.kawase.EnableKeyword(KEYWORD_GAMMA_OFF);
				Graphics.Blit(src, lowSrc, data.kawase, PASS_FLIP_COPY);
				break;
			}

			switch (output) {
			case OutputModeEnum.LowSrc:
				Graphics.Blit(lowSrc, dst);
				break;
			default:
				Graphics.Blit(src, dst);
				StarGlow(lowSrc, dst);
				break;
			}
			
			if (data.lod > 0)
				RenderTexture.ReleaseTemporary(lowSrc);
		}
		void OnGUI() {
			if (CheckCamera() && guiOn)
				_win = GUILayout.Window(0, _win, Window, "UI");
		}
		void Update() {
			if (CheckCamera() && Input.GetKeyDown(guiKey)) {
				guiOn = !guiOn;
				Screen.showCursor = guiOn;
			}
		}

		void StarGlow(RenderTexture lowSrc, RenderTexture dst) {
			var shapeNum = LightStreakData.ShapeNums [(int)data.shape];
			var rt0 = RenderTexture.GetTemporary (lowSrc.width, lowSrc.height, 0, lowSrc.format, RenderTextureReadWrite.Linear);
			var rt1 = RenderTexture.GetTemporary (lowSrc.width, lowSrc.height, 0, lowSrc.format, RenderTextureReadWrite.Linear);
			var dtheta = TWO_PI / shapeNum;
			for (var i = 0; i < shapeNum; i++) {
				rt0.DiscardContents ();
				Graphics.Blit (lowSrc, rt0, data.kawase, PASS_BRIGHT);
				LightStreak (ref rt0, ref rt1, i * dtheta + data.angle * Mathf.Deg2Rad);
				Graphics.Blit (rt0, dst, data.kawase, PASS_ADD);
			}
			RenderTexture.ReleaseTemporary (rt0);
			RenderTexture.ReleaseTemporary (rt1);
		}

		bool CheckCamera() { return camera != null && camera.enabled; }

		RenderTexture Half(RenderTexture src, bool flip) {
			var width = src.width >> 1;
			var height = src.height >> 1;
			var dst = RenderTexture.GetTemporary(width, height, 0, src.format);
			dst.filterMode = FilterMode.Bilinear;
			data.kawase.EnableKeyword(KEYWORD_GAMMA_OFF);
			if (flip)
				Graphics.Blit(src, dst, data.kawase, PASS_FLIP_COPY);
			else
				Graphics.Blit(src, dst);
			return dst;
		}
		void Window(int id) {
			GUILayout.BeginVertical(GUILayout.Width(200));

			var gain = data.kawase.GetFloat(PROP_GAIN);
			GUILayout.Label(string.Format("Gain:{0:f2}", gain));
			gain = GUILayout.HorizontalSlider(gain, 0f, 1f);
			data.kawase.SetFloat(PROP_GAIN, gain);

			GUILayout.Label(string.Format("Atten:{0:f3}", data.atten));
			data.atten = GUILayout.HorizontalSlider(data.atten, 0.9f, 0.95f);

			GUILayout.Label(string.Format("Angle:{0:f1}", data.angle));
			data.angle = GUILayout.HorizontalSlider(data.angle, 0f, 360f);

			GUILayout.Label("Shape");
			data.shape = (LightStreakData.ShapeEnum)GUILayout.Toolbar((int)data.shape, LightStreakData.ShapeLabels);

			GUILayout.Label("Filter");
			data.filter = (LightStreakData.FilterModeEnum)GUILayout.Toolbar((int)data.filter, LightStreakData.FilterModeLabels);

			GUILayout.EndVertical();
			GUI.DragWindow();
		}

		void LightStreak(ref RenderTexture rt0, ref RenderTexture rt1, float dirInRad) {
			for (var i = 0; i < data.n; i++) {
				var texelOffset = 1;
				for (var j = 0; j < i; j++)
					texelOffset *= 4;
				var absorb = Mathf.Pow (data.atten, texelOffset);
				data.kawase.SetFloat(PROP_OFFSET, texelOffset);
				data.kawase.SetFloat(PROP_ATTEN, absorb);
				data.kawase.SetVector(PROP_DIR, new Vector4(Mathf.Cos(dirInRad), Mathf.Sin(dirInRad), 0, 0));
				rt1.DiscardContents();
				Graphics.Blit (rt0, rt1, data.kawase, PASS_STREAK);
				var tmp = rt0;
				rt0 = rt1;
				rt1 = tmp;
			}
		}
	}
}