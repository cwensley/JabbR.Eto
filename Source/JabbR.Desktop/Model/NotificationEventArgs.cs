using System;
using Eto;
using System.Xml;
using Eto.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JabbR.Eto.Model.JabbR;
using JabbR.Eto.Interface;

namespace JabbR.Eto.Model
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

