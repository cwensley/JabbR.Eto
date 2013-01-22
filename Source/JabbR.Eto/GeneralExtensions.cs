using System;
using Eto;

namespace JabbR.Eto
{
	public static class GeneralExtensions
	{
		public static bool IsMac (this Generator generator)
		{
			return generator.ID == Generators.Mac || generator.ID == Generators.XamMac;
		}
	}
}

