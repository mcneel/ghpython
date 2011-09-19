using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Rhino.Runtime;
using Grasshopper.Kernel;
using System.Reflection;
using System.Collections.ObjectModel;

namespace GhPython.Component
{
    /* Giulio Piacentino, 2011-9-19
     * This is experimental code to load Grasshopper "gha"s that needs to be reviewed
     * 
    class GHComponentsLoader
    {
        internal static void LoadAllLibraries()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            var ghLibrariesPath = Path.Combine(appDataPath, "Grasshopper", "Libraries");

            var files = Directory.EnumerateFiles(ghLibrariesPath, "*.gha.py");
            var exceptions = new List<Exception>();

            PythonScript script = null;

            foreach (var file in files)
            {
                try
                {
                    if (script == null)
                        script = PythonScript.Create();

                    script.ExecuteScript(
                        string.Format("from clr import AddReference\nAddReference(\"{0}\")\nAddReference(\"{1}\")",
                    Assembly.GetExecutingAssembly().FullName,
                    typeof(IGH_Component).Assembly.FullName
                    ));


                    if (!script.ExecuteFile(file))
                        throw new InvalidOperationException(string.Format("File {0} cannot be executed", file));
                }
                catch(Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            //TODO: Say something in case of bad loadings
            //TODO: chaeck & install

            var l = Plugins.List;
        }
    }
}

namespace GhPython
{
    public static class Plugins
    {
        static List<IGH_Component> list = new List<IGH_Component>();

        public static void Add(IGH_Component component)
        {
            Type t = component.GetType();
            var ctors = t.GetConstructors(BindingFlags.NonPublic);

            try
            {
                object newobj = System.Activator.CreateInstance(t);
            }
            catch(Exception ex)
            {
                int h = 0; 
                    
            }
            list.Add(component);
        }

        public static IList<IGH_Component> List
        {
            get {

                return new ReadOnlyCollection<IGH_Component>(list);
            }
        }
    }
    */
}