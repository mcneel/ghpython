using System.Drawing;
using System.Reflection;
using GhPython.Properties;
using Grasshopper.Kernel;

namespace GhPython
{
  public class PythonPluginInfo : GH_AssemblyInfo
  {
    public override string Description
    {
      get { return "Python interpreter component for grasshopper"; }
    }

    public override Bitmap Icon
    {
      get { return Resources.python; }
    }

    public override string Name
    {
      get { return "Python Interpreter"; }
    }

    public override string Version
    {
      get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
    }

    public override string AuthorName
    {
      get { return "Robert McNeel and Associates"; }
    }

    public override string AuthorContact
    {
      get { return "giulio@mcneel.com"; }
    }

    public override GH_LibraryLicense License
    {
      get { return GH_LibraryLicense.opensource; }
    }
  }
}