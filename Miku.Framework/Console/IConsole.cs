using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework.Console
{
	public interface IConsole
	{
		void Toggle();
		bool AddCommand(string commandName, ConsoleCommand commandFunc);
	}
}
