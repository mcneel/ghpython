using System.Drawing;
using System.Reflection;
using GhPython.Properties;
using Grasshopper.Kernel;
using GhPython.Component;

namespace GhPython
{
  public class PythonPluginInfo : GH_AssemblyInfo
  {
    public PythonPluginInfo()
    {
      //GHComponentsLoader.LoadAllLibraries();
    }

    public override string Description
    {
      get
      {
        return "The Grasshopper Python interpreter component";
      }
    }

    public override Bitmap Icon
    {
      get
      {
        return Resources.python;
      }
    }

    public override string Name
    {
      get
      {
        return "Python Interpreter";
      }
    }

    public override string Version
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
        return "Robert McNeel and Associates";
      }
    }

    public override string AuthorContact
    {
      get
      {
        return "steve@mcneel.com";
      }
    }

    public override GH_LibraryLicense License
    {
      get
      {
        return GH_LibraryLicense.opensource;
      }
    }
  }
}