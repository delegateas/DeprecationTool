using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using McTools.Xrm.Connection;

namespace DeprecationTool
{
    public partial class DeprecateControl : PluginControlBase
    {
        private Settings pluginSettings;
        private IDictionary<string, IDictionary<string, Lib.Deprecate.MetaData[]>> solutionsWithData;
        private Lib.Deprecate.SolutionData[] solutions;

        public DeprecateControl()
        {
            InitializeComponent();
        }

        private void DeprecateControl_Load(object sender, EventArgs e)
        {
            // ShowInfoNotification("This is a notification that can lead to XrmToolBox repository", new Uri("https://github.com/MscrmTools/XrmToolBox"));

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

            LoadData();
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        private void tsbSample_Click(object sender, EventArgs e)
        {
            // The ExecuteMethod method handles connecting to an
            // organization if XrmToolBox is not yet connected
            ExecuteMethod(GetAccounts);
        }

        private void GetAccounts()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Getting accounts",
                Work = (worker, args) =>
                {
                    args.Result = Service.RetrieveMultiple(new QueryExpression("account")
                    {
                        TopCount = 50
                    });
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    var result = args.Result as EntityCollection;
                    if (result != null)
                    {
                        MessageBox.Show($"Found {result.Entities.Count} accounts");
                    }
                }
            });
        }

        private void LoadData()
        {
            var fetchEntities = new WorkAsyncInfo
            {
                Message = "Fetching entities and attributes",
                Work = (worker, args) =>
                {
                    args.Result = Lib.Deprecate.retrieveSolutionEntities(Service, solutions, "dg_");
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    var result = args.Result as IDictionary<string, IDictionary<string, Lib.Deprecate.MetaData[]>>;
                    if (result != null)
                    {
                        solutionsWithData = result;
                        populateEntitiesListView();
                    }
                }
            };

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Fetching solutions",
                Work = (worker, args) =>
                {
                    args.Result = Lib.Deprecate.retrieveSolutionNames(Service, new string[] { "Default" }, "");
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    var result = args.Result as Lib.Deprecate.SolutionData[];
                    if (result != null)
                    {
                        solutions = result;
                        populateSolutionsComboBox();
                        WorkAsync(fetchEntities);
                    }
                }
            });            
        }

        private void populateSolutionsComboBox()
        {
            foreach (var item in solutions)
                solutionComboBox.Items.Add(item.uniqueName);
        }

        private void populateEntitiesListView()
        {
            solutionsWithData.TryGetValue("OnboardingASA", out var res);

            foreach (var item in res.Keys)
                entityListView.Items.Add(new ListViewItem(new string[] { item }));

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

            if (pluginSettings != null && detail != null)
            {
                pluginSettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
                LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
            }
        }

        private void renderAttributeView(Lib.Deprecate.MetaData[] entity)
        {
            attributeList.Items.Clear();

            foreach(var attr in entity)
            {
                attributeList.Items.Add(attr.attribute.LogicalName);
            }


        }

        private void entityListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            var entityList = (ListView) sender;
            var selectedListViewItems = entityList.SelectedItems.Cast<ListViewItem>();
            var currentlySelected = selectedListViewItems.FirstOrDefault();
            if (currentlySelected != null)
            {
                if(solutionsWithData.TryGetValue("OnboardingASA", out var selectedSolution))
                {
                    if (selectedSolution.TryGetValue(currentlySelected.SubItems[0].Text, out var selectedEntity))
                    {
                        renderAttributeView(selectedEntity);
                    }

                }


            }
            // First subitem in selection should be name. This is sadly not strongly typed.

        }

        private void dropChangesButton_Click(object sender, EventArgs e)
        {

        }

        private void applyButton_Click(object sender, EventArgs e)
        {

        }

        private void attributeList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}