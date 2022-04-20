using System; 
using UnityEngine;

namespace TexLoader
{
	public class TextureInfo
	{
		public Texture2D tex;
		public DateTime time; 
		public TextureInfo(Texture2D tex, DateTime time)
		{
			this.tex = tex;
			this.time = time;
		}
	}
}
