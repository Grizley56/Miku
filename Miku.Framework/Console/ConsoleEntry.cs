using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Miku.Framework.Console
{
	public class ConsoleEntry
	{
		public DateTime Time { get; }
		public string Data { get; }
		public Color? TextColor { get; set; }
		public bool TimeVisible { get; set; }


		public ConsoleEntry(string data, Color? color = null, bool showTime = true)
		{
			Data = data;
			TextColor = color;

			TimeVisible = showTime;
			Time = DateTime.Now;
		}
	}
}