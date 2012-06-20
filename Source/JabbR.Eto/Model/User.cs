using System;
namespace JabbR.Eto.Model
{
	public class User
	{
		public string Id { get; set; }
		
		public string Name { get; set; }

		public bool IsAfk { get; set; }
		
		public bool Active { get; set; }
		
		public bool Owner { get; set; }
	}

}

