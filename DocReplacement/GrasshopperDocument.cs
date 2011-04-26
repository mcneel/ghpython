using System;
using System.Collections.Generic;
using System.Text;
using Rhino;
using Rhino.DocObjects.Tables;
using Grasshopper;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Collections;
using Grasshopper.Kernel;

namespace GhPython.DocReplacement
{
    public class GrasshopperDocument
    {
        RhinoDoc _doc = RhinoDoc.ActiveDoc;
        ObjectTable _docTable = RhinoDoc.ActiveDoc.Objects;
        CustomTable _table = new CustomTable();

        public RhinoList<Guid> CommitIntoRhinoDocument()
        {
            RhinoList<Guid> newGuids = new RhinoList<Guid>(Objects.Count);

            foreach(var content in this.Objects.AttributedGeometries)
            {
                var geom = content.Geometry;
                var attr = content.Attributes;

                Guid guid = Guid.Empty;

                if(geom is IGH_BakeAwareData)
                {
                    (geom as IGH_BakeAwareData).BakeGeometry(_doc, attr, out guid);
                    if(!guid.Equals(Guid.Empty))
                        newGuids.Add(guid);
                }else
                    throw new ApplicationException("UnexpectedObjectException. Please report this error to giulio@mcneel.com");
            }

            return newGuids;
        }

        public object this[Guid id]{
        
            get
            {
                return Objects.Contains(id) ? Objects.Find(id).Geometry : null;
            }
        }

        public BitmapTable Bitmaps
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DimStyleTable DimStyles
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int DistanceDisplayPrecision
        {
            get
            {
                return _doc.DistanceDisplayPrecision;
            }
        }

        public FontTable Fonts
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public GroupTable Groups
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public InstanceDefinitionTable InstanceDefinitions
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public CustomTable Objects
        {
            get
            {
                return _table;
            }
        }

        public bool IsLocked
        {
            get
            {
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsSendingMail
        {
            get
            {
                return _doc.IsSendingMail;
            }
        }

        public LayerTable Layers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public MaterialTable Materials
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public double ModelAbsoluteTolerance
        {
            get
            {
                return _doc.ModelAbsoluteTolerance;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double ModelAngleToleranceDegrees
        {
            get
            {
                return _doc.ModelAngleToleranceDegrees;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double ModelAngleToleranceRadians
        {
            get
            {
                return _doc.ModelAngleToleranceRadians;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double ModelRelativeTolerance
        {
            get
            {
                return _doc.ModelRelativeTolerance;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public UnitSystem ModelUnitSystem
        {
            get
            {
                return _doc.ModelUnitSystem;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Modified
        {
            get
            {
                return true;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                return GH_InstanceServer.DocumentServer[0].FileNameProxy;
            }
        }
        public NamedConstructionPlaneTable NamedConstructionPlanes
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public NamedViewTable NamedViews
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public string Notes
        {
            get
            {
                return _doc.Notes;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double PageAbsoluteTolerance
        {
            get
            {
                return _doc.PageAbsoluteTolerance;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double PageAngleToleranceDegrees
        {
            get
            {
                return _doc.PageAngleToleranceDegrees;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double PageAngleToleranceRadians
        {
            get
            {
                return _doc.PageAngleToleranceRadians;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        
        
        /// <summary>
        /// Page space relative tolerance.
        /// </summary>
        public double PageRelativeTolerance
        {
            get
            {
                return _doc.PageRelativeTolerance;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public UnitSystem PageUnitSystem
        {
            get
            {
                return _doc.PageUnitSystem;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns the path of the currently loaded Grasshopper document (ghx file).
        /// </summary>
        public string Path
        {
            get
            {
                return GH_InstanceServer.DocumentServer[0].FilePath;
            }
        }
        public StringTable Strings
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// name of the template file used to create this document.
        /// This is a runtime value only present if the document was newly created.
        /// </summary>
        public string TemplateFileUsed
        {
            get
            {
                return _doc.TemplateFileUsed;
            }
        }
        public bool UndoRecordingEnabled
        {
            get
            {
                return false;
            }
            set
            {
                if(value)
                    throw new NotSupportedException("No undo is supported the Grasshopper-Python transparent document");
            }
        }
        GHViewTable _views = new GHViewTable();
        public GHViewTable Views
        {
            get
            {
              return _views;// _doc.Views;
            }
        }
    }
}


public class GHViewTable
{
  public void Redraw() { }
}