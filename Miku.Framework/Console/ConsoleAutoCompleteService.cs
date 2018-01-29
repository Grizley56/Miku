using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework.Console
{
	public class ConsoleAutoCompleteService: IAutoCompleteService<IConsoleCommand, string>
	{
		private int _maxCount;
		public IEnumerable<IConsoleCommand> Storage { get; set; }

		public int MaxCount
		{
			get { return _maxCount; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException(nameof(MaxCount));

				_maxCount = value;
			}
		}

		public IConsoleCommand[] this[string elem]
		{
			get
			{
				if (elem == String.Empty || MaxCount == 0)
					return new IConsoleCommand[0];

				var result = Storage.Where(i => i.AutoCompleteEnabled && i.CommandName.StartsWith(elem)).ToList();
				result.Sort(new CommandByNameComparer());
				return result.Count > MaxCount ? result.GetRange(0, MaxCount).ToArray() : result.ToArray();
			}
		}

		public ConsoleAutoCompleteService(IEnumerable<IConsoleCommand> storage, int maxCount = 3)
		{
			Storage = storage;
			MaxCount = maxCount;
		}
		
	}
}
