using FS.VLTK.Utilities.UnityComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK
{
    /// <summary>
    /// Các hàm toàn cục dùng trong Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
		/// <summary>
		/// Convert an audio clip to a byte array
		/// </summary>
		/// <param name="clip">The audio clip to convert</param>
		/// <returns>A byte array</returns>
		public static byte[] AudioClipToBytes(AudioClip clip)
		{
			float[] samples = new float[clip.samples * clip.channels];
			clip.GetData(samples, 0);

			byte[] data = new byte[clip.samples * clip.channels];
			for (int i = 0; i < samples.Length; i++)
			{
				//convert to the -128 to +128 range
				float conv = samples[i] * 128.0f;
				int c = Mathf.RoundToInt(conv);
				c += 127;
				if (c < 0)
					c = 0;
				if (c > 255)
					c = 255;

				data[i] = (byte) c;
			}

			return data;
		}

		/// <summary>
		/// Convert a byte array to an audio clip
		/// </summary>
		/// <param name="data">The byte array representing an audio clip</param>
		/// <param name="channels">How many channels in the audio data</param>
		/// <param name="frequency">The recording frequency of the audio data</param>
		/// <param name="threedimensional">Whether the audio clip should be 3D</param>
		/// <param name="gain">How much to boost the volume (1.0 = unchanged)</param>
		/// <returns>An AudioClip</returns>
		public static AudioClip BytesToAudioClip(byte[] data, int channels, int frequency, bool threedimensional, float gain)
		{
			float[] samples = new float[data.Length];

			for (int i = 0; i < samples.Length; i++)
			{
				//convert to integer in -128 to +128 range
				int c = (int) data[i];
				c -= 127;
				samples[i] = ((float) c / 128.0f) * gain;
			}

			AudioClip clip = USpeakAudioManager.GetOrCreateAudioClip(data.Length / channels, channels, frequency, threedimensional);
			clip.SetData(samples, 0);
			return clip;
		}
	}
}
