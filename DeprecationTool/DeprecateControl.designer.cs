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
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbSample = new System.Windows.Forms.ToolStripButton();
            this.entityListView = new System.Windows.Forms.ListView();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.dropChangesButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.toolStripMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbClose,
            this.tssSeparator1,
            this.tsbSample});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Size = new System.Drawing.Size(559, 25);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";
            // 
            // tsbClose
            // 
            this.tsbClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Size = new System.Drawing.Size(86, 22);
            this.tsbClose.Text = "Close this tool";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);
            // 
            // tssSeparator1
            // 
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbSample
            // 
            this.tsbSample.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbSample.Name = "tsbSample";
            this.tsbSample.Size = new System.Drawing.Size(47, 22);
            this.tsbSample.Text = "Try me";
            this.tsbSample.Click += new System.EventHandler(this.tsbSample_Click);
            // 
            // entityListView
            // 
            this.entityListView.HideSelection = false;
            this.entityListView.Location = new System.Drawing.Point(4, 29);
            this.entityListView.Name = "entityListView";
            this.entityListView.Size = new System.Drawing.Size(278, 268);
            this.entityListView.TabIndex = 5;
            this.entityListView.UseCompatibleStateImageBehavior = false;
            this.entityListView.SelectedIndexChanged += new System.EventHandler(this.entityListView_SelectedIndexChanged);
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(288, 29);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(268, 229);
            this.checkedListBox1.TabIndex = 6;
            // 
            // dropChangesButton
            // 
            this.dropChangesButton.Location = new System.Drawing.Point(288, 267);
            this.dropChangesButton.Name = "dropChangesButton";
            this.dropChangesButton.Size = new System.Drawing.Size(95, 23);
            this.dropChangesButton.TabIndex = 7;
            this.dropChangesButton.Text = "Drop changes";
            this.dropChangesButton.UseVisualStyleBackColor = true;
            this.dropChangesButton.Click += new System.EventHandler(this.dropChangesButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(440, 267);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(116, 23);
            this.applyButton.TabIndex = 8;
            this.applyButton.Text = "Apply Deprecations";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // DeprecateControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.dropChangesButton);
            this.Controls.Add(this.checkedListBox1);
            this.Controls.Add(this.entityListView);
            this.Controls.Add(this.toolStripMenu);
            this.Name = "DeprecateControl";
            this.Size = new System.Drawing.Size(559, 300);
            this.Load += new System.EventHandler(this.MyPluginControl_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripButton tsbSample;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.ListView entityListView;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.Button dropChangesButton;
        private System.Windows.Forms.Button applyButton;
    }
}
