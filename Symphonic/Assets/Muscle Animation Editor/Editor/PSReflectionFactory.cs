using UnityEngine;
using System.Collections;

namespace PavoStudio.MAE
{
	class PSReflectionFactory
	{
		public static PSBaseReflection GetReflection ()
		{
			return new PSReflection ();
		}
	}
}
