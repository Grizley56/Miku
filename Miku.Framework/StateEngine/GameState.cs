using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework.StateEngine
{
	public abstract class GameState: IGameState
	{
		public GameState(Game game)
		{
			Components = new List<GameComponent>();
			if(GameStateController == null)
				GameStateController = (GameStateController)game.Services.GetService(typeof(IGameStateController));
		}

		#region Fields
		private int order;
		private const int MAX_COMPS = 100;
		private bool visible;
		private bool enabled;
		private readonly IGameStateController GameStateController;
		#endregion

		#region Properties
		#region Indexator
		public GameComponent this[int i]
		{
			get
			{
				try
				{
					return Components?[i];
				}
				catch (IndexOutOfRangeException)
				{
					return null;
				}
			}
		}
		#endregion
		public List<GameComponent> Components { get; }
		public int Order
		{
			get { return order; }
			set
			{
				if (value < 0) throw new ArgumentOutOfRangeException("Order");
				if (value == order) return;
				foreach(GameComponent gc in Components)
				{
					gc.UpdateOrder += (value - order)*MAX_COMPS;
					if (gc is DrawableGameComponent)
						(gc as DrawableGameComponent).DrawOrder += (value - order)*MAX_COMPS;
				}
			}
		}
		public bool IsWorkOnBackground { get; protected set; }
		public bool Visible
		{
			get { return visible; }
			set
			{
				if (visible == value)
					return;
				foreach (GameComponent gameComponent in Components)
					if (gameComponent is DrawableGameComponent)
						(gameComponent as DrawableGameComponent).Visible = value;
				OnVisibleChanged(this, EventArgs.Empty);
			}
		}
		public bool Enabled
		{
			get { return enabled; }
			set
			{
				if (enabled == value)
					return;
				foreach (GameComponent gameComponent in Components)
					gameComponent.Enabled = value;
				OnEnabledChanged(this, EventArgs.Empty);
			}
		}
		#endregion

		#region Methods
		public void Pause()
		{
			foreach (GameComponent component in Components)
			{
				component.Enabled = false;
				if (component is DrawableGameComponent)
					(component as DrawableGameComponent).Visible = false;
			}
		}
		public void UnPause()
		{
			foreach(GameComponent component in Components)
			{
				component.Enabled = true;
				if (component is DrawableGameComponent)
					(component as DrawableGameComponent).Visible = true;
			}
		}
		#endregion

		#region EventsRegion
		public event EventHandler<EventArgs> OnVisibleChanged;
		public event EventHandler<EventArgs> OnEnabledChanged;
		#endregion
	}
}