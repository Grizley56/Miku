using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Miku.Framework.StateEngine
{
	public interface IGameState
	{
		bool IsStarted { get; }
		void Start();
		void Stop();
		void Update(GameTime gameTime);
		void Draw(GameTime gameTime);
		bool UpdateFadeIn(GameTime gameTime);
		bool UpdateFadeOut(GameTime gameTime);
		void DrawFadeIn(GameTime gameTime);
		void DrawFadeOut(GameTime gameTime);
		void LoadContent();
		void UnloadContent();
	}
}
