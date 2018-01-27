using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Miku.Framework.Drawing;

namespace Miku.Framework.Console
{
	internal class ConsoleHistoryRenderer
	{
		public SpriteFont Font { get; set; }
		public ConsoleHistory Histroy { get; }

		public int ScrollBarWidth { get; set; }
		public Point ScrollBarPadding { get; set; } = new Point(0);
		public Color ScrollBarColor { get; set; } = Color.White;
		public Color ScrollStripColor { get; set; } = Color.White;
		public bool ScrollBarVisible { get; set; }

		//TODO: property for choose DateTime format (entry time)

		public float ScrollDelta
		{
			get { return _scrollDelta; }
			set { _scrollDelta = value < 0 ? 0 : value; }
		}

		private float _scrollDelta;

		public ConsoleHistoryRenderer(ConsoleHistory history, SpriteFont font, bool showScrolling = true)
		{
			Histroy = history;
			Font = font;
			ScrollBarVisible = showScrolling;

			history.HistoryCleared += (_, __) => ScrollDelta = 0;
			history.EntryAdded += (_, __) => ScrollDelta = 0;
		}

		private ConsoleEntry[] GetForBounds(SpriteFont font, Rectangle bounds)
		{
			List<ConsoleEntry> result = new List<ConsoleEntry>(Histroy.Entries.Count);

			foreach (var entry in Histroy.Entries)
			{
				string time = String.Empty;
				if (entry.TimeVisible)
					time = $"[{entry.Time:T}] ";
				string[] multiLineCommand = BreakStringToLines(time + entry.Data, font, bounds).Split('\n');

				//Save first with command-time
				result.Add(new ConsoleEntry(multiLineCommand[0], entry.TextColor));

				for (int i = 1; i < multiLineCommand.Length; i++)
					result.Add(new ConsoleEntry(multiLineCommand[i], entry.TextColor, false));
			}

			return result.ToArray();
		}

		public void Render(GameTime gameTime, SpriteBatch batch, Rectangle bounds)
		{
			if (bounds == Rectangle.Empty)
				throw new ArgumentException(nameof(bounds));
			
			batch.Begin();

			//One output entry can be bigger than aviable width-space, so split it to a few lines
			ConsoleEntry[] dividedToLines = GetForBounds(Font, new Rectangle(bounds.Location, 
				new Point(bounds.Size.X - ScrollBarWidth - ScrollBarPadding.X - 5, bounds.Size.Y - ScrollBarPadding.Y)));

			int linesToDraw = Math.Min(bounds.Height / Font.LineSpacing, dividedToLines.Length);

			float linesScrolled = ScrollDelta / Font.LineSpacing;

			int totalLines = dividedToLines.Length;

			int totalLinesSkipped = (int)linesScrolled + linesToDraw;

			if (totalLinesSkipped >= totalLines)
			{
				linesScrolled = totalLines - linesToDraw;
				ScrollDelta = Font.LineSpacing * linesScrolled;
			}

			int lastTextIndex = dividedToLines.Length - 1 - (int) linesScrolled;
			int firstTextIndex = lastTextIndex - linesToDraw + 1;

			int drawAfterBounds = lastTextIndex == dividedToLines.Length - 1 ? 0 : 1;
			int drawBeforeBounds = firstTextIndex == 0 ? 0 : 1;
			
			float precentOfLineScrolled = linesScrolled - (int)linesScrolled;
			float scrolledPartInPixels = precentOfLineScrolled * Font.LineSpacing;

			Vector2 offset = new Vector2(bounds.X,
				bounds.Y + scrolledPartInPixels - (drawBeforeBounds == 1 ? Font.LineSpacing : 0));

			Regex timePattern = new Regex(@"\[\d+:\d+:\d+\]");

			for (int i = firstTextIndex - drawBeforeBounds; i <= lastTextIndex + drawAfterBounds; i++)
			{
				ConsoleEntry entry = dividedToLines[i];
				string text = entry.Data;

				if (entry.TimeVisible)
				{
					Match match = timePattern.Match(text);
					Vector2 size = Font.MeasureString(match.Value);
					batch.DrawString(Font, match.Value, offset, entry.TimeColor);
					batch.DrawString(Font, text.Substring(match.Value.Length), new Vector2(offset.X + size.X, offset.Y),
						entry.TextColor);
				}
				else
					batch.DrawString(Font, text, offset, entry.TextColor);

				offset.Y += Font.LineSpacing;
			}

			if (ScrollBarVisible)
			{
				Rectangle scrollBarBounds = new Rectangle(bounds.Size.X - ScrollBarWidth - ScrollBarPadding.X, ScrollBarPadding.Y,
					ScrollBarWidth, bounds.Height - ScrollBarPadding.Y * 2);

				batch.DrawRect(scrollBarBounds, ScrollBarColor);

				int stripHeight = (int)(linesToDraw / (float)totalLines * scrollBarBounds.Height);

				//not enought just compare equal, because possible loss of precision
				if (Math.Abs(stripHeight - scrollBarBounds.Height) > 0.01f)
				{
					int stripOffset = (int)(scrollBarBounds.Height / (float)totalLines * linesScrolled);
					Rectangle stripBounds = new Rectangle(scrollBarBounds.X,
						scrollBarBounds.Height - stripHeight - stripOffset + ScrollBarPadding.Y,
						scrollBarBounds.Width, stripHeight);

					batch.DrawRect(stripBounds, ScrollStripColor);
				}
			}


			batch.End();
		}

		private static string BreakStringToLines(string text, SpriteFont font, Rectangle bounds)
		{
			Vector2 fullSize = font.MeasureString(text);
			if (fullSize.X <= bounds.Width)
				return text;

			var splittedByWords = text.Split(' ');
			float maxWidth = bounds.Width;

			StringBuilder buffer = new StringBuilder();

			int wordsInBounds = 0;

			if (font.MeasureString(splittedByWords[0]).X > maxWidth)
			{
				string word = splittedByWords[wordsInBounds];

				if (font.MeasureString(word[0].ToString()).X > maxWidth)
					throw new Exception("One character is bigger than bounds");

				int charsInBounds = 0;

				do
				{
					buffer.Append(word[charsInBounds++]);
				} while (font.MeasureString(word.Substring(0, charsInBounds + 1)).X < maxWidth);

				buffer.Append('\n');

				return buffer + BreakStringToLines(word.Substring(charsInBounds, word.Length - charsInBounds)
				                                   + string.Join(" ", splittedByWords, 1, splittedByWords.Length - 1), font, bounds);
			}

			do
			{
				buffer.Append(splittedByWords[wordsInBounds++] + ' ');
			} while (font.MeasureString(string.Join(" ", splittedByWords, 0, wordsInBounds + 1)).X < maxWidth);

			buffer.Append('\n');

			return buffer + BreakStringToLines(string.Join(" ", splittedByWords, wordsInBounds,
				       splittedByWords.Length - wordsInBounds), font, bounds);
		}
	}
}
