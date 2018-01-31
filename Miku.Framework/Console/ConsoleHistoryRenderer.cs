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
		private float _scrollDelta;
		private readonly ConsoleRenderManager _renderManager;

		public ConsoleHistory History => _renderManager.InputTarget.ConsoleHistory;
		public bool ScrollBarVisible { get; set; }
		public float ScrollDelta
		{
			get { return _scrollDelta; }
			set { _scrollDelta = value < 0 ? 0 : value; }
		}

		public ConsoleHistoryRenderer(ConsoleRenderManager renderManager)
		{
			if (renderManager == null)
				throw new ArgumentNullException(nameof(renderManager));
			_renderManager = renderManager;

			History.HistoryCleared += (_, __) => ScrollDelta = 0;
			History.EntryAdded += (_, __) => ScrollDelta = 0;

			ScrollBarVisible = true;
		}

		private ConsoleEntry[] GetForBounds(SpriteFont font, Rectangle bounds)
		{
			List<ConsoleEntry> result = new List<ConsoleEntry>(History.Entries.Count);

			foreach (var entry in History.Entries)
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
				return;
			
			batch.Begin();

			int scrollBarWidth = _renderManager.Skin.ScrollBarWidth;
			Point scrollBarPadding = _renderManager.Skin.ScrollBarPadding;
			SpriteFont font = _renderManager.Font;

			//One output entry can be bigger than aviable width-space, so split it to a few lines
			ConsoleEntry[] dividedToLines = GetForBounds(font, new Rectangle(bounds.Location, 
				new Point(bounds.Size.X - scrollBarWidth - scrollBarPadding.X - 5, 
				bounds.Size.Y - scrollBarPadding.Y)));

			int linesToDraw = Math.Min(bounds.Height / font.LineSpacing, dividedToLines.Length);

			float linesScrolled = ScrollDelta / font.LineSpacing;

			int totalLines = dividedToLines.Length;

			int totalLinesSkipped = (int)linesScrolled + linesToDraw;

			if (totalLinesSkipped >= totalLines)
			{
				linesScrolled = totalLines - linesToDraw;
				ScrollDelta = font.LineSpacing * linesScrolled;
			}

			int lastTextIndex = dividedToLines.Length - 1 - (int) linesScrolled;
			int firstTextIndex = lastTextIndex - linesToDraw + 1;

			int drawAfterBounds = lastTextIndex == dividedToLines.Length - 1 ? 0 : 1;
			int drawBeforeBounds = firstTextIndex == 0 ? 0 : 1;
			
			float precentOfLineScrolled = linesScrolled - (int)linesScrolled;
			float scrolledPartInPixels = precentOfLineScrolled * font.LineSpacing;

			Vector2 offset = new Vector2(bounds.X,
				bounds.Y + scrolledPartInPixels - (drawBeforeBounds == 1 ? font.LineSpacing : 0));

			Regex timePattern = new Regex(@"\[\d+:\d+:\d+\]");

			for (int i = firstTextIndex - drawBeforeBounds; i <= lastTextIndex + drawAfterBounds; i++)
			{
				ConsoleEntry entry = dividedToLines[i];
				string text = entry.Data;

				if (entry.TimeVisible)
				{
					Match match = timePattern.Match(text);
					Vector2 size = font.MeasureString(match.Value);
					batch.DrawString(font, match.Value, offset, _renderManager.Skin.HistoryTimeColor);
					batch.DrawString(font, text.Substring(match.Value.Length), new Vector2(offset.X + size.X, offset.Y),
						entry.TextColor ?? _renderManager.Skin.HistoryField.TextColor);
				}
				else
					batch.DrawString(font, text, offset, entry.TextColor ?? _renderManager.Skin.HistoryField.TextColor);

				offset.Y += font.LineSpacing;
			}

			if (ScrollBarVisible)
			{
				Rectangle scrollBarBounds = new Rectangle(bounds.Size.X - scrollBarWidth - scrollBarPadding.X, scrollBarPadding.Y,
					scrollBarWidth, bounds.Height - scrollBarPadding.Y * 2);

				batch.DrawRect(scrollBarBounds, _renderManager.Skin.ScrollBarColor);

				int stripHeight = (int)(linesToDraw / (float)totalLines * scrollBarBounds.Height);

				//not enought just compare equal, because possible loss of precision
				if (Math.Abs(stripHeight - scrollBarBounds.Height) > 0.01f)
				{
					int stripOffset = (int)(scrollBarBounds.Height / (float)totalLines * linesScrolled);
					Rectangle stripBounds = new Rectangle(scrollBarBounds.X,
						scrollBarBounds.Height - stripHeight - stripOffset + scrollBarPadding.Y,
						scrollBarBounds.Width, stripHeight);

					batch.DrawRect(stripBounds, _renderManager.Skin.ScrollBarStripColor);
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
