using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeprecationTool.Models;
using Lib;
using XrmToolBox.Extensibility;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk.Messages;

namespace DeprecationTool
{
    public partial class DeprecateControl : PluginControlBase
    {
        private Settings pluginSettings;
        private FormState formState;
        private IDictionary<string, IDictionary<string, Deprecate.MetaData[]>> solutionsWithData;
        private Deprecate.SolutionData[] solutions;

        public DeprecateControl()
        {
            InitializeComponent();
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

            firstTimeSettingsPrompt();

            formState = new FormState();
            LoadData();
        }

        private void settingsPrompt()
        {
            Settings res = null;
            
            while(res == null || res.DeprecationPrefix == string.Empty)
                res = SettingsPrompt.SettingsDialog("Your field prefix (not required)",
                    "Your deprecation prefix",
                    "Deprecation settings",
                    pluginSettings);

            if (res == pluginSettings) return;

            pluginSettings = res;
            SettingsManager.Instance.Save(GetType(), pluginSettings);

        }
        private void firstTimeSettingsPrompt()
        {
            if (string.IsNullOrEmpty(pluginSettings.DeprecationPrefix))
            {
                settingsPrompt();
            }

        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        private void reload_click(object sender, EventArgs e)
        {
            clearSolutionComboBox();
            clearAttributeList();
            clearEntityList();
            LoadData();
        }
        private void tsSettings_Click(object sender, EventArgs e)
        {
            settingsPrompt();
        }

        private WorkAsyncInfo fetchEntities()
        {
            return new WorkAsyncInfo
            {
                Message = "Fetching entities and attributes",
                Work = (worker, args) =>
                {
                    args.Result = Lib.Deprecate.retrieveSolutionEntities(Service, solutions, 
                        pluginSettings.FieldPrefix, 
                        pluginSettings.DeprecationPrefix);
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    if (args.Result is IDictionary<string, IDictionary<string, Deprecate.MetaData[]>> result)
                    {
                        solutionsWithData = result;
                        populateSolutionsComboBox();
                    }
                }
            };
        }

        private WorkAsyncInfo fetchSolutionsAndEntities()
        {
            return new WorkAsyncInfo
            {
                Message = "Fetching solutions",
                Work = (worker, args) =>
                {
                    args.Result = Lib.Deprecate.retrieveSolutionNames(Service, new string[] {"Default"}, "");
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    if (args.Result is Deprecate.SolutionData[] result)
                    {
                        solutions = result;
                        WorkAsync(fetchEntities());
                    }
                }
            };
        }

        private void LoadData()
        {
            WorkAsync(fetchSolutionsAndEntities());            
        }

        private void populateSolutionsComboBox()
        {
            clearSolutionComboBox();
            foreach(var sol in solutions)
            {
                var logicalName = sol.uniqueName;
                solutionsWithData.TryGetValue(logicalName, out var entities);
                var entityCountText = $"[{entities?.Count.ToString() ?? "n/a"}] {logicalName} ";

                solutionComboBox.Items.Add(new Deprecate.DisplayValue(entityCountText, logicalName));
            }

        }

        private void populateEntitiesListView(string solLogicalName)
        {
            clearEntityList();
            if (!solutionsWithData.TryGetValue(solLogicalName, out var res)) return;

            foreach (var item in res.Keys)
                entityList.Items.Add(new ListViewItem(new string[] { item }));
        }
        private void clearSolutionComboBox()
        {
            solutionComboBox.Text = string.Empty;
            solutionComboBox.SelectedText = string.Empty;
            solutionComboBox.Items.Clear();
        }

        private void clearEntityList()
        {
            entityList.Items.Clear();
        }

        private void clearAttributeList()
        {
            entityFieldList.Items.Clear();
        }

        /// <summary>
        /// This event occurs when the plugin is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeprecateControl_OnCloseTool(object sender, EventArgs e)
        {
            // Before leaving, save the settings
            SettingsManager.Instance.Save(GetType(), pluginSettings);
        }

        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (newService != null)
            {
                LoadData();
            }
        }

