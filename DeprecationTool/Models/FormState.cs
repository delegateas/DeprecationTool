using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lib;

namespace DeprecationTool.Models
{
    class FormState
    { 

        public IEnumerable<Deprecate.MetaData> SelectedAttributes { get; set; }
        public string SelectedSolution { get; set; } = string.Empty;
        public int CurrentSolutionIdx { get; set; } = -1;
        public ListViewItem CurrentEntity { get; set; } = null;

        public bool HasPendingChanges => SelectedAttributes.Any();

        public FormState()
        {
            SelectedAttributes = new List<Deprecate.MetaData>();
        }

        public void ClearChanges()
        {
            SelectedAttributes = null;
        }

    }
}
