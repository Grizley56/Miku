using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Miku.Framework.Console;

namespace Miku.Framework.Input
{
	public delegate string ClipboardTextGetter();

	public class KeyboardTextEditor : KeyboardTextReader
	{
		public event EventHandler<TextSubmittedEventArgs> TextSubmitted;
		public event EventHandler<CursorPositionChangedEventArgs> CursorPositionChanged;
		public event EventHandler<TextAddedEventArgs> TextAdded;
		public event EventHandler<TextRemovedEventArgs> TextRemoved;

		public ClipboardTextGetter ClipboardPasting { get; set; }
		public event EventHandler<ClipboardTextEventArgs> ClipboardCutting;
		public event EventHandler<ClipboardTextEventArgs> ClipboardCopying;

		public int CursorPosition
		{
			get { return _cursorPosition; }
			private set
			{
				if (value > _buffer.Length || value < 0)
					throw new ArgumentOutOfRangeException(nameof(CursorPosition));
				if (value == _cursorPosition)
					return;

				int oldPosition = _cursorPosition;
				_cursorPosition = value;
				OnCursorPositionChanged(new CursorPositionChangedEventArgs(oldPosition, _cursorPosition));
			}
		}

		public new string Text
		{
			get { return _buffer.ToString(); }
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(Text));
				Clear();
				CursorPosition = 0;
				WriteTextToBuffer(value);
			}
		}
		public TimeSpan ControlKeysRepeatSpeed
		{
			get { return _controlKeysRepeatSpeed; }
			set
			{
				if (_controlKeysRepeatSpeed == value)
					return;

				_controlKeysRepeatSpeed = value;
				_leftArrowTracer.TimeBetweenRepeat = value;
				_rightArrowTracer.TimeBetweenRepeat = value;
			}
		}
		public bool CursorMoving => _leftArrowTracer.IsHolded || _rightArrowTracer.IsHolded;
		public void Clear() => RemoveTextFromBuffer(0, _buffer.Length);
		public Point HighlightRange
		{
			get { return _highlightRange; }
			set
			{
				if (value.X > value.Y)
					throw new ArgumentException("The beginning can not be more than the end of highlight");
				if (value.X < 0 || value.Y < 0)
					throw new ArgumentException("The beginning and the end of highlight can not be negative");

				_highlightRange = value;
			}
		}
		public bool IsTextHighlighted => HighlightRange.X != HighlightRange.Y;
		public string HighlightedText => IsTextHighlighted
			? Text.Substring(HighlightRange.X, HighlightRange.Y - HighlightRange.X)
			: String.Empty;

		private readonly KeyboardHoldingTracer _leftArrowTracer;
		private readonly KeyboardHoldingTracer _rightArrowTracer;
		private readonly KeyboardHoldingTracer _deleteTracer;

		private TimeSpan _controlKeysRepeatSpeed = TimeSpan.FromMilliseconds(35);
		private readonly char[] _sectionSplitSymbols =  {' ', ',', '.'};
		private Point _highlightRange;
		private int _cursorPosition;

		private bool IsErasingCharacter(char character) => character == '\b' || character == '\u007f' ||
		                                                   character == '\u0010' || character == '\u0000';

		public void HighlightAllText() => HighlightRange = new Point(0, _buffer.Length);
		public void ResetHighlighting() => _highlightRange = new Point(0);

		public KeyboardTextEditor(GameWindow window) : base(window)
		{
			_leftArrowTracer = new KeyboardHoldingTracer(Keys.Left, TimeSpan.FromSeconds(0.3f), ControlKeysRepeatSpeed);
			_rightArrowTracer = new KeyboardHoldingTracer(Keys.Right, TimeSpan.FromSeconds(0.3f), ControlKeysRepeatSpeed);
			_deleteTracer = new KeyboardHoldingTracer(Keys.Delete, TimeSpan.FromSeconds(0.3f), ControlKeysRepeatSpeed);
		}

		protected override void WriteControlToBuffer(char control)
		{
			if (IsErasingCharacter(control))
			{
				int startIndex = 0;
				int count = 0;
				int newCursorPosition = CursorPosition;
				if (IsTextHighlighted)
				{
					switch (control)
					{																																															////////////////////
						case '\b':																																									//		Backspace		//
						case '\u0000':																																							//			Delete		//
							startIndex = HighlightRange.X;                                                            //								//
							count = HighlightRange.Y - HighlightRange.X;                                              //								//
							newCursorPosition = HighlightRange.X;                                                     //								//
							break;																																										////////////////////
						case '\u007f':																																						  // Ctrl+Backspace //
							startIndex = GetIndexOfPreviousSection(Text, _sectionSplitSymbols, HighlightRange.X);			//								//
							count = HighlightRange.Y - startIndex;																										//								//
							newCursorPosition = startIndex;																														//								//
							break;																																										////////////////////
						case '\u0010':																																							//	 Ctrl+Delete  //
							startIndex = HighlightRange.X;																														//								//
							count = GetIndexOfNextSection(Text, _sectionSplitSymbols, HighlightRange.Y) - startIndex; //								//
							newCursorPosition = startIndex;                                                           //								//
							break;                                                                                    ////////////////////
					}
					ResetHighlighting();
				}
				else
				{
					switch (control)
					{
						case '\b':
							count = CursorPosition == 0 ? 0 : 1;
							startIndex = CursorPosition == 0 ? 0 : CursorPosition - 1;
							newCursorPosition = startIndex;
							break;
						case '\u0000':
							startIndex = CursorPosition;
							count = CursorPosition == _buffer.Length ? 0 : 1;
							break;
						case '\u007f':
							startIndex = GetIndexOfPreviousSection(Text, _sectionSplitSymbols, CursorPosition);
							count = CursorPosition - startIndex;
							newCursorPosition = startIndex;
							break;
						case '\u0010':
							startIndex = CursorPosition;
							count = GetIndexOfNextSection(Text, _sectionSplitSymbols, CursorPosition) - startIndex;
							break;
					}
				}
				//Remove the text before change the curosor position
				RemoveTextFromBuffer(startIndex, count);
				CursorPosition = newCursorPosition;
				return;
			}

			switch (control)
			{
				case '\t':
					break;																																							////////////////////
				case '\r':																																						//			Enter			//
					OnTextEntered(new TextSubmittedEventArgs(Text));																		//								//
					Clear();                                                                            //								//
																																															//								//
					CursorPosition = 0;                                                                 //								//
					break;                                                                              ////////////////////
				case '\u0001':                                                                        //		Ctrl + A		//
					HighlightAllText();                                                                 //								//
					CursorPosition = _buffer.Length;                                                    //								//
					break;                                                                              ////////////////////
				case '\u0016':																																				//		Ctrl + V		//
					if (ClipboardPasting != null)                                                       //								//
					{                                                                                   //								//
						string pastingText = ClipboardPasting.Invoke();                                   //								//
						if (IsTextHighlighted)                                                            //								//
						{                                                                                 //								//
							RemoveTextFromBuffer(HighlightRange.X, HighlightRange.Y - HighlightRange.X);    //								//
							CursorPosition = HighlightRange.X;                                              //								//
							ResetHighlighting();                                                            //								//
						}                                                                                 //								//
						WriteTextToBuffer(pastingText);                                                   //								//
					}                                                                                   //								//
					break;                                                                              ////////////////////
				case '\u0003':																																				//		Ctrl + C		//
					if (IsTextHighlighted)                                                              //								//
						OnClipboardCopying(new ClipboardTextEventArgs(HighlightedText));                  //								//
					break;                                                                              ////////////////////
				case '\u0018':																																				//		Ctrl + X		//
					if (IsTextHighlighted)                                                              //								//
					{                                                                                   //								//
						OnClipboardCutting(new ClipboardTextEventArgs(HighlightedText));                  //								//
						RemoveTextFromBuffer(HighlightRange.X, HighlightRange.Y - HighlightRange.X);      //								//
						CursorPosition = HighlightRange.X;                                                //								//
						ResetHighlighting();                                                              //								//
					}                                                                                   //								//
					break;                                                                              ////////////////////
			}
		}

		protected virtual bool RemoveTextFromBuffer(int startIndex, int count)
		{
			if (startIndex + count > _buffer.Length)
				throw new ArgumentOutOfRangeException();

			if (count == 0 || startIndex == _buffer.Length)
				return false;
			
			var removedSubstring = _buffer.Remove(startIndex, count);

			OnTextRemoved(new TextRemovedEventArgs(removedSubstring.ToString(), Text, startIndex));
			return true;
		}

		protected override void WriteTextToBuffer(char chr)
		{
			WriteTextToBuffer(chr.ToString());
		}

		protected override void WriteTextToBuffer(string str)
		{
			if (IsTextHighlighted)
			{
				CursorPosition = HighlightRange.X;
				RemoveTextFromBuffer(HighlightRange.X, HighlightRange.Y - HighlightRange.X);
				ResetHighlighting();
			}
			_buffer.Insert(CursorPosition, str);
			OnTextAdded(new TextAddedEventArgs(str, Text, CursorPosition));
			CursorPosition += str.Length;
		}

		public void SetCursorTo(InputCursorPosition position)
		{
			switch (position)
			{
				case InputCursorPosition.Begin:
					CursorPosition = 0;
					break;
				case InputCursorPosition.Center:
					CursorPosition = _buffer.Length == 0 ? 0 : _buffer.Length / 2;
					break;
				case InputCursorPosition.End:
					CursorPosition = _buffer.Length;
					break;
				default:
					throw new ArgumentException(nameof(position));
			}
			ResetHighlighting();
		}

		public override void Update(GameTime gameTime)
		{
			if (!Enabled)
				return;

			base.Update(gameTime);

			if (_leftArrowTracer.Check() && CursorPosition > 0)
			{
				int oldCursorPosition = CursorPosition;

				if (KeyboardComponent.IsKeyDown(Keys.LeftControl) || KeyboardComponent.IsKeyDown(Keys.RightControl))
					CursorPosition = GetIndexOfPreviousSection(Text, _sectionSplitSymbols, CursorPosition);
				else
					CursorPosition--;

				if (KeyboardComponent.IsKeyDown(Keys.LeftShift) || KeyboardComponent.IsKeyDown(Keys.RightShift))
				{
					if (IsTextHighlighted)
					{
						int highlightedCount = oldCursorPosition - CursorPosition;
						for (int i = 0; i < highlightedCount; i++)
							if (CursorPosition < HighlightRange.X)
								HighlightRange -= new Point(1, 0);
							else
								HighlightRange -= new Point(0, 1);
					}
					else
					{
						HighlightRange = new Point(CursorPosition, oldCursorPosition);
					}
				}
				else
					ResetHighlighting();
			}

			if (_rightArrowTracer.Check() && CursorPosition < _buffer.Length)
			{
				int oldCursorPosition = CursorPosition;

				if (KeyboardComponent.IsKeyDown(Keys.LeftControl) || KeyboardComponent.IsKeyDown(Keys.RightControl))
					CursorPosition = GetIndexOfNextSection(Text, _sectionSplitSymbols, CursorPosition);
				else
					CursorPosition++;

				if (KeyboardComponent.IsKeyDown(Keys.LeftShift) || KeyboardComponent.IsKeyDown(Keys.RightShift))
				{
					if (IsTextHighlighted)
					{
						int highlightedCount = CursorPosition - oldCursorPosition;
						for (int i = 0; i < highlightedCount; i++)
							if (HighlightRange.Y >= CursorPosition)
								HighlightRange += new Point(1, 0);
							else
								HighlightRange += new Point(0, 1);
					}
					else
					{
						HighlightRange = new Point(oldCursorPosition, CursorPosition);
					}
				}
				else
					ResetHighlighting();
			}

			if (_deleteTracer.Check())
			{
				// I send unused range of Unicode symbols
				// for reduce count of code (for example \u0010 is DLE symbol (Data link escape))
				if (KeyboardComponent.IsKeyDown(Keys.LeftControl) || KeyboardComponent.IsKeyDown(Keys.RightControl))
					WriteControlToBuffer('\u0010');
				else
					WriteControlToBuffer('\u0000');
			}

		}

		private static int GetIndexOfNextSection(string str, char[] sectionSplitChars, int startIndex)
		{
			int nextSectionIndex = str.IndexOfAny(sectionSplitChars, startIndex);

			if (nextSectionIndex == startIndex)
				return GetIndexOfNextSection(str, sectionSplitChars, ++startIndex);

			if (nextSectionIndex == -1)
				nextSectionIndex = str.Length;

			return nextSectionIndex;
		}

		private static int GetIndexOfPreviousSection(string str, char[] sectionSplitChars, int startIndex)
		{
			int nextSectionIndex = str.IndexOfAny(sectionSplitChars, startIndex, Direction.RightToLeft);

			if (nextSectionIndex == startIndex)
				return GetIndexOfPreviousSection(str, sectionSplitChars, --startIndex);

			if (nextSectionIndex == -1)
				nextSectionIndex = 0;

			return nextSectionIndex;
		}

		protected virtual void OnCursorPositionChanged(CursorPositionChangedEventArgs e)
		{
			CursorPositionChanged?.Invoke(this, e);
		}

		protected virtual void OnTextEntered(TextSubmittedEventArgs e)
		{
			TextSubmitted?.Invoke(this, e);
		}

		protected virtual void OnClipboardCutting(ClipboardTextEventArgs e)
		{
			ClipboardCutting?.Invoke(this, e);
		}

		protected virtual void OnClipboardCopying(ClipboardTextEventArgs e)
		{
			ClipboardCopying?.Invoke(this, e);
		}

		protected virtual void OnTextRemoved(TextRemovedEventArgs e)
		{
			TextRemoved?.Invoke(this, e);
		}

		protected virtual void OnTextAdded(TextAddedEventArgs e)
		{
			TextAdded?.Invoke(this, e);
		}
	}
}
