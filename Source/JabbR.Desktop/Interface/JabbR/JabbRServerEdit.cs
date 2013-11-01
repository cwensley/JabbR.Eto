using System;
using Eto.Forms;
using JabbR.Desktop.Model;
using JabbR.Desktop.Model.JabbR;
using Eto;
using Eto.Drawing;
using Microsoft.AspNet.SignalR.Client;

namespace JabbR.Desktop.Interface.JabbR
{
    public class JabbRServerEdit
    {
        JabbRServer server;
        DockContainer authSection;
        DockContainer loginSection;
        DockContainer socialSection;
        CheckBox useSocialLogin;
        Label statusLabel;
        TextBox janrainAppName;
        TextBox serverAddress;
        Button authButton;

        public JabbRServerEdit(JabbRServer server, DynamicLayout layout)
        {
            this.server = server;
            layout.AddRow(new Label { Text = "Address" }, EditAddress());
            layout.EndBeginVertical();
            layout.AddRow(UseSocialLogin());
            layout.Add(authSection = new Panel { MinimumSize = new Size(0, 100) });
            layout.EndBeginVertical();
            LoginSection();
            SocialSection();
            
            authSection.DataContextChanged += (sender, e) => {
                SetVisibility();
            };
        }
        
        void SetVisibility()
        {
            var useSocial = useSocialLogin.Checked ?? false;
            if (useSocial)
            {
                authSection.Content = socialSection;
                var authenticated = !string.IsNullOrEmpty(server.UserId);
                if (authenticated)
                {
                    authButton.Text = "Re-authenticate";
                    statusLabel.Text = "Authenticated";
                    statusLabel.TextColor = Colors.Green;
                }
                else
                {
                    authButton.Text = "Authenticate";
                    statusLabel.Text = "Not Authenticated";
                    statusLabel.TextColor = Colors.Red;
                }
                    
            }
            else
                authSection.Content = loginSection;
        }
        
        Control LoginSection()
        {
            var layout = new DynamicLayout();
            
            layout.Add(null);
            layout.BeginVertical();
            layout.AddRow(new Label { Text = "User Name" }, EditUserName());
            layout.AddRow(new Label { Text = "Password" }, EditPassword());
            layout.EndVertical();
            layout.AddSeparateRow(null, CreateAccountButton(), null);
            layout.Add(null);

            return new GroupBox{ Text = "Login", Content = layout };
        }

        Control CreateAccountButton()
        {
            var control = new Label { Text = "Create a New Account", TextColor = Colors.Blue };
            control.MouseDown += (sender, e) => {
                var uri = new UriBuilder(this.serverAddress.Text) {
                    Path = "account/register"
                };

                Application.Instance.Open(uri.ToString());
            };
            return control;
        }

        Control SocialSection()
        {
            var layout = new DynamicLayout();

            layout.Add(null);
            layout.AddSeparateRow(new Label { Text = "App Name" }, JanrainAppName());
            layout.Add(null);
            layout.BeginVertical();
            layout.AddRow(null, StatusLabel(), null);
            layout.AddRow(null, AuthButton(), null);
            layout.EndVertical();
            layout.Add(null);

            return new GroupBox{ Text = "Janrain", Content = layout };
        }
        
        Control JanrainAppName()
        {
            var control = janrainAppName = new TextBox();
            control.TextBinding.Bind <JabbRServer>(c => c.JanrainAppName, (c,v) => c.JanrainAppName = v, mode: DualBindingMode.OneWay);
            return control;
        }
        
        Control StatusLabel()
        {
            var control = statusLabel = new Label {
                HorizontalAlign = HorizontalAlign.Center
            };
            return control;
        }
            
        Control AuthButton()
        {
            var control = authButton = new Button { Text = "Authenticate" };
            control.Click += delegate
            {
                var dlg = new JabbRAuthDialog(serverAddress.Text, janrainAppName.Text);
                dlg.DisplayMode = DialogDisplayMode.Attached;
                var result = dlg.ShowDialog(control);
                if (result == DialogResult.Ok)
                {
                    server.UserId = dlg.UserID;
                    SetVisibility();
                }
            };
            return control;
        }
        
        Control UseSocialLogin()
        {
            var control = useSocialLogin = new CheckBox { Text = "Use Social Login", Visible = false };
            control.CheckedBinding.Bind <JabbRServer>(c => c.UseSocialLogin, (c,v) => c.UseSocialLogin = v ?? false, mode: DualBindingMode.OneWay);
            control.CheckedChanged += delegate
            {
                SetVisibility();
            };
            return control;
            
        }
        
        Control EditAddress()
        {
            var control = serverAddress = new TextBox();
            control.TextBinding.Bind<JabbRServer>(c => c.Address, (c,v) => c.Address = v, mode: DualBindingMode.OneWay);
            return control;
        }
        
        Control EditUserName()
        {
            var control = new TextBox();
            control.TextBinding.Bind<JabbRServer>(c => c.UserName, (c,v) => c.UserName = v, mode: DualBindingMode.OneWay);
            return control;
        }
        
        Control EditPassword()
        {
            var control = new PasswordBox();
            control.TextBinding.Bind<JabbRServer>(c => c.Password, (c,v) => c.Password = v, mode: DualBindingMode.OneWay);
            return control;
        }
    }
}

