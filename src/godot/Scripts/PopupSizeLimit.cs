using System;
using Godot;

namespace godotutils
{
    public partial class PopupSizeLimit: OptionButton
    {
        [Export]
        public int MaxHeight = 0;
        public override void _Ready()
        {
            var popup = this.GetPopup();
            if (popup == null)
            {
                GD.Print("no popup");
                return;
            }
            if (MaxHeight > 0)
            {
                var sz = popup.MaxSize;
                sz.Y = MaxHeight;
                popup.MaxSize = sz;
            }
        }
    }
}
