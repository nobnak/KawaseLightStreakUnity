using UnityEngine;
using System.Collections;

public class GenScreen : MonoBehaviour {
	public int count = 100;
	public float size = 1f;
	public GameObject[] prefabs;
	public Camera targetCam;
	public Vector3 angularSpeed;

	GameObject[] _instances;

	void Start () {
		var posInViewport = targetCam.WorldToViewportPoint(transform.position);
		_instances = new GameObject[count];
		for (var i = 0; i < count; i++) {
			var pos = new Vector3(Random.value, Random.value, posInViewport.z);
			var roundIndex = (Random.value < 0.5f ? 0 : 1);
			pos[roundIndex] = Mathf.Round(pos[roundIndex]);
			pos = targetCam.ViewportToWorldPoint(pos);
			var inst = (GameObject)Instantiate(prefabs[Random.Range(0, prefabs.Length)], pos, Random.rotationUniform);
			inst.transform.localScale = size * Vector3.one;
			inst.transform.SetParent(transform, true);
			_instances[i] = inst;
		}
	}

	void Update() {
		for (var i = 0; i < count; i++)
			_instances[i].transform.localRotation *= Quaternion.Euler(Time.deltaTime * angularSpeed);
	}
}
