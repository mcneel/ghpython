using System.Drawing;
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
                return "The Python interpreter component";
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
                return "0.1.4.0";
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
    }
}