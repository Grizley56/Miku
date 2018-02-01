using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Design;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Miku.Framework.Input;

namespace Miku.Framework.Console
{
	public class GameConsole : DrawableGameComponent, IConsole
	{
		public static GameConsole Create(Game game, SpriteFont font, ConsoleSkin skin = null)
		{
			if (Instance != null)
				throw new InvalidOperationException("Instance already created");

			Instance = new GameConsole(game, font, skin ?? ConsoleSkins.Dark);
			return Instance;
		}
		public static GameConsole Instance { get; private set; }

		public ConsoleCommand Help { get; private set; }
		public ConsoleCommand Cls { get; private set; }

		internal ConsoleRenderManager RenderManager;
		internal ConsoleInputManager InputManager;

		private SpriteBatch _spriteBatch;
		private bool _isOpened;

		public event EventHandler<EventArgs> Closed;
		public event EventHandler<EventArgs> Opened;

		public KeyboardTextEditor TextEditor => InputManager.TextEditor;
		
		public Keys OpenKey { get; set; } = Keys.OemTilde;

		public bool IsOpened
		{
			get { return _isOpened; }
			set
			{
				if (_isOpened == value)
					return;

				_isOpened = value;
				InputManager.Enabled = value;
				if (value)
					OnOpened();
				else
					OnClosed();
			}
		}

		public void Toggle() => IsOpened = !IsOpened;

		public IAutoCompleteService<IConsoleCommand, string> AutoCompleteService
		{
			get { return InputManager.AutoCompleteService; }
			set { InputManager.AutoCompleteService = value; }
		}

		public ConsoleSkin Skin
		{
			get { return RenderManager.Skin; }
			set { RenderManager.Skin = value; }
		}

		public SpriteFont Font
		{
			get { return RenderManager.Font; }
			set { RenderManager.Font = value; }
		}

		protected GameConsole(Game game, SpriteFont font, ConsoleSkin skin)
			:base(game)
		{
			InputManager = new ConsoleInputManager(this);
			RenderManager = new ConsoleRenderManager(this, font, skin);

			_spriteBatch = Game.Services.GetService<SpriteBatch>();

			DrawOrder = int.MaxValue - 1; // TODO: find the better way
			Game.Services.AddService(typeof(IConsole), this);
			Game.Components.Add(this);

			LoadDefaultCommands();
		}

		private void LoadDefaultCommands()
		{
			Help = new ConsoleCommand("help", _ =>
			{
				InputManager.Commands.ForEach(i =>
				{
					if (i.CommandHelp != null)
						InputManager.ConsoleHistory.Log(new ConsoleEntry($"- {i.CommandName} \"{i.CommandHelp}\"", Color.Yellow, false));
				});
				return null;
			}, true, "guide to commands");

			Cls = new ConsoleCommand("cls", _ =>
			{
				Clear();
				return null;
			});
		}

		public void Log(string message)
		{
			InputManager.ConsoleHistory.Log(new ConsoleEntry(message));
		}

		public void Log(ConsoleEntry entry)
		{
			InputManager.ConsoleHistory.Log(entry);
		}

		public bool AddCommand(ConsoleCommand command)
		{
			if (command == null)
				throw new ArgumentNullException(nameof(command));

			if (InputManager.Commands.Contains(command, new CommandEqualityComparer()))
				return false;

			InputManager.Commands.Add(command);
			return true;
		}

		public override void Draw(GameTime gameTime)
		{
			if (!IsOpened)
				return;

			RenderManager.Draw(gameTime, _spriteBatch);
			base.Draw(gameTime);
		}

		public override void Update(GameTime gameTime)
		{
			if (KeyboardComponent.KeyPressed(OpenKey))
			{
				Toggle();
			}

			if (!IsOpened)
				return;

			InputManager.Update(gameTime);
			RenderManager.Update(gameTime);
		}

		public void Clear()
		{
			InputManager.ConsoleHistory.Clear();
		}

		protected virtual void OnClosed()
		{
			Closed?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnOpened()
		{
			Opened?.Invoke(this, EventArgs.Empty);
		}
	}
}
