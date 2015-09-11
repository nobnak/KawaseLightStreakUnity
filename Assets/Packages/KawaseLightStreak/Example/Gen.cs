using UnityEngine;
using System.Collections;

public class Gen : MonoBehaviour {
	public enum AlignementEnum { In = 0, On }

	public AlignementEnum alignment;
	public float size = 1f;
	public float radius = 10f;
	public int count = 100;
	public GameObject[] prefabs;

	void Start () {
		for (var i = 0; i < count; i++) {
			var go = (GameObject)Instantiate(prefabs[i % prefabs.Length], 
			                                 Position(), Random.rotationUniform);
			go.layer = gameObject.layer;
			go.transform.SetParent(transform, false);			
			go.transform.localScale = size * Vector3.one;
		}
	}

	public Vector3 Position() {
		switch (alignment) {
		case AlignementEnum.On:
			return radius * Random.onUnitSphere;
		default:
			return radius * Random.insideUnitSphere;
		}
	}
}
