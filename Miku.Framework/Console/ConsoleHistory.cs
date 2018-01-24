using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Miku.Framework.Console
{
	internal class ConsoleHistory
	{
		public List<ConsoleEntry> Entries { get; private set; } = new List<ConsoleEntry>();
		public Color TimeColor { get; set; } = Color.Red;
		public bool ShowTime { get; set; } = true;

		public event EventHandler<EventArgs> HistoryCleared;
		public event EventHandler<ConsoleEntryAddedEventArgs> EntryAdded;

		public ConsoleHistory() { }

		public void Log(ConsoleEntry entry)
		{
			Entries.Add(entry);
			OnEntryAdded(new ConsoleEntryAddedEventArgs(entry));
		}


		public void Clear()
		{
			Entries.Clear();
			OnHistoryCleared();
		}

		protected virtual void OnHistoryCleared()
		{
			HistoryCleared?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnEntryAdded(ConsoleEntryAddedEventArgs e)
		{
			EntryAdded?.Invoke(this, e);
		}
	}
}
