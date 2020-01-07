using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DeprecationTool.Models;
using Lib;
using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow.Activities;
using XrmToolBox.Extensibility;

namespace DeprecationTool
{
    public partial class DeprecateControl : PluginControlBase
    {
        private ListViewItemComparer entityListViewComparer;
        private FormState formState;
        private Settings pluginSettings;
        private Deprecate.SolutionData[] solutions;
        private IDictionary<string, IDictionary<string, Deprecate.MetaData[]>> solutionsWithData;

        public DeprecateControl()
        {
            InitializeComponent();

            this.entityListViewComparer = new ListViewItemComparer();
            this.entityFieldList.ListViewItemSorter = this.entityListViewComparer;
        }

        private void DeprecateControl_Load(object sender, EventArgs e)
        {
            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out pluginSettings))
            {
                pluginSettings = new Settings();

                LogWarning("Settings not found => a new settings file has been created!");
            }
            else
            {
                LogInfo("Settings found and loaded");
            }

            if (Service == null) solutionComboBox.Text = "No organization loaded.";

            FirstTimeSettingsPrompt();

            formState = new FormState();
        }

        private void SettingsPrompt()
        {
            Settings res = null;

            while (res == null || res.DeprecationPrefix == string.Empty)
                res = DeprecationTool.SettingsPrompt.SettingsDialog("Your field prefix (not required)",
                    "Your deprecation prefix (required)",
                    "Deprecation settings",
                    pluginSettings);

            if (res == pluginSettings) return;

            pluginSettings = res;
            SettingsManager.Instance.Save(GetType(), pluginSettings);
        }

        private void FirstTimeSettingsPrompt()
        {
            if (string.IsNullOrEmpty(pluginSettings.DeprecationPrefix))
            {
                ShowInfoNotification("To read about what this tool is designed for, visit the readme!", 
                    new Uri("https://github.com/delegateas/DeprecationTool/blob/master/README.md"));

                pluginSettings.FieldPrefix = "";
                pluginSettings.DeprecationPrefix = "zz_";
            }

        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        private void reload_click(object sender, EventArgs e)
        {
            ClearSolutionComboBox();
            ClearFieldList();
            ClearEntityList();
            ExecuteMethod(LoadData);
        }

        private void tsSettings_Click(object sender, EventArgs e)
        {
            SettingsPrompt();
        }

        private WorkAsyncInfo FetchEntities()
        {
            return new WorkAsyncInfo
            {
                Message = "Fetching entities and attributes",
                Work = (worker, args) =>
                {
                    args.Result = Deprecate.retrieveSolutionEntities(Service, solutions,
                        pluginSettings.FieldPrefix,
                        pluginSettings.DeprecationPrefix);
                },
                PostWorkCallBack = args =>
                {
                    if (args.Error != null)
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    if (!(args.Result is IDictionary<string, IDictionary<string, Deprecate.MetaData[]>> result)) return;

                    solutionsWithData = result;
                    PopulateSolutionsComboBox();
                }
            };
        }

        private WorkAsyncInfo FetchSolutionsAndEntities()
        {
            return new WorkAsyncInfo
            {
                Message = "Fetching solutions",
                Work = (worker, args) =>
                {
                    args.Result = Deprecate.retrieveSolutionNames(Service, new[] {"Default"}, "");
                },
                PostWorkCallBack = args =>
                {
                    if (args.Error != null)
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    if (!(args.Result is Deprecate.SolutionData[] result)) return;

                    solutions = result;
                    WorkAsync(FetchEntities());
                }
            };
        }

        private void LoadData()
        {
            WorkAsync(FetchSolutionsAndEntities());
        }

        private void PopulateSolutionsComboBox()
        {
            ClearSolutionComboBox();
            foreach (var sol in solutions)
            {
                var logicalName = sol.uniqueName;
                solutionsWithData.TryGetValue(logicalName, out var entities);
                var entityCountText = $"[{entities?.Count.ToString() ?? "n/a"}] {logicalName} ";

                solutionComboBox.Items.Add(new Deprecate.DisplayValue(entityCountText, logicalName));
            }

            solutionComboBox.Text = "Select solution here.";

        }

        private void PopulateEntitiesListView(string solLogicalName)
        {
            ClearEntityList();
            if (!solutionsWithData.TryGetValue(solLogicalName, out var res)) return;

            foreach (var item in res.Keys)
                entityList.Items.Add(new ListViewItem(new[] {item}));
        }

        private void PopulateFieldListView(Deprecate.MetaData[] fields)
        {
            ClearFieldList();
            FieldButtonEnabled(true);

            foreach (var field in fields)
            {
                var names = field.ColumnNames();
                var itemToAdd = new ListViewItem
                {
                    Text = names.logicalName,
                    Tag = field,
                    ImageIndex = (int) field.deprecationState,
                    ImageKey = Deprecate.DeprecationStateToCheckBoxLiteral(field.deprecationState)
                };
                // First column is given by Text field, subsequent ones are given by subitems
                itemToAdd.SubItems.AddRange(new[] { names.displayName });

                entityFieldList.Items.Add(itemToAdd);
            }

            foreach (ColumnHeader column in entityFieldList.Columns)
                column.Width = -1;

        }

        private void ClearSolutionComboBox()
        {
            solutionComboBox.Text = string.Empty;
            solutionComboBox.SelectedText = string.Empty;
            solutionComboBox.Items.Clear();
        }

        private void ClearEntityList()
        {
            entityList.Items.Clear();
        }

        private void ClearFieldList()
        {
            FieldButtonEnabled(false);
            entityFieldList.Items.Clear();
        }

        private void FieldButtonEnabled(bool isOn)
        {
            resetButton.Enabled = isOn;
            fixPartialButton.Enabled = isOn;
            applyButton.Enabled = isOn;
        }

        /// <summary>
        ///     This event occurs when the plugin is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeprecateControl_OnCloseTool(object sender, EventArgs e)
        {
            // Before leaving, save the settings
            SettingsManager.Instance.Save(GetType(), pluginSettings);
        }

        /// <summary>
        ///     This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail,
            string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (newService != null) LoadData();
        }

        private bool DiscardChangesMessage()
        {
            return MessageBox.Show(
                       "You have pending changes, do you wish to discard them?",
                       "You have changed deprecation states that are not saved, do you wish to discard?",
                       MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        private void solutionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var solutionList = (ToolStripComboBox) sender;
            var currentlySelected = (Deprecate.DisplayValue) solutionList.SelectedItem;
            var currentIndex = solutionList.SelectedIndex;

            if (currentlySelected == null) return;
            //if (currentIndex == formState.CurrentSolutionIdx) return;

            if (Deprecate.hasPendingChanges(FieldsWithCheckedState()))
                if (!DiscardChangesMessage() && formState.CurrentSolutionIdx != -1)
                {
                    solutionList.SelectedIndex = formState.CurrentSolutionIdx;
                    return;
                }

            formState.CurrentSolutionIdx = currentIndex;
            formState.SelectedSolution = currentlySelected.Value;
            var logicalName = currentlySelected.Value;
            PopulateEntitiesListView(logicalName);
            ClearFieldList();
        }

        private void entityListView_SelectedIndexChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected || e.Item == formState.CurrentEntityListItem) return;

            var currentlySelected = e.Item;
            if (currentlySelected == null) return;


            if (Deprecate.hasPendingChanges(FieldsWithCheckedState()))
                if (!DiscardChangesMessage() && formState.CurrentEntityListItem != null)
                {
                    currentlySelected.Selected = false;
                    formState.CurrentEntityListItem.Selected = true;
                    return;
                }

            formState.CurrentEntityListItem = currentlySelected;

            var selectedEntityFields = GetEntityFields(currentlySelected);
            PopulateFieldListView(selectedEntityFields);
        }

        private Deprecate.MetaData[] GetEntityFields(ListViewItem currentlySelected)
        {
            if (!solutionsWithData.TryGetValue(formState.SelectedSolution, out var selectedSolution)) return null;
            if (!selectedSolution.TryGetValue(currentlySelected.SubItems[0].Text, out var selectedEntityFields))
                return null;
            return selectedEntityFields;
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < entityFieldList.Items.Count; i++)
            {
                var field = entityFieldList.Items[i];
                field.ImageIndex = (int) ((Deprecate.MetaData)field.Tag).deprecationState;
            }
        }

        private void fixPartialButton_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < entityFieldList.Items.Count; i++)
            {
                var field = entityFieldList.Items.Cast<ListViewItem>().ElementAt(i);
                var metadata = ((Deprecate.MetaData)field.Tag);
                if (metadata.deprecationState == Deprecate.DeprecationState.Partial)
                    field.ImageIndex = (int)CheckState.Checked;
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            var attrWithCheckedState = FieldsWithCheckedState();

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Deprecating fields",
                Work = (worker, args) =>
                {
                    args.Result = Deprecate.decideAndExecuteOperations(Service, attrWithCheckedState,
                        pluginSettings.DeprecationPrefix);
                },
                PostWorkCallBack = args =>
                {
                    if (args.Error != null)
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        private Deprecate.MetaDataWithCheck[] FieldsWithCheckedState()
        {
            var fieldList = entityFieldList;
            var attrWithCheckedState = entityFieldList.Items.Cast<ListViewItem>()
                .Select((item, i) =>
                    new Deprecate.MetaDataWithCheck(
                        (Deprecate.MetaData) item.Tag,
                        (Deprecate.DeprecationState) item.ImageIndex)
                )
                .ToArray();
            return attrWithCheckedState;
        }

        private void tsInfo_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                    "In depth explanation can be found in our readme.\nDo you wish to open the webpage?", "Visit readme?", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk
                ) == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start("https://github.com/delegateas/DeprecationTool/blob/master/README.md");
            }
        }

        private void fieldListColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView myListView = (ListView)sender;
            entityListViewComparer.toggleOrder(e.Column);
            myListView.Sort();
        }

        private void fieldListOnkeyboardPress(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space)
            {
                var items = entityFieldList.SelectedItems.Cast<ListViewItem>();
                for (var i = 0; i < items.Count(); i++)
                {
                    var item = items.ElementAt(i);
                    // Take current index with modulo of 2, subtract that with 1.
                    // 1 turns into 0
                    // 2 turns into 1
                    // 3 turns into 0
                    // Repeated modulo because % on negative numbers are negative otherwise.
                    item.ImageIndex = 1 - ((item.ImageIndex % 2 + 2) % 2);
                }

                e.Handled = e.SuppressKeyPress = true;
            }
        }

    }

    class ListViewItemComparer : IComparer
    {
        // Start with false so we sync up with the beginning of the winform column state
        public bool[] ascending { get; set; } = {true, true};
        public int col { get; set; } = 0;

        public ListViewItemComparer() { }

        public void toggleOrder(int givenCol) {
            if(ascending.Length > givenCol)
            {
                col = givenCol;
                ascending[col] = !ascending[col];
            }
        }

        public int Compare(object x, object y)
        {
            return ascending[col]
                ? string.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text)
                : string.Compare(((ListViewItem)y).SubItems[col].Text, ((ListViewItem)x).SubItems[col].Text);
        }
    }
}