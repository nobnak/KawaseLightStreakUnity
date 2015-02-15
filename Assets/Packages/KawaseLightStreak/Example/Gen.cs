using UnityEngine;
using System.Collections;

public class Gen : MonoBehaviour {
	public float radius = 10f;
	public int count = 100;
	public GameObject[] prefabs;

	void Start () {
		for (var i = 0; i < count; i++) {
			var go = (GameObject)Instantiate(prefabs[i % prefabs.Length], 
			                                 radius * Random.insideUnitSphere, Random.rotationUniform);
			go.transform.SetParent(transform, false);			
		}
	}
}
