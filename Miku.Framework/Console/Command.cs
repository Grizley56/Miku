using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework.Console
{
	public struct Command
	{
		public string CommandName { get; }
		public string[] Args { get; }

		public Command(string commandName, string[] args = null)
		{
			CommandName = commandName;
			Args = args ?? new string[0];
		}

		public override string ToString()
		{
			string result = CommandName;
			if (Args.Length != 0)
				result += " " + String.Join(" ", Args);
			return result;
		}

		public static Command Parse(string command)
		{
			if (command == null)
				throw new ArgumentNullException(nameof(command));

			
			int argsStart = command.IndexOf(' ');
			if (argsStart == -1)
				return new Command(command);

			string commandName = command.Substring(0, argsStart);
			string args = command.Substring(argsStart);

			return new Command(commandName, args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
		}

		public static bool operator ==(Command command1, Command command2)
		{
			if (!string.Equals(command1.CommandName, command2.CommandName, StringComparison.OrdinalIgnoreCase))
				return false;
			if (command1.Args.Length != command2.Args.Length)
				return false;
			var difference = command1.Args.Except(command2.Args, StringComparer.Create(CultureInfo.CurrentCulture, false));
			return !difference.Any();
		}

		public static bool operator !=(Command command1, Command command2)
		{
			return !(command1 == command2);
		}
	}
}
