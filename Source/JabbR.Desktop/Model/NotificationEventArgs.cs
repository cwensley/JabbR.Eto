using System;
using Eto;
using System.Xml;
using Eto.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JabbR.Desktop.Model.JabbR;
using JabbR.Desktop.Interface;

namespace JabbR.Desktop.Model
{
    public class NotificationEventArgs : EventArgs
    {
        public NotificationMessage Message { get; private set; }
        
        public NotificationEventArgs(NotificationMessage message)
        {
            this.Message = message;
        }
    }

}