        private void renderAttributeView(Deprecate.MetaData[] fields)
        {
            clearAttributeList();

            foreach (var field in fields)
            {
                CheckState state = (CheckState) field.deprecationState;
                entityFieldList.Items.Add(field, state);
            }

        }

        private bool discardChangesMessage()
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
            if (currentIndex == formState.CurrentSolutionIdx) return;

            if (Deprecate.hasPendingChanges(fieldsWithCheckedState()))
            {
                if (!discardChangesMessage() && formState.CurrentSolutionIdx != -1)
                {
                    solutionList.SelectedIndex = formState.CurrentSolutionIdx;
                    return;
                } 

            }

            formState.CurrentSolutionIdx = currentIndex;
            formState.SelectedSolution = currentlySelected.Value;
            var logicalName = currentlySelected.Value;
            populateEntitiesListView(logicalName);
            clearAttributeList();

        }

        private void entityListView_SelectedIndexChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected || e.Item == formState.CurrentEntityListItem) return;

            var currentlySelected = e.Item;
            if (currentlySelected == null) return;


            if (Deprecate.hasPendingChanges(fieldsWithCheckedState()))
            {
                if (!discardChangesMessage() && formState.CurrentEntityListItem != null)
                {
                    currentlySelected.Selected = false;
                    formState.CurrentEntityListItem.Selected = true;
                    return;
                } 

            }

            formState.CurrentEntityListItem = currentlySelected;

            var selectedEntityFields = getEntityFields(currentlySelected);
            renderAttributeView(selectedEntityFields);
        }

        private Deprecate.MetaData[] getEntityFields(ListViewItem currentlySelected)
        {
            if (!solutionsWithData.TryGetValue(formState.SelectedSolution, out var selectedSolution)) return null;
            if (!selectedSolution.TryGetValue(currentlySelected.SubItems[0].Text, out var selectedEntityFields)) return null;
            return selectedEntityFields;
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < entityFieldList.Items.Count; i++)
            {
                var field = entityFieldList.Items.Cast<Deprecate.MetaData>().ElementAt(i);

                entityFieldList.SetItemCheckState(i, (CheckState) field.deprecationState);
            }
        }
        private void fixPartialButton_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < entityFieldList.Items.Count; i++)
            {
                var field = entityFieldList.Items.Cast<Deprecate.MetaData>().ElementAt(i);
                if(field.deprecationState == Deprecate.DeprecationState.Partial)
                    entityFieldList.SetItemCheckState(i, CheckState.Checked);
            }
            
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            var attrWithCheckedState = fieldsWithCheckedState();

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Deprecating fields",
                Work = (worker, args) =>
                {
                    args.Result = Deprecate.decideAndExecuteOperations(Service, attrWithCheckedState, pluginSettings.DeprecationPrefix);
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    if (args.Result is ExecuteMultipleResponse[] result)
                    {
                        //Thread.Sleep(2000);
                        //WorkAsync(fetchEntities(true));
                    }
                }
            });
        }

        private Deprecate.MetaDataWithCheck[] fieldsWithCheckedState()
        {
            var fieldList = entityFieldList;
            var attrWithCheckedState = entityFieldList.Items
                .Cast<Deprecate.MetaData>()
                .Select((metaData, i) =>
                    new Deprecate.MetaDataWithCheck(
                        metaData,
                        (Deprecate.DeprecationState) fieldList.GetItemCheckState(i))
                )
                .ToArray();
            return attrWithCheckedState;
        }

        private void checkBoxStyle(object sender, PaintEventArgs e)
        {
            CheckBox s = (CheckBox)sender;
            if (s.CheckState == CheckState.Indeterminate)
            {
                e.Graphics.FillRectangle(Brushes.White, new Rectangle(new Point(4, 4), new Size(6, 8)));
                e.Graphics.DrawString("-", s.Font, Brushes.Black, new Point(1, 1));
            }
        }

    }
}