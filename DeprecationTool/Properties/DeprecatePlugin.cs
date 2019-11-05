using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;
using Lib;

namespace DeprecationTool
{
    [Export(typeof(IXrmToolBoxPlugin)),
        ExportMetadata("Name", "Deprecation Tool"),
        ExportMetadata("Description", "Field deprecation tool for XRM"),
        // Please specify the base64 content of a 32x32 pixels image
        ExportMetadata("SmallImageBase64", "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"),
        // Please specify the base64 content of a 80x80 pixels image
        ExportMetadata("BigImageBase64", "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"),
        ExportMetadata("BackgroundColor", "SlateGray"),
        ExportMetadata("PrimaryFontColor", "Snow"),
        ExportMetadata("SecondaryFontColor", "White")]
    public class DeprecatePlugin : PluginBase
    {
        public override IXrmToolBoxPluginControl GetControl()
        {
            return new DeprecateControl();
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        public DeprecatePlugin()
        {
            // If you have external assemblies that you need to load, uncomment the following to 
            // hook into the event that will fire when an Assembly fails to resolve
            // AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolveEventHandler);
        }

        /// <summary>
        /// Event fired by CLR when an assembly reference fails to load
        /// Assumes that related assemblies will be loaded from a subfolder named the same as the Plugin
        /// For example, a folder named Sample.XrmToolBox.MyPlugin 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly AssemblyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            Assembly loadAssembly = null;
            Assembly currAssembly = Assembly.GetExecutingAssembly();

            // base name of the assembly that failed to resolve
            var argName = args.Name.Substring(0, args.Name.IndexOf(","));

            // check to see if the failing assembly is one that we reference.
            List<AssemblyName> refAssemblies = currAssembly.GetReferencedAssemblies().ToList();
            var refAssembly = refAssemblies.Where(a => a.Name == argName).FirstOrDefault();

            // if the current unresolved assembly is referenced by our plugin, attempt to load
            if (refAssembly != null)
            {
                // load from the path to this plugin assembly, not host executable
                string dir = Path.GetDirectoryName(currAssembly.Location).ToLower();
                string folder = Path.GetFileNameWithoutExtension(currAssembly.Location);
                dir = Path.Combine(dir, folder);

                var assmbPath = Path.Combine(dir, $"{argName}.dll");

                if (File.Exists(assmbPath))
                {
                    loadAssembly = Assembly.LoadFrom(assmbPath);
                }
                else
                {
                    throw new FileNotFoundException($"Unable to locate dependency: {assmbPath}");
                }
            }

            return loadAssembly;
        }
    }
}