using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace Miku.Framework.Input
{
	public class KeyboardHoldingTracer
	{
		private readonly Keys _key;
		private TimeSpan _requireElapsedTime;
		private TimeSpan _timeBetweenRepeat;
		private TimeSpan _tempTime;

		public TimeSpan TimeBetweenRepeat
		{
			get
			{
				return _timeBetweenRepeat;
			}

			set
			{
				if (_timeBetweenRepeat == value)
					return;

				_timeBetweenRepeat = value;
				_tempTime = TimeSpan.Zero;
			}
		}
		public TimeSpan RequireElapsedTime
		{
			get
			{
				return _requireElapsedTime;
			}
			set
			{
				_requireElapsedTime = value;
			}
		}
		public bool IsHolded => KeyboardComponent.KeyHolded(_key, RequireElapsedTime);

		internal KeyboardHoldingTracer(Keys key, TimeSpan requireElapsedTime, TimeSpan timeBetweenRepeat)
		{
			_key = key;
			_requireElapsedTime = requireElapsedTime;
			TimeBetweenRepeat = timeBetweenRepeat;
			_tempTime = TimeBetweenRepeat;
		}

		public bool Check()
		{
			if (KeyboardComponent.KeyReleased(_key))
				_tempTime = TimeBetweenRepeat;

			if (KeyboardComponent.KeyPressed(_key))
				return true;

			if (KeyboardComponent.KeyHolded(_key, RequireElapsedTime) &&
			    KeyboardComponent.TotalTimeKeyPressed[_key] >= _tempTime)
			{
				// _tempTime == TimeBetweenRepeat its like 
				// KeyHoldedFirstTriggering possibly variable
				// If we do not do it we notice undesirable triggering
				// (RequireElapsedTime.Miliseconds / TimeBetweenRepeat.Miliseconds) times 
				if (_tempTime == TimeBetweenRepeat)
					_tempTime += RequireElapsedTime;

				_tempTime += TimeBetweenRepeat;
				return true;
			}

			return false;
		}

	}
}
