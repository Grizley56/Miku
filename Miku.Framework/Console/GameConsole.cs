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
		public static GameConsole Create(Game game, SpriteFont font)
		{
			if (Instance != null)
				throw new InvalidOperationException("Instance already created");
			Instance = new GameConsole(game, font);
			return Instance;
		}

		public static GameConsole Instance { get; private set; }

		public ConsoleCommand Help { get; private set; }
		public ConsoleCommand Cls { get; private set; }

		internal ConsoleRenderManager RenderManager;
		internal ConsoleInputManager InputManager;

		private SpriteBatch _spriteBatch;
		private bool _isOpened;
		private SpriteFont _font;
		private ConsoleSkin _skin;

		public event EventHandler<EventArgs> Closed;
		public event EventHandler<EventArgs> Opened;

		public KeyboardTextEditor TextEditor => InputManager.TextEditor;

		public SpriteFont Font
		{
			get
			{
				return _font;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("Font");

				_font = value;
			}
		}

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
			get { return _skin; }
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(Skin));

				_skin = value; 
			}
		}

		protected GameConsole(Game game, SpriteFont font) : base(game)
		{
			Font = font;
			Skin = ConsoleSkins.Light;

			InputManager = new ConsoleInputManager(this);
			RenderManager = new ConsoleRenderManager(this, game.GraphicsDevice) {Font = font};
			_spriteBatch = Game.Services.GetService<SpriteBatch>();

			DrawOrder = int.MaxValue - 1; // TODO: change it 
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
				Toggle();

			if (!IsOpened)
				return;

			InputManager.Update(gameTime);
			RenderManager.Update(gameTime);
		}

		public void Clear()
		{
			InputManager.ConsoleHistory.Clear();
		}
	}
}
