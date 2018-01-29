using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework.Console
{
	public interface IAutoCompleteService<T, in U> where T : IAutoCompleteable
	{
		IEnumerable<T> Storage { get; set; }
		T[] this[U elem] { get; }
	}
}
