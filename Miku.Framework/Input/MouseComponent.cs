using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Miku.Framework.Input
{
	public class MouseComponent: GameComponent
	{
		private static int _previousScrollValue;
		private static int _currentScrollValue;

		private static MouseState _previousState;
		private static MouseState _currentState;

		public MouseComponent(Game game) : base(game)
		{
			_previousScrollValue = _currentScrollValue = 0;
			_previousState = _currentState = Mouse.GetState();

			game.Components.Add(this);
		}


		public override void Update(GameTime gameTime)
		{
			_currentState = Mouse.GetState();
			_currentScrollValue = _currentState.ScrollWheelValue - _previousState.ScrollWheelValue;


			_previousScrollValue = _currentScrollValue;
			_previousState = _currentState;
		}

		public static int ScrolledInBounds(Rectangle rect)
		{
			return rect.Contains(Mouse.GetState().Position) ? _currentScrollValue : 0;
		}

		public static int Scrolled()
		{
			return _currentScrollValue;
		}

	}
}
