using System;
using System.Collections;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Collections;
using Rhino.DocObjects.Tables;
using Rhino.Display;
using System.Collections.Generic;
using Rhino.DocObjects;

namespace GhPython.DocReplacement
{
    public class GrasshopperDocument
    {
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
                    (geom as IGH_BakeAwareData).BakeGeometry(RhinoDoc.ActiveDoc, attr, out guid);
                    if(!guid.Equals(Guid.Empty))
                        newGuids.Add(guid);
                }else
                    throw new ApplicationException("UnexpectedObjectException. Please report this error to giulio@mcneel.com");
            }

            return newGuids;
        }

        public object this[Guid id]
        {
            get
            {
                return Objects.Contains(id) ? Objects.Find(id).Geometry : null;
            }
        }


        public IEnumerable this[IEnumerable guids]
        {

            get
            {
                if (guids == null)
                    throw new ArgumentNullException("guids",
                        "Cannot obtain a null item or subset from " + GhPython.Component.PythonComponent.DOCUMENT_NAME);

                return SubSet(guids);
            }
        }

        public IEnumerable SubSet(IEnumerable guids)
        {
            if (guids == null)
                throw new ArgumentNullException("guids",
                    "Cannot obtain a null item or subset from " + GhPython.Component.PythonComponent.DOCUMENT_NAME);

            foreach (var obj in guids)
            {
                if (obj is Guid)
                {
                    var id = (Guid)obj;
                    if (Objects.Contains(id))
                        yield return Objects.Find(id).Geometry;
                    else
                        yield return null;
                }
                else
                    yield return null;
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
                return RhinoDoc.ActiveDoc.DistanceDisplayPrecision;
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
                return RhinoDoc.ActiveDoc.IsSendingMail;
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
                return RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
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
                return RhinoDoc.ActiveDoc.ModelAngleToleranceDegrees;
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
                return RhinoDoc.ActiveDoc.ModelAngleToleranceRadians;
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
                return RhinoDoc.ActiveDoc.ModelRelativeTolerance;
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
                return RhinoDoc.ActiveDoc.ModelUnitSystem;
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
                return RhinoDoc.ActiveDoc.Notes;
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
                return RhinoDoc.ActiveDoc.PageAbsoluteTolerance;
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
                return RhinoDoc.ActiveDoc.PageAngleToleranceDegrees;
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
                return RhinoDoc.ActiveDoc.PageAngleToleranceRadians;
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
                return RhinoDoc.ActiveDoc.PageRelativeTolerance;
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
                return RhinoDoc.ActiveDoc.PageUnitSystem;
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
                return RhinoDoc.ActiveDoc.TemplateFileUsed;
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
        GhViewTable _views = new GhViewTable(() => RhinoDoc.ActiveDoc.Views, false);
        public GhViewTable Views
        {
            get
            {
              return _views;
            }
        }
    }

    public class GhViewTable : IEnumerable<RhinoView>
    {
        Func<ViewTable> _tableFunc;
        bool _redraws;

        public GhViewTable(Func<ViewTable> tableFunc, bool redraws)
        {
            _tableFunc = tableFunc;
            _redraws = redraws;
        }

        public RhinoView ActiveView
        {
            get
            {
                return _tableFunc().ActiveView;
            }
            set
            {
                _tableFunc().ActiveView = value;
            }
        }

        public RhinoDoc Document
        {
            get
            {
                return _tableFunc().Document;
            }
        }

        public bool RedrawEnabled
        {
            get
            {
                return _tableFunc().RedrawEnabled;
            }
            set
            {
                _tableFunc().RedrawEnabled = value;
            }
        }

        public RhinoPageView AddPageView(string title)
        {
            return _tableFunc().AddPageView(title);
        }

        public RhinoPageView AddPageView(string title, double pageWidth, double pageHeight)
        {
            return _tableFunc().AddPageView(title, pageWidth, pageHeight);
        }

        public void DefaultViewLayout()
        {
            _tableFunc().DefaultViewLayout();
        }

        public RhinoView Find(Guid mainViewportId)
        {
            return Find(mainViewportId);
        }

        public RhinoView Find(string mainViewportName, bool compareCase)
        {
            return _tableFunc().Find(mainViewportName, compareCase);
        }

        public void FlashObjects(IEnumerable<RhinoObject> list, bool useSelectionColor)
        {
            _tableFunc().FlashObjects(list, useSelectionColor);
        }

        public void FourViewLayout(bool useMatchingViews)
        {
            _tableFunc().FourViewLayout(useMatchingViews);
        }

        public IEnumerator<RhinoView> GetEnumerator()
        {
            return _tableFunc().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public RhinoPageView[] GetPageViews()
        {
            return _tableFunc().GetPageViews();
        }

        public RhinoView[] GetStandardRhinoViews()
        {
            return _tableFunc().GetStandardRhinoViews();
        }

        public RhinoView[] GetViewList(bool includeStandardViews, bool includePageViews)
        {
            return _tableFunc().GetViewList(includeStandardViews, includePageViews);
        }

        public void Redraw()
        {
            if (_redraws)
                _tableFunc().Redraw();
        }

        public void ThreeViewLayout(bool useMatchingViews)
        {
            _tableFunc().ThreeViewLayout(useMatchingViews);
        }
    }
}


