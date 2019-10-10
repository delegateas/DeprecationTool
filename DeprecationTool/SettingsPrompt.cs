using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeprecationTool.Models;

namespace DeprecationTool
{
    class SettingsPrompt
    {
        public static Settings SettingsDialog(string fieldPrefixStr, string deprecationPrefixStr, string caption, Settings pluginSettings)
        {
            Form prompt = new Form()
            {
                Width = 250,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label fieldPrefixLabel = new Label() { Left = 25, Top = 20, Text = fieldPrefixStr, Width = 175};
            TextBox fieldPrefix = new TextBox() { Left = 25, Top = 35, Width = 175 };

            Label dreprecationPrefixLabel = new Label() { Left = 25, Top = 65, Text = deprecationPrefixStr, Width = 175};
            TextBox deprecationPrefix = new TextBox() { Left = 25, Top = 80, Width = 175 };

            Button confirmation = new Button() { Text = "Save", Left = 25, Width = 175, Top = 120, DialogResult = DialogResult.OK };

            if (pluginSettings.FieldPrefix != null)
                fieldPrefix.Text = pluginSettings.FieldPrefix;
            
            if (pluginSettings.DeprecationPrefix != null)
                fieldPrefix.Text = pluginSettings.DeprecationPrefix;

            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(fieldPrefix);
            prompt.Controls.Add(deprecationPrefix);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(fieldPrefixLabel);
            prompt.Controls.Add(dreprecationPrefixLabel);
            prompt.AcceptButton = confirmation;

            // If changes are made, we return a new settings object, if user cancels, return old settings
            return prompt.ShowDialog() == DialogResult.OK 
                ? new Settings()
                {
                    FieldPrefix = fieldPrefix.Text,
                    DeprecationPrefix = deprecationPrefix.Text
                } 
                : pluginSettings;
        }
    
    }
}
