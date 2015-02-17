using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace nobnak.Config {

	public static class ScriptableObjUtil {

		public static void CreateAsset<T>() where T : ScriptableObject {
			var path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (path == "")
				path = "Assets";
			else if (!Directory.Exists(path)) {
				var lastSlash = path.LastIndexOf("/");
				if (lastSlash >= 0)
					path = path.Substring(0, lastSlash);
			}
			path = AssetDatabase.GenerateUniqueAssetPath(path + "/" + typeof(T).Name + ".asset");

			var asset = ScriptableObject.CreateInstance<T>();
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;
		}
	}
}