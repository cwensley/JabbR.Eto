using System;
using MonoMac.AppKit;
using MonoMac.Foundation;
using Eto.Platform.Mac.Forms.Controls;

namespace JabbR.Mac
{
    public class CustomTreeViewDelegate : TreeViewHandler.EtoOutlineDelegate
    {
        public bool AllowGroupSelection { get; set; }
        
        public override bool IsGroupItem(NSOutlineView outlineView, NSObject item)
        {
            return item != null && outlineView.LevelForItem(item) == 0;
        }
        
        public override void WillDisplayCell(NSOutlineView outlineView, NSObject cell, NSTableColumn tableColumn, NSObject item)
        {
            var textCell = cell as MacImageListItemCell;
            if (textCell != null)
            {
                textCell.UseTextShadow = true;
                textCell.SetGroupItem(this.IsGroupItem(outlineView, item), outlineView, NSFont.SmallSystemFontSize, NSFont.SmallSystemFontSize);
            }
        }
        
        public override float GetRowHeight(NSOutlineView outlineView, NSObject item)
        {
            return 18;
        }
        
        public override bool ShouldSelectItem(NSOutlineView outlineView, NSObject item)
        {
            return AllowGroupSelection || !IsGroupItem(outlineView, item);
        }
    }
}

