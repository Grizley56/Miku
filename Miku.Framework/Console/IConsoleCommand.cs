using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework.Console
{
	public interface IConsoleCommand: IAutoCompleteable
	{
		string CommandName { get; }
		string CommandHelp { get; }
		CommandFunc Function { get; }
	}
}
