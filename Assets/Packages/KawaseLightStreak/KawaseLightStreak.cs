using UnityEngine;
using System.Collections;

namespace KawaseLightStreak {

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

		public LightStreakData data;

		public KeyCode guiKey = KeyCode.K;
		public bool guiOn = false;

		private Rect _win = new Rect(10, 10, 0, 0);

		void OnRenderImage(RenderTexture src, RenderTexture dst) {
			var width = src.width >> data.lod;
			var height = src.height >> data.lod;
			var rt0 = RenderTexture.GetTemporary(width, height, 0, src.format, RenderTextureReadWrite.Linear);
			var rt1 = RenderTexture.GetTemporary(width, height, 0, src.format, RenderTextureReadWrite.Linear);

			Graphics.Blit(src, dst);

			var shapeNum = LightStreakData.ShapeNums[(int)data.shape];
			var dtheta = TWO_PI / shapeNum;
			for (var i = 0; i < shapeNum; i++) {
				rt0.DiscardContents();
				Graphics.Blit(src, rt0, data.kawase, PASS_BRIGHT);
				LightStreak(ref rt0, ref rt1, i * dtheta + data.angle * Mathf.Deg2Rad);
				Graphics.Blit(rt0, dst, data.kawase, PASS_ADD);
			}

			RenderTexture.ReleaseTemporary(rt0);
			RenderTexture.ReleaseTemporary(rt1);
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
		bool CheckCamera() { return camera != null && camera.enabled; }

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

			data.shape = (LightStreakData.ShapeEnum)GUILayout.Toolbar((int)data.shape, LightStreakData.ShapeLabels);

			GUILayout.EndVertical();
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