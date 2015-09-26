using UnityEngine;
using System.Collections;

namespace KawaseLightStreak {
	[RequireComponent(typeof(Camera))]
	public class ChannelViewer : MonoBehaviour {
		public enum ChannelEnum { Red = 1, Green = 2, Blue = 4, Alpha = 8, Normal = Red | Green | Blue | Alpha }

		public const string PROP_CHANNEL = "_Channel";

		public ChannelEnum channel;
		public Material channelViewer;

		Vector4 _channel;

		void OnRenderImage(RenderTexture src, RenderTexture dst) {
			UpdateChannel ();
			channelViewer.SetVector (PROP_CHANNEL, _channel);
			Graphics.Blit (src, dst, channelViewer);
		}

		void UpdateChannel() {
			_channel = Vector4.zero;
			var i = (int)channel;
			_channel.x = (i & (int)ChannelEnum.Red) != 0 ? 1f : 0f;
			_channel.y = (i & (int)ChannelEnum.Green) != 0 ? 1f : 0f;
			_channel.z = (i & (int)ChannelEnum.Blue) != 0 ? 1f : 0f;
			_channel.w = (i & (int)ChannelEnum.Alpha) != 0 ? 1f: 0f;
		}
	}
}