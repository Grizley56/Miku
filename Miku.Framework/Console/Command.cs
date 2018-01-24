using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework.Console
{
	public delegate ConsoleEntry CommandFunc(string[] args);

	public class ConsoleCommand: IEquatable<ConsoleCommand>
	{
		public string CommandName { get; }
		public string CommandHelp { get; }
		public CommandFunc Function { get; }

		public ConsoleCommand(string commandName, CommandFunc function, string commandHelp = null)
		{
			CommandName = commandName;
			Function = function;
			CommandHelp = commandHelp;
		}

		public override string ToString()
		{
			return CommandName;
		}

		public bool Equals(ConsoleCommand other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(CommandName, other.CommandName, StringComparison.OrdinalIgnoreCase);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((ConsoleCommand) obj);
		}

		public override int GetHashCode()
		{
			return (CommandName != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(CommandName) : 0);
		}
	}

	public class CommandComparer : IEqualityComparer<ConsoleCommand>
	{
		public bool Equals(ConsoleCommand x, ConsoleCommand y)
		{
			return x != null && x.Equals(y);
		}

		public int GetHashCode(ConsoleCommand obj)
		{
			return obj.GetHashCode();
		}
	}
}
