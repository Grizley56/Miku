using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Design;
using Microsoft.Xna.Framework.Graphics;
using Miku;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework.StateEngine
{
	public sealed class GameStateController : IGameStateController
	{
		#region Fields
		private Stack<GameState> states;
		private Game GameRef;
		private int Order;
		#endregion

		#region Properties
		
		#endregion

		public GameStateController(Game game)
		{
			GameRef = game;
			game.Services.AddService(typeof(IGameStateController), this);
			states = new Stack<GameState>();
		}

		public GameState Peek()
		{
			return states.Peek();
		}

		public GameState Pop()
		{
			GameState gameState = states.Pop();
			RemoveComponents(gameState);
			if (!Peek().IsWorkOnBackground)
				Peek().UnPause();
			Order--;
			return gameState;
		}

		public void Push(GameState gameState)
		{
			if (states.Count > 0 && !states.Peek().IsWorkOnBackground)
				states.Peek().Pause();
			
			gameState.Order = Order;
			states.Push(gameState);
			AddComponents(gameState);
			Order++;
		}

		public event EventHandler<EventArgs> OnStateChanged;

		#region ServiceMethods ADD/REMOVE
		private void AddComponents(GameState gameState)
		{
			foreach(GameComponent gc in gameState.Components)
			{
				GameRef.Components.Add(gc);
			}
		}
		private void RemoveComponents(GameState gameState)
		{
			foreach(GameComponent gc in gameState.Components)
			{
				GameRef.Components.Remove(gc);
			}
		}
		#endregion

	}
}
