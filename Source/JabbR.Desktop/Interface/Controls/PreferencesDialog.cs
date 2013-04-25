using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto;
using Eto.Drawing;

namespace JabbR.Desktop.Interface.Controls
{
    public abstract class PreferenceItem
    {
        public string Title { get; set; }
        
        public Image Icon { get; set; }
        
        public abstract void Generate(PreferencesDialog dialog, Container container);
    }
    
    public class PreferenceGroup : PreferenceItem
    {
        public List<PreferenceItem> Children { get; private set; }
        
        public PreferenceGroup()
        {
            Children = new List<PreferenceItem>();
        }
        
        public PreferenceGroup(IEnumerable<PreferenceItem> items)
            : this ()
        {
            Children.AddRange(items);
        }
        
        public override void Generate(PreferencesDialog dialog, Container container)
        {
            var handler = dialog.Handler as IPreferencesDialog;
            handler.GenerateGroup(this, container);
        }
    }
    
    public class PreferenceSection : PreferenceItem
    {
        public Control Control { get; protected set; }
        
        public PreferenceSection(Control control)
        {
            this.Control = control;
        }
        
        public override void Generate(PreferencesDialog dialog, Container container)
        {
            var handler = dialog.Handler as IPreferencesDialog;
            handler.GenerateSection(this, container);
        }
    }
    
    public interface IPreferencesDialog : IDialog
    {
        void GenerateGroup(PreferenceGroup group, Container container);
        
        void GenerateSection(PreferenceSection section, Container container);
        
        void AppendSection(PreferenceItem item);
    }
    
    public class PreferencesDialog : Dialog
    {
        List<PreferenceItem> items;
        
        protected new IPreferencesDialog Handler { get { return (IPreferencesDialog)base.Handler; } }
        
        public IEnumerable<PreferenceItem> Items
        {
            get { return items; }
            set
            {
                if (value != null)
                    items = new List<PreferenceItem>(value);
                else
                    items = null;
            }
        }
        
        public PreferencesDialog(IEnumerable<PreferenceItem> items = null)
            : this (Generator.Current, items)
        {
        }
        
        public PreferencesDialog(Generator generator, IEnumerable<PreferenceItem> items = null)
            : base(generator, typeof(IPreferencesDialog), false)
        {
            this.Items = new List<PreferenceItem>(items);
            Initialize();
        }
        
        public override void OnLoad(EventArgs e)
        {
            foreach (var item in items)
            {
                Handler.AppendSection(item);
            }
            base.OnLoad(e);
        }
    }
}

