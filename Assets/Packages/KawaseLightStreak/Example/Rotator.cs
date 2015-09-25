using UnityEngine;
using System.Collections;

namespace KawaseLightStreak.Example {
	public class Rotator : MonoBehaviour {
		public Vector3 rotSpeed = Vector3.zero;

		void Update () {
			transform.localRotation *= Quaternion.Euler(rotSpeed * Time.deltaTime);
		}
	}
}