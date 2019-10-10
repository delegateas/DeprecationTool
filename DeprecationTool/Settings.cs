using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeprecationTool.Models;

namespace DeprecationTool
{
    /// <summary>
    /// This class can help you to store settings for your plugin
    /// </summary>
    /// <remarks>
    /// This class must be XML serializable
    /// </remarks>
    public class Settings
    {
        public string FieldPrefix { get; set; }
        public string DeprecationPrefix { get; set; }
    }
}