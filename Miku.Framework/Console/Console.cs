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
	public class Console : DrawableGameComponent, IConsole
	{
		public ConsoleCommand Help { get; private set; }
		public ConsoleCommand Cls { get; private set; }

		private SpriteFont _font;

		private ConsoleRenderManager _renderManager;
		private ConsoleInputManager _inputManager;
		private SpriteBatch _spriteBatch;

		private bool _isOpened;

		public event EventHandler<EventArgs> ConsoleClosed;
		public event EventHandler<EventArgs> ConsoleOpened;

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

		public Color BackColor
		{
			get { return _renderManager.BackColor; }
			set
			{
				_renderManager.BackColor = value;
			}
		}

		public float BackOpacity
		{
			get
			{
				return _renderManager.BackOpacity;
			}
			set
			{
				_renderManager.BackOpacity = value;
			}
		}

		public Color InputFontColor
		{
			get { return _renderManager.InputTextColor; }
			set { _renderManager.InputTextColor = value; } 
		}

		public float InputFontOpacity
		{
			get
			{
				return _renderManager.InputFontOpacity;
			}
			set
			{
				_renderManager.InputFontOpacity = value;
			}
		}

		public bool IsOpened

		{
			get { return _isOpened; }
			set
			{
				if (_isOpened == value)
					return;

				_isOpened = value;
				_inputManager.Enabled = value;
			}
		}

		public void Toggle() => IsOpened = !IsOpened;

		public Console(Game game, SpriteFont font) : base(game)
		{
			Font = font;

			_inputManager = new ConsoleInputManager(this);
			_renderManager = new ConsoleRenderManager(_inputManager, game.GraphicsDevice) {Font = font};
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
				_inputManager.Commands.ForEach(i =>
				{
					if (i.CommandHelp != null)
						_inputManager.ConsoleHistory.Log(new ConsoleEntry($"- {i.CommandName} \"{i.CommandHelp}\"", Color.Yellow, false));
				});
				return null;
			}, "guide to commands");

			Cls = new ConsoleCommand("cls", _ =>
			{
				Clear();
				return null;
			});
		}

		public void Log(string message)
		{
			_inputManager.ConsoleHistory.Log(new ConsoleEntry(message, Color.White));
		}

		public void Log(ConsoleEntry entry)
		{
			_inputManager.ConsoleHistory.Log(entry);
		}

		public bool AddCommand(ConsoleCommand command)
		{
			if (command == null)
				throw new ArgumentNullException(nameof(command));

			if (_inputManager.Commands.Contains(command, new CommandComparer()))
				return false;

			_inputManager.Commands.Add(command);
			return true;
		}

		public override void Draw(GameTime gameTime)
		{
			if (!IsOpened)
				return;

			_renderManager.Draw(gameTime, _spriteBatch);
			base.Draw(gameTime);
		}

		public override void Update(GameTime gameTime)
		{
			if (KeyboardComponent.KeyPressed(OpenKey))
				Toggle();

			if (!IsOpened)
				return;

			_inputManager.Update(gameTime);
			_renderManager.Update(gameTime);
		}

		public void Clear()
		{
			_inputManager.ConsoleHistory.Clear();
		}
	}
}
