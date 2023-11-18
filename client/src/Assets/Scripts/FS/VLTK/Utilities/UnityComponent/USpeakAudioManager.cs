using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
	/// <summary>
	/// Helper class to emulate the functionality of AudioSource.PlayClipAtPoint, while adding an additional 'delay' parameter
	/// </summary>
	public class USpeakAudioManager
	{
		#region Private Fields

		private static List<AudioSource> audioPool = new List<AudioSource>(); //we're going to play a lot of these, use a pool in order to not waste memory

		private static List<AudioClip> clipPool = new List<AudioClip>(); //a pool of audio clips

		#endregion

		#region Static Methods

		/// <summary>
		/// Play the given clip at the given point in space, with a delay of given samples
		/// </summary>
		/// <param name="clip">The clip to play</param>
		/// <param name="position">Where to play it from</param>
		/// <param name="delay">How many samples to delay</param>
		/// <param name="calcPan">Whether to calculate speaker pan based on position</param>
		public static AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position, ulong delay, bool calcPan = false, bool usePooledClips = true)
		{
			AudioSource src = GetAudioSource();
			src.transform.position = position;
			if (usePooledClips)
			{
				if (src.clip != null)
					PoolAudioClip(src.clip);
			}
			else
			{
				if (src.clip != null)
				{
					AudioClip.DestroyImmediate(src.clip);
				}
			}
			src.clip = clip;
			if (calcPan)
			{
				src.panStereo = -Vector3.Dot(Vector3.Cross(Camera.main.transform.forward, Vector3.up).normalized, (position - Camera.main.transform.position).normalized);
			}
			src.PlayDelayed(delay * 44100.0f);
			return src;
		}

		/// <summary>
		/// Stop all currently playing audio sources
		/// </summary>
		public static void StopAll()
		{
			foreach (AudioSource src in audioPool)
			{
				src.Stop();
			}
		}

		public static AudioClip GetOrCreateAudioClip(int lenSamples, int channels, int frequency, bool threeD)
		{
			AudioClip pooled_clip = ReturnPooledAudioClip(lenSamples, channels);
			if (pooled_clip != null)
			{
				RemoveFromPool(pooled_clip);
				return pooled_clip;
			}
			else
			{
				return CreateAudioClip(lenSamples, channels, frequency, threeD);
			}
		}

		public static AudioClip CreateAudioClip(int lenSamples, int channels, int frequency, bool threeD)
		{
			return AudioClip.Create("clip", lenSamples / channels, channels, frequency, threeD, false);
		}

		public static void RemoveFromPool(AudioClip clip)
		{
			clipPool.Remove(clip);
		}

		public static AudioClip ReturnPooledAudioClip(int samples, int channels)
		{
			foreach (AudioClip clip in clipPool)
			{
				if (clip.channels == channels && clip.samples == samples / channels)
				{
					return clip;
				}
				else
				{
					continue;
				}
			}
			return null;
		}

		public static void PoolAudioClip(AudioClip clip)
		{
			clipPool.Add(clip);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Allocate an audio source. Either creates a new one and adds it to the pool, or gets an inactive one already in the pool.
		/// </summary>
		/// <returns></returns>
		private static AudioSource GetAudioSource()
		{
			AudioSource pooled = FindInactiveAudioFromPool();
			if (pooled == null)
			{
				GameObject src = new GameObject();
				src.hideFlags = HideFlags.HideInHierarchy;
				pooled = src.AddComponent<AudioSource>();
				audioPool.Add(pooled);
			}
			return pooled;
		}

		/// <summary>
		/// Search the audio source pool for an inactive audio source.
		/// </summary>
		/// <returns></returns>
		private static AudioSource FindInactiveAudioFromPool()
		{
			Cleanup();
			foreach (AudioSource source in audioPool)
			{
				if (!source.isPlaying)
					return source;
			}
			return null;
		}

		/// <summary>
		/// Clean up null audio sources, this could happen
		/// if you load a new scene and the audio sources are destroyed.
		/// </summary>
		private static void Cleanup()
		{
			audioPool.RemoveAll(delegate (AudioSource src) { return src == null; });
		}

		#endregion
	}
}
