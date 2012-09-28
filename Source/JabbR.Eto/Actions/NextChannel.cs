using System;
using Eto.Forms;
using JabbR.Eto.Interface;
using Eto;

namespace JabbR.Eto.Actions
{
	public class NextChannel : ButtonAction
	{
		public const string ActionID = "NextChannel";
		
		public Channels Channels { get; private set; }
		
		public NextChannel (Channels channels)
		{
			this.Channels = channels;
			this.ID = ActionID;
			this.MenuText = "Next Channel";
			if (channels.Generator.ID == Generators.Mac)
				this.Accelerator = Application.Instance.CommonModifier | Key.Shift | Key.Down;
			else
				this.Accelerator = Key.Alt | Key.Shift | Key.Down;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			Channels.GoToNextChannel (false);
		}
	}
}

