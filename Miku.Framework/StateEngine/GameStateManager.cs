using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Miku.Framework.StateEngine
{
	public class GameStateManager: DrawableGameComponent
	{
		public IGameState CurrentState { get; private set; }

		private Queue<IGameState> _nextStates;

		public GameStateManager(Game game) : base(game)
		{
			_nextStates = new Queue<IGameState>();

			game.Services.AddService(this);
			game.Components.Add(this);
		}


		public void ChangeState(IGameState newState)
		{
			if (CurrentState == null)
			{
				CurrentState = newState;
				if (!CurrentState.IsStarted)
					CurrentState.Start();

				return;
			}
			_nextStates.Enqueue(newState);
		}

		public override void Initialize()
		{
			base.Initialize();
		}

		protected override void LoadContent()
		{
			base.LoadContent();
		}

		protected override void UnloadContent()
		{
			base.UnloadContent();
		}

		public override void Draw(GameTime gameTime)
		{
			if (_nextStates.Count != 0)
			{
				IGameState nextState = _nextStates.Peek();

				if (CurrentState != null)
					CurrentState.DrawFadeOut(gameTime);
				else
					nextState.DrawFadeIn(gameTime);
				return;
			}

			if (CurrentState != null)
				CurrentState.Draw(gameTime);
			else
				Game.GraphicsDevice.Clear(Color.Black);
		}

		public override void Update(GameTime gameTime)
		{
			if (_nextStates.Count != 0)
			{
				if (CurrentState != null && CurrentState.UpdateFadeOut(gameTime))
				{
					CurrentState.Stop();
					CurrentState = null;
				}

				if (CurrentState != null)
					return;

				IGameState nextState = _nextStates.Peek();
				if (!nextState.IsStarted)
					nextState.Start();

				if (!nextState.UpdateFadeIn(gameTime))
					return;

				CurrentState = _nextStates.Dequeue();
				CurrentState.Update(gameTime);
				return;
			}

			CurrentState?.Update(gameTime);
		}
	}
}
