using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework.Console
{
	class CommandInfo: IEquatable<CommandInfo>
	{
		public string CommandName { get; }
		public string[] Args { get; }
		public DateTime Time { get; }

		private readonly string _sourceText;

		private CommandInfo(string command, DateTime time, string sourceText, string[] args = null)
		{
			_sourceText = sourceText;
			CommandName = command;
			Time = time;
			Args = args ?? new string[0];
		}



		public override string ToString() => _sourceText;

		public static CommandInfo FromString(string text)
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));

			int argsStart = text.IndexOf(' ');
			if (argsStart == -1)
				return new CommandInfo(text, DateTime.Now, text);

			string commandName = text.Substring(0, argsStart);
			string args = text.Substring(argsStart);

			return new CommandInfo(commandName, DateTime.Now, text, args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
		}

		public bool Equals(CommandInfo other)
		{
			if (ReferenceEquals(this, other))
				return true;
			if (ReferenceEquals(null, other))
				return false;
			if (!string.Equals(CommandName, other.CommandName))
				return false;
			if (Args.Length != other.Args.Length)
				return false;
			if (Args.Intersect(other.Args).Count() != Args.Length)
				return false;
			return true;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((CommandInfo) obj);
		}

		public override int GetHashCode()
		{
			return (CommandName != null ? CommandName.GetHashCode() : 0);
		}
	}
}
