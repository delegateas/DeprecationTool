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
        public string SelectedSolution { get; set; } = string.Empty;
        public int CurrentSolutionIdx { get; set; } = -1;
        public ListViewItem CurrentEntityListItem { get; set; } = null;
    }
}
