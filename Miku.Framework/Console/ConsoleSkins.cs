using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Miku.Framework.Console
{
	public static class ConsoleSkins
	{
		private static ConsoleSkin _dark;
		private static ConsoleSkin _light;

		public static ConsoleSkin Dark => (ConsoleSkin)_dark.Clone();
		public static ConsoleSkin Light => (ConsoleSkin)_light.Clone();

		static ConsoleSkins()
		{
			_dark = new ConsoleSkin
			{
				AutoCompleteBorder = Color.Black * 0.25f,
				AutoCompleteSelectedTextColor = Color.Gold,
				AutoCompleteField = new ConsoleSkin.FieldSkin(Color.Black * 0.6f, Color.Gray * 0.8f),
				ConsoleBackground = new Color(33, 28, 46) * 0.8f,
				HistoryField = new ConsoleSkin.FieldSkin(Color.Black * 0.3f, Color.White),
				DefaultHistoryTimeColor = Color.Green,
				HistoryWarningColor = Color.Red,
				CursorColor = new Color(185, 214, 255),
				CursorWidth = 2,
				HighlightColor = Color.LightGray * 0.3f,
				InputField = new ConsoleSkin.FieldSkin(Color.Black * 0.3f, new Color(185, 214, 255)),
				ScrollBarColor = new Color(26, 28, 47) * 1f,
				ScrollBarPadding = new Point(0,0),
				ScrollBarStripColor = Color.Black * 0.5f,
				ScrollBarWidth = 7,
				CursorBlinkSpeed = TimeSpan.FromSeconds(0.5f)
			};

			_light = new ConsoleSkin
			{
				ConsoleBackground = Color.OldLace * 0.5f,
				HistoryField = new ConsoleSkin.FieldSkin(Color.White * 0.45f, new Color(109, 0, 132)),
				InputField = new ConsoleSkin.FieldSkin(Color.White * 0.45f, new Color(0, 29, 94)),
				CursorColor = new Color(0, 29, 94),
				CursorWidth = 2,
				AutoCompleteField = new ConsoleSkin.FieldSkin(Color.White * 0.65f, Color.DarkViolet),
				AutoCompleteBorder = Color.DarkGray * 0.9f,
				AutoCompleteSelectedTextColor = Color.Lime,
				DefaultHistoryTimeColor = new Color(0, 72, 170),
				HistoryWarningColor = Color.DarkRed,
				HighlightColor = Color.OliveDrab * 0.5f,
				ScrollBarColor = Color.Gray * 0.4f,
				ScrollBarStripColor = Color.Black * 0.25f,
				ScrollBarWidth = 7,
				CursorBlinkSpeed = TimeSpan.FromSeconds(0.5f),
				ScrollBarPadding = new Point(1, 1)
			};

		}
	}
}