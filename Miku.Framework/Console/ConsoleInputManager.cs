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
		private LinkedList<Command> _commandHistory  { get; } = new LinkedList<Command>();
		private LinkedListNode<Command> _selectedCommandFromHistory;
		

		internal KeyboardTextEditor TextEditor;
		internal Dictionary<string, ConsoleCommand> Commands { get; } = new Dictionary<string, ConsoleCommand>();

		public List<string> ConsoleHistory { get; } = new List<string>();
		public string CommandNotFoundMessage { get; set; } = "Unknow command";
		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				if (_enabled == value)
					return;

				if (value)
					TextEditor.IgnoreInput(1); // TODO: ignore console open key only

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

			TextEditor = new KeyboardTextEditor(_console.Game.Window, i => _console.Font.GetGlyphs().ContainsKey(i));
			TextEditor.TextEntered += CommandEntered;
		}

		public void Update(GameTime gameTime)
		{
			if (!Enabled)
				return;


			if (KeyboardComponent.KeyPressed(Keys.Down))
			{
				if (_selectedCommandFromHistory == null)
					_selectedCommandFromHistory = _commandHistory.Last;
				else if (_selectedCommandFromHistory != _commandHistory.First)
					_selectedCommandFromHistory = _selectedCommandFromHistory.Previous;

				TextEditor.Text = _selectedCommandFromHistory.Value.ToString();
				TextEditor.SetCursorTo(InputCursorPosition.End);
			}

			if (KeyboardComponent.KeyPressed(Keys.Up))
			{
				if (_selectedCommandFromHistory == null)
					_selectedCommandFromHistory = _commandHistory.First;
				else if (_selectedCommandFromHistory != _commandHistory.Last)
					_selectedCommandFromHistory = _selectedCommandFromHistory.Next;

				TextEditor.Text = _selectedCommandFromHistory.Value.ToString();
				TextEditor.SetCursorTo(InputCursorPosition.End);
			}

			TextEditor.Update(gameTime);
		}
		
		private void CommandEntered(object sender, TextEnteredEventArgs e)
		{
			Command command = Command.Parse(e.EnteredText);

			//if(_commandHistory.First != null && command != _commandHistory.First.Value)
			_commandHistory.AddFirst(command);

			_selectedCommandFromHistory = null;

			if (IsValideCommand(command.CommandName))
			{
				string result = Commands[command.CommandName]?.Invoke(command.Args);
				ConsoleHistory.Add(e.EnteredText);
				if (result != null)
					ConsoleHistory.Add("- " + result);
			}
			else
				ConsoleHistory.Add(GenerateNotFoundMessage(command.CommandName));
		}
		private string GenerateNotFoundMessage(string command) =>  $"{CommandNotFoundMessage} \"{command}\"";
		private bool IsValideCommand(string command) => Commands.ContainsKey(command);
	}
}