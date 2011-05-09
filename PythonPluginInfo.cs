using System.Drawing;
using System.Reflection;
using GhPython.Properties;
using Grasshopper.Kernel;

namespace GhPython
{
    public class PythonPluginInfo : GH_AssemblyInfo
    {
        public override string AssemblyDescription
        {
            get
            {
                return "The Grasshopper Python interpreter component";
            }
        }

        public override Bitmap AssemblyIcon
        {
            get
            {
                return Resources.python;
            }
        }

        public override string AssemblyName
        {
            get
            {
                return "Python Interpreter";
            }
        }

        public override string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public override string AuthorName
        {
            get
            {
                return "McNeel Europe, Barcelona";
            }
        }

        public override string AuthorContact
        {
            get
            {
                return "giulio@mcneel.com";
            }
        }

        public override GH_LibraryLicense AssemblyLicense
        {
            get
            {
                return GH_LibraryLicense.opensource;
            }
        }
    }
}