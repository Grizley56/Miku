using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Miku.Framework.Input;

namespace Miku.Framework.Console
{
	internal sealed class ConsoleInputManager
	{
		private readonly GameConsole _console;
		private bool _enabled;
		private string _tempInput;

		private readonly LinkedList<CommandInfo> _commandHistory = new LinkedList<CommandInfo>();
		private LinkedListNode<CommandInfo> _selectedCommandFromHistory;
		
		internal KeyboardTextEditor TextEditor;
		internal List<ConsoleCommand> Commands = new List<ConsoleCommand>();
		internal IAutoCompleteService<IConsoleCommand, string> AutoCompleteService;

		public ConsoleHistory ConsoleHistory { get; } = new ConsoleHistory();
		public string CommandNotFoundMessage { get; set; } = "Unknown command";

		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				if (_enabled == value)
					return;

				if (value)
					TextEditor.IgnoreInput(1); // TODO: ignore console open key only (safely)

				_enabled = TextEditor.Enabled = value;
			}
		}
		public string CurrentInput => TextEditor.Text;
		public int CursorPosition => TextEditor.CursorPosition;

		public IConsoleCommand[] AutoCompleteVariation { get; private set; } = new IConsoleCommand[0];
		public int AutoCompleteSelectedIndex { get; private set; }

		public ConsoleInputManager(GameConsole console)
		{
			_console = console;

			_console.Closed += (_, __) => Enabled = false;
			_console.Opened += (_, __) => Enabled = true;

			TextEditor = new KeyboardTextEditor(_console.Game.Window);
			TextEditor.TextSubmitted += CommandEntered;
			TextEditor.CharHandling += TextEditorTabHandling;

			TextEditor.TextAdded += (_, __) => UpdateAutoComplete();
			TextEditor.TextRemoved += (_, __) => UpdateAutoComplete();

			AutoCompleteService = new ConsoleAutoCompleteService(Commands);
		}

		private void TextEditorTabHandling(object sender, CharHandlingEventArgs e)
		{
			if (e.Char != '\t')
				return;

			TextEditor.Text = AutoCompleteVariation[AutoCompleteSelectedIndex].CommandName;
			e.IgnoreHandle = true;
		}

		public void Update(GameTime gameTime)
		{
			if (!Enabled)
				return;

			if (KeyboardComponent.KeyPressed(Keys.Down))
			{
				if (!MoveToNextHistoryCommand())
				{
					AutoCompleteSelectedIndex = Math.Min(++AutoCompleteSelectedIndex, AutoCompleteVariation.Length - 1);
				}
			}

			if (KeyboardComponent.KeyPressed(Keys.Up))
			{
				if (AutoCompleteSelectedIndex > 0)
					AutoCompleteSelectedIndex = Math.Max(--AutoCompleteSelectedIndex, 0);
				else
					MoveToPreviousHistoryCommand();
			}

			TextEditor.Update(gameTime);
		}

		public bool MoveToNextHistoryCommand()
		{
			if (_selectedCommandFromHistory?.Previous == null)
			{
				if (_tempInput == null)
					return false;

				_selectedCommandFromHistory = null;
				TextEditor.Text = _tempInput;
				_tempInput = null;
				return true;
			}

			_selectedCommandFromHistory = _selectedCommandFromHistory.Previous;
			TextEditor.Text = _selectedCommandFromHistory.Value.CommandName;
			return true;
		}

		public bool MoveToPreviousHistoryCommand()
		{
			if (_commandHistory.Count == 0)
				return false;

			if (_selectedCommandFromHistory == null)
			{
				_tempInput = CurrentInput;
				_selectedCommandFromHistory = _commandHistory.First;
			}
			else if (_selectedCommandFromHistory != _commandHistory.Last)
				_selectedCommandFromHistory = _selectedCommandFromHistory.Next;

			// ReSharper disable once PossibleNullReferenceException
			TextEditor.Text = _selectedCommandFromHistory.Value.CommandName;

			return true;
		}
		
		private void CommandEntered(object sender, TextSubmittedEventArgs e)
		{
			CommandInfo commandInfo = CommandInfo.FromString(e.ResultText);

			if(_commandHistory.First == null)
				_commandHistory.AddFirst(commandInfo);
			else if(!_commandHistory.First.Value.Equals(commandInfo))
				_commandHistory.AddFirst(commandInfo);

			_selectedCommandFromHistory = null;

			var command = Commands.Find(i => i.CommandName == commandInfo.CommandName);

			if (command != null)
			{
				ConsoleEntry result = command.Function?.Invoke(commandInfo.Args);
				ConsoleHistory.Log(new ConsoleEntry(e.ResultText));
				if (result != null)
					ConsoleHistory.Log(new ConsoleEntry("- " + result.Data, result.TextColor, false));
			}
			else
				ConsoleHistory.Log(new ConsoleEntry(GenerateNotFoundMessage(commandInfo.CommandName), GameConsole.Instance.Skin.HistoryWarningColor));

			UpdateAutoComplete();
		}
		private string GenerateNotFoundMessage(string command) =>  $"{CommandNotFoundMessage} \"{command}\"";

		private void UpdateAutoComplete()
		{
			if (AutoCompleteService == null)
				return;

			AutoCompleteVariation = AutoCompleteService[CurrentInput];
			if (AutoCompleteVariation.Length > 0)
				AutoCompleteSelectedIndex = 0;
		}
	}
}