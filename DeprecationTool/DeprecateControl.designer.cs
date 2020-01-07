using System;
using System.Drawing;
using System.Windows.Forms;

namespace DeprecationTool
{
    partial class DeprecateControl
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeprecateControl));
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.solutionComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsReload = new System.Windows.Forms.ToolStripButton();
            this.tsSettings = new System.Windows.Forms.ToolStripButton();
            this.tsInfo = new System.Windows.Forms.ToolStripButton();
            this.tsClose = new System.Windows.Forms.ToolStripButton();
            this.entityList = new System.Windows.Forms.ListView();
            this.EntityHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.entityFieldList = new CustomControls.CheckBoxListView();
            this.EntityAttributeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.resetButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.fixPartialButton = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.toolStripMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.solutionComboBox,
            this.tssSeparator1,
            this.tsReload,
            this.tsSettings,
            this.tsInfo,
            this.tsClose});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Size = new System.Drawing.Size(559, 31);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";
            // 
            // solutionComboBox
            // 
            this.solutionComboBox.DropDownWidth = 271;
            this.solutionComboBox.Name = "solutionComboBox";
            this.solutionComboBox.Size = new System.Drawing.Size(271, 31);
            this.solutionComboBox.ToolTipText = "Solutions when org. is loaded";
            this.solutionComboBox.SelectedIndexChanged += new System.EventHandler(this.solutionComboBox_SelectedIndexChanged);
            // 
            // tssSeparator1
            // 
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(6, 31);
            // 
            // tsReload
            // 
            this.tsReload.Image = global::DeprecationTool.Properties.Resources.ReloadIcon;
            this.tsReload.Name = "tsReload";
            this.tsReload.Size = new System.Drawing.Size(86, 28);
            this.tsReload.Text = "Reload all";
            this.tsReload.Click += new System.EventHandler(this.reload_click);
            // 
            // tsSettings
            // 
            this.tsSettings.Image = global::DeprecationTool.Properties.Resources.SettingsIcon;
            this.tsSettings.Name = "tsSettings";
            this.tsSettings.Size = new System.Drawing.Size(77, 28);
            this.tsSettings.Text = "Settings";
            this.tsSettings.ToolTipText = "Plugin info";
            this.tsSettings.Click += new System.EventHandler(this.tsSettings_Click);
            // 
            // tsInfo
            // 
            this.tsInfo.Image = ((System.Drawing.Image)(resources.GetObject("tsInfo.Image")));
            this.tsInfo.Name = "tsInfo";
            this.tsInfo.Size = new System.Drawing.Size(60, 28);
            this.tsInfo.Text = "Help";
            this.tsInfo.Click += new System.EventHandler(this.tsInfo_Click);
            // 
            // tsClose
            // 
            this.tsClose.Image = global::DeprecationTool.Properties.Resources.CloseIcon;
            this.tsClose.Name = "tsClose";
            this.tsClose.Size = new System.Drawing.Size(64, 28);
            this.tsClose.Text = "Close";
            this.tsClose.Click += new System.EventHandler(this.tsbClose_Click);
            // 
            // entityList
            // 
            this.entityList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.EntityHeader});
            this.entityList.Dock = System.Windows.Forms.DockStyle.Left;
            this.entityList.FullRowSelect = true;
            this.entityList.HideSelection = false;
            this.entityList.Location = new System.Drawing.Point(0, 31);
            this.entityList.MultiSelect = false;
            this.entityList.Name = "entityList";
            this.entityList.Size = new System.Drawing.Size(279, 269);
            this.entityList.TabIndex = 5;
            this.entityList.UseCompatibleStateImageBehavior = false;
            this.entityList.View = System.Windows.Forms.View.Details;
            this.entityList.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.entityListView_SelectedIndexChanged);
            // 
            // EntityHeaderName
            // 
            this.EntityHeader.Text = "Name";
            this.EntityHeader.Width = 150;
            // 
            // entityFieldList
            // 
            //this.entityFieldList.CheckBoxes = true;
            this.entityFieldList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.EntityAttributeHeader});
            this.entityFieldList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entityFieldList.HideSelection = false;
            this.entityFieldList.Location = new System.Drawing.Point(0, 0);
            this.entityFieldList.Name = "entityFieldList";
            this.entityFieldList.Size = new System.Drawing.Size(280, 209);
            this.entityFieldList.TabIndex = 6;
            this.entityFieldList.UseCompatibleStateImageBehavior = false;
            this.entityFieldList.View = System.Windows.Forms.View.Details;
            this.entityFieldList.ColumnClick += fieldListColumnClick;
            this.entityFieldList.KeyDown += fieldListOnkeyboardPress;
            // 
            // Name
            // 
            this.EntityAttributeHeader.Text = "Name";
            this.EntityAttributeHeader.Width = 80;
            // 
            // resetButton
            // 
            this.resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.resetButton.Enabled = false;
            this.resetButton.Location = new System.Drawing.Point(6, 7);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(48, 46);
            this.resetButton.TabIndex = 7;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.Enabled = false;
            this.applyButton.Location = new System.Drawing.Point(206, 7);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(58, 46);
            this.applyButton.TabIndex = 8;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // fixPartialButton
            // 
            this.fixPartialButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.fixPartialButton.Enabled = false;
            this.fixPartialButton.Location = new System.Drawing.Point(102, 7);
            this.fixPartialButton.Name = "fixPartialButton";
            this.fixPartialButton.Size = new System.Drawing.Size(60, 46);
            this.fixPartialButton.TabIndex = 9;
            this.fixPartialButton.Text = "Fix Partial";
            this.fixPartialButton.UseVisualStyleBackColor = true;
            this.fixPartialButton.Click += new System.EventHandler(this.fixPartialButton_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(279, 31);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.entityFieldList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.fixPartialButton);
            this.splitContainer1.Panel2.Controls.Add(this.resetButton);
            this.splitContainer1.Panel2.Controls.Add(this.applyButton);
            this.splitContainer1.Size = new System.Drawing.Size(280, 269);
            this.splitContainer1.SplitterDistance = 209;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 10;
            // 
            // DeprecateControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.entityList);
            this.Controls.Add(this.toolStripMenu);
            this.Size = new System.Drawing.Size(559, 300);
            this.Load += new System.EventHandler(this.DeprecateControl_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsClose;
        private System.Windows.Forms.ToolStripButton tsReload;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.ListView entityList;
        private CustomControls.CheckBoxListView entityFieldList;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.ToolStripComboBox solutionComboBox;
        private ColumnHeader EntityHeader;
        private ColumnHeader EntityAttributeHeader;
        private ToolStripButton tsSettings;
        private Button fixPartialButton;
        private SplitContainer splitContainer1;
        private ToolStripButton tsInfo;
    }
}
