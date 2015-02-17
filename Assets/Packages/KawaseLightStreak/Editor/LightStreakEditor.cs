using UnityEngine;
using System.Collections;
using nobnak.Config;
using UnityEditor;

namespace KawaseLightStreak {

	public static class LightStreakEditor {

		[MenuItem("Assets/Create/KawaseLightStreak/Data")]
		public static void CreateLightStreakData() { ScriptableObjUtil.CreateAsset<LightStreakData>(); }
	}
}