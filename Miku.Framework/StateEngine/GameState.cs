using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Miku.Framework.StateEngine
{
	public abstract class GameState: IGameState
	{
		public Game Game { get; }
		protected GameStateManager _manager;
		protected SpriteBatch _spriteBatch;

		public bool IsStarted { get; protected set; }

		public abstract void LoadContent();
		public abstract void UnloadContent();


		protected GameState(Game game)
		{
			Game = game;
		}

		public virtual void Start()
		{
			if (IsStarted)
				return;

			_manager = Game.Services.GetService<GameStateManager>();
			_spriteBatch = Game.Services.GetService<SpriteBatch>();
			LoadContent();

			IsStarted = true;
		}
		public virtual void Stop()
		{
			if (!IsStarted)
				return;

			UnloadContent();

			IsStarted = false;
		}



		public virtual bool UpdateFadeIn(GameTime gameTime)
		{
			return true;
		}

		public virtual bool UpdateFadeOut(GameTime gameTime)
		{
			return true;
		}

		public virtual void DrawFadeIn(GameTime gameTime)
		{
			
		}

		public virtual void DrawFadeOut(GameTime gameTime)
		{
			
		}

		public virtual void Update(GameTime gameTime)
		{
			
		}

		public virtual void Draw(GameTime gameTime)
		{

		}
		
	}
}
