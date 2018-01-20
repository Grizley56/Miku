using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework.StateEngine
{
  public interface IGameStateController
	{
		event EventHandler<EventArgs> OnStateChanged;
		void Push(GameState gameState);
		GameState Pop();
		GameState Peek();
	}
}
