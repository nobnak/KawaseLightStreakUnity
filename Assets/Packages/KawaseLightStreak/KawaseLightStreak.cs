﻿using UnityEngine;
using System.Collections;

namespace KawaseLightStreak {

	[ExecuteInEditMode]
	public class KawaseLightStreak : MonoBehaviour {
		public enum OutputModeEnum { Normal = 0, LowSrc, Glow }

		public const int PASS_BRIGHT = 0;
		public const int PASS_STREAK = 1;
		public const int PASS_ADD = 2;
		public const int PASS_FLIP_COPY = 3;

		public const string KEYWORD_GAMMA_OFF = "GAMMA_OFF";
		public const string KEYWORD_GAMMA_ON  = "GAMMA_ON";
		public const string KEYWORD_GAMMA_INV = "GAMMA_INV";

		public const float TWO_PI = 2f * Mathf.PI;

		public LightStreakData data;

		public OutputModeEnum output;
		public KeyCode guiKey = KeyCode.K;
		public bool guiOn = false;
		[SerializeField]
		public RenderTextureEvent OnCreateGlowTex;

		Rect _win = new Rect(10, 10, 0, 0);
		RenderTexture _glowTex;

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

			InitGlowTex(src.width, src.height);
			StarGlow(lowSrc, _glowTex);

			switch (output) {
			case OutputModeEnum.LowSrc:
				Graphics.Blit(lowSrc, dst);
				break;
			case OutputModeEnum.Glow:
				Clear(dst);
				Graphics.Blit(_glowTex, dst, data.kawase, PASS_ADD);
				break;
			default:
				Graphics.Blit(src, dst);
				Graphics.Blit(_glowTex, dst, data.kawase, PASS_ADD);
				break;
			}
			
			if (data.lod > 0)
				RenderTexture.ReleaseTemporary(lowSrc);
		}
		void OnDestroy() {
			ReleaseGlowTex();
		}
		void OnGUI() {
			if (CheckCamera() && guiOn)
				_win = GUILayout.Window(0, _win, Window, "UI");
		}
		void Start() {
			data.Load ();
		}
		void Update() {
			if (CheckCamera() && Input.GetKeyDown(guiKey)) {
				guiOn = !guiOn;
				Cursor.visible = guiOn;
			}
		}

		void ReleaseGlowTex() {
			if (Application.isPlaying)
				Destroy (_glowTex);
			else
				DestroyImmediate (_glowTex);
		}
		void InitGlowTex(int width, int height) {
			if (_glowTex == null || _glowTex.width != width || _glowTex.height != height) {
				ReleaseGlowTex();
				_glowTex = new RenderTexture(width, height, 24);
				_glowTex.antiAliasing = (QualitySettings.antiAliasing == 0 ? 1 : QualitySettings.antiAliasing);
				_glowTex.filterMode = FilterMode.Bilinear;
				_glowTex.wrapMode = TextureWrapMode.Clamp;
				OnCreateGlowTex.Invoke(_glowTex);
			}
			Clear(_glowTex);
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

		void Clear (RenderTexture target) {
			var old = RenderTexture.active;
			RenderTexture.active = target;
			GL.Clear (true, true, Color.clear);
			RenderTexture.active = old;
		}

		bool CheckCamera() { return GetComponent<Camera>() != null && GetComponent<Camera>().enabled; }

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

			var gain = data.kawase_gain;
			GUILayout.Label(string.Format("Gain:{0:f2}", gain));
			gain = GUILayout.HorizontalSlider(gain, 0f, 1f);
			data.kawase_gain = gain;

			var thresh = data.kawase_threshold;
			GUILayout.Label (string.Format ("Threshold:{0:f2}", thresh));
			thresh = GUILayout.HorizontalSlider (thresh, 0f, 1f);
			data.kawase_threshold = thresh;

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

			data.Load ();
		}

		void LightStreak(ref RenderTexture rt0, ref RenderTexture rt1, float dirInRad) {
			for (var i = 0; i < data.n; i++) {
				var texelOffset = 1;
				for (var j = 0; j < i; j++)
					texelOffset *= 4;
				var absorb = Mathf.Pow (data.atten, texelOffset);
				data.kawase.SetFloat(LightStreakData.PROP_OFFSET, texelOffset);
				data.kawase.SetFloat(LightStreakData.PROP_ATTEN, absorb);
				data.kawase.SetVector(LightStreakData.PROP_DIR, new Vector4(Mathf.Cos(dirInRad), Mathf.Sin(dirInRad), 0, 0));
				rt1.DiscardContents();
				Graphics.Blit (rt0, rt1, data.kawase, PASS_STREAK);
				var tmp = rt0;
				rt0 = rt1;
				rt1 = tmp;
			}
		}
	}

	[System.Serializable]
	public class RenderTextureEvent : UnityEngine.Events.UnityEvent<RenderTexture> {}
}