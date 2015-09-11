using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FPS : MonoBehaviour {
	public float interval = 1f;

	private Text _text;
	private int _lastFrame;
	private float _lastTime;

	// Use this for initialization
	void Start () {
		_text = GetComponent<Text>();
		_lastTime = Time.timeSinceLevelLoad;
		_lastFrame = Time.frameCount;
		StartCoroutine(Calc());
	}
	
	IEnumerator Calc() {
		while (true) {
			yield return new WaitForSeconds(interval);

			var fps = UpdateFps();
			_text.text = string.Format("FPS : {0:f1}", fps);
		}
	}

	public float UpdateFps() {
		var fps = (Time.frameCount - _lastFrame) / (Time.timeSinceLevelLoad - _lastTime);
		_lastTime = Time.timeSinceLevelLoad;
		_lastFrame = Time.frameCount;
		return fps;
	}
}
