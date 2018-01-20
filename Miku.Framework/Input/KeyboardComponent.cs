using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework.Input
{
	public sealed class KeyboardComponent : GameComponent
	{
		public static bool CapsLock => Keyboard.GetState().CapsLock;
		public static bool NumLock => Keyboard.GetState().NumLock;
		public static bool IsKeyDown(Keys key) => Keyboard.GetState().IsKeyDown(key);
		public static bool IsKeyUp(Keys key) => Keyboard.GetState().IsKeyUp(key);

		public static KeyboardComponent Instance { get; private set; }
		public static Dictionary<Keys, TimeSpan> TotalTimeKeyPressed { get; } = new Dictionary<Keys, TimeSpan>(); // TODO readonly
		public static bool Loaded { get; set; }

		private static KeyboardState _previousState, _currentState;

		private KeyboardComponent(Game game) : base(game) { }

		public static void Load(Game game)
		{
			if (Loaded)
				throw new InvalidOperationException("Keyboard component already loaded");

			Instance = new KeyboardComponent(game);
			game.Components.Add(Instance);
			Loaded = true;
			game.Components.ComponentRemoved += CheckForForceRemove;
		}

		private static void CheckForForceRemove(object sender, GameComponentCollectionEventArgs e)
		{
			if (e.GameComponent is KeyboardComponent && Loaded)
			{
        Loaded = false;
				((GameComponent) e.GameComponent).Game.Components.ComponentRemoved -= CheckForForceRemove;
			}
		}

		/// <summary>
		/// Use this method if u want remove the Keyboard component from Game.Components 
		/// </summary>
		public static void Unload()
		{
			if (!Loaded)
				return;

			Loaded = false;
			Instance.Game.Components.ComponentRemoved -= CheckForForceRemove;
			Instance.Game.Components.Remove(Instance);
			Instance = null;
		}


		public override void Update(GameTime gameTime)
		{
			_previousState = _currentState;
			_currentState = Keyboard.GetState();

			foreach (Keys key in _previousState.GetPressedKeys())
			{
				if (_currentState.IsKeyDown(key))
					TotalTimeKeyPressed[key] += gameTime.ElapsedGameTime;
				else
					TotalTimeKeyPressed[key] = TimeSpan.Zero;
			}

		}

		public override void Initialize()
		{
			foreach (Keys key in Enum.GetValues(typeof(Keys)))
				TotalTimeKeyPressed.Add(key, TimeSpan.Zero);

			_previousState = Keyboard.GetState();
			_currentState = Keyboard.GetState();
		}

		/// <param name="key">Pressed key</param>
		/// <param name="afterRelease">Want know only when key already released</param>
		public static bool KeyPressed(Keys key, bool afterRelease = false)
		{
			if (!Loaded)
				return false; //throw new InvalidOperationException("You should load KeyboardComponent before");
			return !_previousState.IsKeyDown(key) ^ afterRelease && _currentState.IsKeyDown(key) ^ afterRelease;
		}

		public static bool KeyReleased(Keys key)
		{
			if (!Loaded)
				return false;
			return _previousState.IsKeyDown(key) && _currentState.IsKeyUp(key);
		}

		public static bool KeyHolded(Keys key, TimeSpan requireElapsedTime)
		{
			if (!Loaded)
				return false; //throw new InvalidOperationException("You should load KeyboardComponent before");
			return TotalTimeKeyPressed[key] >= requireElapsedTime;
		}

		public static KeyboardHoldingTracer TraceHardHolding(Keys key, TimeSpan requireElapsedTime, TimeSpan timeBeforeRepeat)
		{
			return new KeyboardHoldingTracer(key, requireElapsedTime, timeBeforeRepeat);
		}

	}
}
