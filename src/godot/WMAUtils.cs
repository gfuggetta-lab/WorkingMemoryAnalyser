using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using WMAData;

namespace godot
{
	public static class WMAUtils
	{
		public static Godot.Color GDColor(WMAData.ColorFloat clr)
		{
			return new Color((float)clr.r, (float)clr.g, (float)clr.b);
		}
	}
}
