using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Miku.Framework.Console
{
	public class ConsoleSkin: ICloneable
	{
		public class FieldSkin: ICloneable
		{
			public Color BackColor { get; set; }
			public Color TextColor { get; set; }

			public FieldSkin(Color backColor, Color textColor)
			{
				BackColor = backColor;
				TextColor = textColor;
			}

			public object Clone() => new FieldSkin(BackColor, TextColor);
		}

		public Color ConsoleBackground { get; set; }
		public Color HighlightColor { get; set; }
		public Color CursorColor { get; set; }
		public Color AutoCompleteBorder { get; set; }

		public Color AutoCompleteSelectedTextColor { get; set; }

		public Color ScrollBarColor { get; set; }
		public Color ScrollBarStripColor { get; set; }
		public Point ScrollBarPadding { get; set; }

		public int ScrollBarWidth { get; set; }
		public int CursorWidth { get; set; }

		public FieldSkin InputField { get; set; }
		public FieldSkin HistoryField { get; set; }
		public FieldSkin AutoCompleteField { get; set; }

		public TimeSpan CursorBlinkSpeed { get; set; }

		public Color DefaultHistoryTimeColor { get; set; }
		public Color HistoryWarningColor { get; set; }

		public object Clone()
		{
			return new ConsoleSkin
			{
				ConsoleBackground = ConsoleBackground,
				HighlightColor = HighlightColor,
				CursorColor = CursorColor,
				AutoCompleteBorder = AutoCompleteBorder,
				AutoCompleteSelectedTextColor = AutoCompleteSelectedTextColor,
				ScrollBarColor = ScrollBarColor,
				ScrollBarStripColor = ScrollBarStripColor,
				ScrollBarPadding = ScrollBarPadding,
				ScrollBarWidth = ScrollBarWidth,
				CursorWidth = CursorWidth,
				CursorBlinkSpeed = CursorBlinkSpeed,
				DefaultHistoryTimeColor = DefaultHistoryTimeColor,
				HistoryWarningColor = HistoryWarningColor,
				InputField = (FieldSkin)InputField.Clone(),
				HistoryField = (FieldSkin)HistoryField.Clone(),
				AutoCompleteField = (FieldSkin)AutoCompleteField.Clone()
			};
		}
	}

}
