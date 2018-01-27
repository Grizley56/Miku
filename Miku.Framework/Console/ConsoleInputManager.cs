using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Miku.Framework.Input;

namespace Miku.Framework.Console
{
	internal sealed class ConsoleInputManager
	{
		private readonly Console _console;
		private bool _enabled;

		private readonly LinkedList<CommandInfo> _commandHistory = new LinkedList<CommandInfo>();
		private LinkedListNode<CommandInfo> _selectedCommandFromHistory;
		
		internal KeyboardTextEditor TextEditor;
		internal List<ConsoleCommand> Commands = new List<ConsoleCommand>();

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

		public ConsoleInputManager(Console console)
		{
			_console = console;

			_console.ConsoleClosed += (_, __) => Enabled = false;
			_console.ConsoleOpened += (_, __) => Enabled = true;

			TextEditor = new KeyboardTextEditor(_console.Game.Window);
			TextEditor.TextEntered += CommandEntered;
		}

		public void Update(GameTime gameTime)
		{
			if (!Enabled)
				return;

			if (KeyboardComponent.KeyPressed(Keys.Down))
			{
				CommandInfo nextCommand = GetNextHistoryItem();
				if (nextCommand != null)
				{
					TextEditor.Text = _selectedCommandFromHistory.Value.ToString();
					TextEditor.SetCursorTo(InputCursorPosition.End);
				}
			}

			if (KeyboardComponent.KeyPressed(Keys.Up))
			{
				CommandInfo previousCommand = GetPreviousHistoryItem();
				if (previousCommand != null)
				{
					TextEditor.Text = previousCommand.ToString();
					TextEditor.SetCursorTo(InputCursorPosition.End);
				}
			}

			TextEditor.Update(gameTime);
		}

		public CommandInfo GetPreviousHistoryItem()
		{
			if (_commandHistory.Count == 0)
				return null;

			if (_selectedCommandFromHistory == null)
				_selectedCommandFromHistory = _commandHistory.First;
			else if (_selectedCommandFromHistory != _commandHistory.Last)
				_selectedCommandFromHistory = _selectedCommandFromHistory.Next;

			return _selectedCommandFromHistory?.Value;
		}

		public CommandInfo GetNextHistoryItem()
		{
			if (_selectedCommandFromHistory == null)
				return null;

			if (_selectedCommandFromHistory == null)
				_selectedCommandFromHistory = _commandHistory.Last;
			else if (_selectedCommandFromHistory != _commandHistory.First)
				_selectedCommandFromHistory = _selectedCommandFromHistory.Previous;

			return _selectedCommandFromHistory?.Value;
		}

		private void CommandEntered(object sender, TextEnteredEventArgs e)
		{
			CommandInfo commandInfo = CommandInfo.FromString(e.EnteredText);

			if(_commandHistory.First == null)
				_commandHistory.AddFirst(commandInfo);
			else if(!_commandHistory.First.Value.Equals(commandInfo))
				_commandHistory.AddFirst(commandInfo);

			_selectedCommandFromHistory = null;

			var command = Commands.Find(i => i.CommandName == commandInfo.CommandName);

			if (command != null)
			{
				ConsoleEntry result = command.Function?.Invoke(commandInfo.Args);
				ConsoleHistory.Log(new ConsoleEntry(e.EnteredText, Color.White));
				if (result != null)
					ConsoleHistory.Log(new ConsoleEntry("- " + result.Data, result.TextColor, false));
			}
			else
				ConsoleHistory.Log(new ConsoleEntry(GenerateNotFoundMessage(commandInfo.CommandName), Color.Red));
		}
		private string GenerateNotFoundMessage(string command) =>  $"{CommandNotFoundMessage} \"{command}\"";
	}
}