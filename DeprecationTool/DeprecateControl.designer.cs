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
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.tsSettings = new System.Windows.Forms.ToolStripButton();
            this.entityList = new System.Windows.Forms.ListView();
            this.EntityHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.entityFieldList = new System.Windows.Forms.CheckedListBox();
            this.resetButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.fixPartialButton = new System.Windows.Forms.Button();
            this.toolStripMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.solutionComboBox,
            this.tssSeparator1,
            this.tsReload,
            this.tsbClose,
            this.tsSettings});
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
            // tsbClose
            // 
            this.tsbClose.Image = global::DeprecationTool.Properties.Resources.CloseIcon;
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Size = new System.Drawing.Size(88, 28);
            this.tsbClose.Text = "Close tool";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);
            // 
            // tsSettings
            // 
            this.tsSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsSettings.Image = ((System.Drawing.Image)(resources.GetObject("tsSettings.Image")));
            this.tsSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsSettings.Name = "tsSettings";
            this.tsSettings.Size = new System.Drawing.Size(28, 28);
            this.tsSettings.Text = "tsSettings";
            this.tsSettings.Click += new System.EventHandler(this.tsSettings_Click);
            // 
            // entityList
            // 
            this.entityList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.EntityHeaderName});
            this.entityList.FullRowSelect = true;
            this.entityList.HideSelection = false;
            this.entityList.Location = new System.Drawing.Point(3, 29);
            this.entityList.MultiSelect = false;
            this.entityList.Name = "entityList";
            this.entityList.Size = new System.Drawing.Size(279, 268);
            this.entityList.TabIndex = 5;
            this.entityList.UseCompatibleStateImageBehavior = false;
            this.entityList.View = System.Windows.Forms.View.Details;
            this.entityList.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.entityListView_SelectedIndexChanged);
            // 
            // EntityHeaderName
            // 
            this.EntityHeaderName.Text = "Name";
            // 
            // entityFieldList
            // 
            this.entityFieldList.CheckOnClick = true;
            this.entityFieldList.FormattingEnabled = true;
            this.entityFieldList.Location = new System.Drawing.Point(288, 29);
            this.entityFieldList.Name = "entityFieldList";
            this.entityFieldList.Size = new System.Drawing.Size(268, 229);
            this.entityFieldList.TabIndex = 6;
            //this.entityFieldList.Paint += new System.Windows.Forms.PaintEventHandler(this.CheckBoxStyle);
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(288, 267);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(48, 23);
            this.resetButton.TabIndex = 7;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(498, 267);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(58, 23);
            this.applyButton.TabIndex = 8;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // fixPartialButton
            // 
            this.fixPartialButton.Location = new System.Drawing.Point(382, 267);
            this.fixPartialButton.Name = "fixPartialButton";
            this.fixPartialButton.Size = new System.Drawing.Size(60, 23);
            this.fixPartialButton.TabIndex = 9;
            this.fixPartialButton.Text = "Fix Partial";
            this.fixPartialButton.UseVisualStyleBackColor = true;
            this.fixPartialButton.Click += new System.EventHandler(this.fixPartialButton_Click);
            // 
            // DeprecateControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.fixPartialButton);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.entityFieldList);
            this.Controls.Add(this.entityList);
            this.Controls.Add(this.toolStripMenu);
            this.Name = "DeprecateControl";
            this.Size = new System.Drawing.Size(559, 300);
            this.Load += new System.EventHandler(this.DeprecateControl_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripButton tsReload;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.ListView entityList;
        private System.Windows.Forms.CheckedListBox entityFieldList;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.ToolStripComboBox solutionComboBox;
        private ColumnHeader EntityHeaderName;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private ToolStripButton tsSettings;
        private Button fixPartialButton;
    }
}
