#if GH_0_9

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino;

namespace GhPython.Assemblies
{
  class PythonInstantiatorProxy : IGH_ObjectProxy
	{
    dynamic _operations;
    object _pythonType;

    private Bitmap _icon;
		private bool _obsolete;
		private bool _compliant;
		private GH_Exposure _exposure;
		private string _location;
		private IGH_InstanceDescription _description;

    internal PythonInstantiatorProxy(IGH_DocumentObject obj, object pythonType, dynamic operations, string location)
		{
      _operations = operations;
      _pythonType = pythonType;

			_location = string.Empty;
			_description = new GH_InstanceDescription(obj);
			Guid = obj.ComponentGuid;
			_icon = obj.Icon_24x24;
			_exposure = obj.Exposure;
			_obsolete = obj.Obsolete;
			_compliant = true;
			if (obj is IGH_ActiveObject)
			{
				IGH_ActiveObject actobj = (IGH_ActiveObject)obj;
				if (!actobj.SDKCompliancy(RhinoApp.ExeVersion, RhinoApp.ExeServiceRelease))
				{
					this._compliant = false;
				}
			}
			Type = obj.GetType();
      this.LibraryGuid = GH_Convert.StringToGuid(location);
      this._location = location;
			if (this._location.Length > 0)
			{
				this._location = this._location.Replace("file:///", string.Empty);
				this._location = this._location.Replace("/", Convert.ToString(Path.DirectorySeparatorChar));
			}
		}

    private PythonInstantiatorProxy() { }

		public string Location
		{
			get
			{
				return this._location;
			}
		}

    public Guid LibraryGuid
    {
      get;
      private set;
    }

		public Bitmap Icon
		{
			get
			{
				if (this._icon == null && GH_InstanceServer.IsComponentServer)
				{
					IGH_DocumentObject obj = GH_InstanceServer.ComponentServer.EmitObject(this.Guid);
					if (obj != null)
					{
						this._icon = obj.Icon_24x24;
					}
					if (this._icon == null)
					{
						this._icon = Grasshopper.GUI.GH_StandardIcons.BlankObjectIcon_24x24;
					}
				}
				return this._icon;
			}
		}

		public IGH_InstanceDescription Desc
		{
			get
			{
				return this._description;
			}
		}

    public Type Type
    {
      get;
      private set;
    }

		public GH_ObjectType Kind
		{
			get
			{
				return GH_ObjectType.CompiledObject;
			}
		}

    public Guid Guid
    {
      get;
      private set;
    }

		public GH_Exposure Exposure
		{
			get
			{
				return this._exposure;
			}
			set
			{
				this._exposure = value;
			}
		}

		public bool SDKCompliant
		{
			get
			{
				return this._compliant;
			}
		}

		public bool Obsolete
		{
			get
			{
				return this._obsolete;
			}
		}

		public IGH_ObjectProxy DuplicateProxy()
		{
      PythonInstantiatorProxy dup = new PythonInstantiatorProxy();
			dup._description = new GH_InstanceDescription(this.Desc);
			dup.Guid = this.Guid;
			dup.Type = this.Type;
			dup._location = this.Location;
			dup._exposure = this.Exposure;
			dup.LibraryGuid = this.LibraryGuid;
			dup._compliant = this.SDKCompliant;
			dup._obsolete = this.Obsolete;
			if (this._icon != null)
			{
				dup._icon = (Bitmap)this._icon.Clone();
			}
			return dup;
		}

		public IGH_DocumentObject CreateInstance()
		{
			try
			{
        object docObject = _operations.Invoke(_pythonType);
        if (docObject != null)
        {
          return (docObject as IGH_DocumentObject);
        }
			}
			catch
			{
			}
			return null;
		}
	}
}
#endif