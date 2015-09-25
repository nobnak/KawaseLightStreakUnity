using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml.Serialization;

namespace KawaseLightStreak {

	public class LightStreakData : ScriptableObject {
		public enum FilterModeEnum { Direct = 0, Pyramid }
		public enum ShapeEnum { Cross = 0, Star = 1, Snow = 2, Octet = 3 }

		public static readonly string[] FilterModeLabels = new string[]{ "Direct", "Pyramid" };
		public static readonly string[] ShapeLabels = new string[]{ "Cross", "Star", "Snow", "Octet" };
		public static readonly int[] ShapeNums = new int[]{ 4, 5, 6, 8 };

		public const string PROP_GAIN = "_Gain";
		public const string PROP_DIR = "_Dir";
		public const string PROP_OFFSET = "_Offset";
		public const string PROP_ATTEN = "_Atten";
		public const string PROP_THRESHOLD = "_Thresh";
		public const string PROP_BOUNDARY = "_Boundary";

		public string savePath = "LightStreakData.xml";
		public Material kawase;
		public Data data = new Data();

		[System.Serializable]
		public class Data {
			public FilterModeEnum filter;
			public ShapeEnum shape = ShapeEnum.Cross;
			public float atten = 0.95f;
			public int n = 3;
			public int lod = 2;
			public float angle = 45f;
			public float kawase_gain;
			public float kawase_threshold;
		}

		public void Apply() {
			kawase.SetFloat(PROP_GAIN, data.kawase_gain);
			kawase.SetFloat(PROP_THRESHOLD, data.kawase_threshold);
			kawase.SetInt(PROP_BOUNDARY, data.n * 4);
		}
		public void Save() {
			using (var writer = new StreamWriter(DataFullPath, false, System.Text.Encoding.UTF8))
				Serializer.Serialize(writer, data);
		}
		public void Load() {
			if (!string.IsNullOrEmpty(savePath)) {
				var fullPath = DataFullPath;
				if (File.Exists(fullPath)) {
					using (var reader = new StreamReader(fullPath, System.Text.Encoding.UTF8))
						data = (Data)Serializer.Deserialize(reader);
				} else {
					Debug.LogFormat("Serialized Data Not found of type {0} at {1}", this.GetType(), fullPath);
				}
			}
		}

		public string DataFullPath { get { return Path.Combine(Application.streamingAssetsPath, savePath); } }
		public XmlSerializer Serializer { get { return new XmlSerializer(typeof(Data)); } }
	}
}