using System;

namespace JabbR.Eto.Model
{
	public class MeMessageEventArgs : EventArgs
	{
		public MeMessage Message { get; private set; }
		
		public MeMessageEventArgs (MeMessage message)
		{
			this.Message = message;
		}
	}
}

