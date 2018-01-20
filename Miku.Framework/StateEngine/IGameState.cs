using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework.StateEngine
{
	public interface IGameState
	{
		bool Enabled { get; set; }
		bool Visible { get; set; }
		bool IsWorkOnBackground { get; }
		GameComponent this[int index] { get; }
		int Order { get; set; }
		List<GameComponent> Components { get; }
		void Pause();
		void UnPause();
		event EventHandler<EventArgs> OnVisibleChanged;
		event EventHandler<EventArgs> OnEnabledChanged;
	}
}
