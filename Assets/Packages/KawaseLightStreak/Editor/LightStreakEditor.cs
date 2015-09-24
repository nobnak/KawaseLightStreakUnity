using UnityEngine;
using System.Collections;
using UnityEditor;

namespace KawaseLightStreak {

	public static class LightStreakEditor {

		[MenuItem("Assets/Create/KawaseLightStreak/Data")]
		public static void CreateLightStreakData() { ScriptableObjUtil.CreateAsset<LightStreakData>(); }
	}
}