using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Entities.ActionSet.Character
{
	/// <summary>
	/// Thông tin Frame động tác
	/// </summary>
	public class ActionSetFrameInfo : IDisposable
	{
		/// <summary>
		/// Layer
		/// </summary>
		public int Layer { get; set; }

		/// <summary>
		/// Sprite tương ứng
		/// </summary>
		public Sprite Sprite { get; set; }

		/// <summary>
		/// Hủy đối tượng
		/// </summary>
		public void Dispose()
		{
			GameObject.Destroy(this.Sprite);
		}
	}
}
