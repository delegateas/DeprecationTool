using Lib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeprecationTool.CustomControls
{
    public class CheckBoxListView : System.Windows.Forms.ListView
    {
        public enum CheckState {
            Unchecked = 0,
            Checked = 1,
            Indeterminate = 2
        }

        ImageList CheckStates = new ImageList();

        public CheckBoxListView()
        {
           
            Bitmap indeterminate = new Bitmap(16, 16);
            CheckBoxRenderer.DrawCheckBox(Graphics.FromImage(indeterminate), new Point(0, 3), System.Windows.Forms.VisualStyles.CheckBoxState.MixedNormal);

            Bitmap uncheckedstate = new Bitmap(16, 16);
            CheckBoxRenderer.DrawCheckBox(Graphics.FromImage(uncheckedstate), new Point(0, 3), System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);

            Bitmap checkedstate = new Bitmap(16, 16);
            CheckBoxRenderer.DrawCheckBox(Graphics.FromImage(checkedstate), new Point(0, 3), System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal);

            CheckStates.Images.Add(Deprecate.UNCHECKED, uncheckedstate);
            CheckStates.Images.Add(Deprecate.CHECKED, checkedstate);
            CheckStates.Images.Add(Deprecate.INDETERMINATE, indeterminate);

            SmallImageList = CheckStates;
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            //if (e.Button == MouseButtons.Left)
               /* SelectedItems[0].ImageKey = SelectedItems[0].ImageKey == Deprecate.CHECKED
                    ? Deprecate.UNCHECKED
                    : Deprecate.CHECKED;*/
        }
    }

}
