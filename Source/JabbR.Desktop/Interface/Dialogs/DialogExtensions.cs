using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JabbR.Eto.Interface.Dialogs
{
    static class DialogExtensions
    {
        public static Button OkButton(this Dialog dialog, string text = null, Func<bool> clicked = null)
        {
            var control = new Button { Text = text ?? "OK" };
            dialog.DefaultButton = control;
            control.Click += (sender, e) => {
                if (clicked != null && !clicked())
                    return;
                dialog.Close(DialogResult.Ok);
            };
            return control;
        }

        public static Button CancelButton(this Dialog dialog, string text = null, Func<bool> clicked = null)
        {
            var control = new Button { Text = text ?? "Cancel" };
            dialog.AbortButton = control;
            control.Click += (sender, e) => {
                if (clicked != null && !clicked())
                    return;
                dialog.Close(DialogResult.Cancel);
            };
            return control;
        }
    }
}
