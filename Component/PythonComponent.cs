using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using GhPython.DocReplacement;
using System.Windows.Forms;
using System.Collections.Generic;
using Grasshopper.Kernel.Parameters.Hints;


#pragma warning disable 0618

namespace GhPython.Component
{
    public class PythonComponent_OBSOLETE : ScriptingAncestorComponent, IGH_VarParamComponent
    {
        protected override void AddDefaultInput(GH_Component.GH_InputParamManager pManager)
        {
            pManager.RegisterParam(ConstructVariable(GH_VarParamSide.Input, "x"));
            pManager.RegisterParam(ConstructVariable(GH_VarParamSide.Input, "y"));
        }

        protected override void AddDefaultOutput(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(ConstructVariable(GH_VarParamSide.Output, "a"));
        }

        internal const string Id = "{CEAB6E56-CEEC-A646-84D5-363C57440969}";

        public override Guid ComponentGuid
        {
            get { return new Guid(Id); }
        }

        internal DocStorage DocStorageMode
        {
          get;
          set;
        }

        protected override void SetScriptTransientGlobals()
        {
          base.SetScriptTransientGlobals();

          switch (DocStorageMode)
          {
            case DocStorage.InGrasshopperMemory:
            case DocStorage.AutomaticMarshal:
              {
                _py.ScriptContextDoc = _document;
                _marshal = new OldComponentIOMarshal(_document, this);
                _py.SetVariable(DOCUMENT_NAME, _document);
                _py.SetIntellisenseVariable(DOCUMENT_NAME, _document);
                break;
              }
            case DocStorage.InRhinoDoc:
              {
                _py.ScriptContextDoc = Rhino.RhinoDoc.ActiveDoc;
                _marshal = new OldComponentIOMarshal(Rhino.RhinoDoc.ActiveDoc, this);
                Rhino.RhinoDoc.ActiveDoc.UndoRecordingEnabled = true;
                if (_py.ContainsVariable(DOCUMENT_NAME))
                {
                  _py.RemoveVariable(DOCUMENT_NAME);
                  _py.SetIntellisenseVariable(DOCUMENT_NAME, null);
                }
                break;
              }
            default:
              {
                throw new ApplicationException("Unexpected DocStorage type.");
              }
          }
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden; }
        }

        public override void VariableParameterMaintenance()
        {
            ParametersModified(GH_VarParamSide.Input);
            ParametersModified(GH_VarParamSide.Output);
        }

        class DocSetter
        {
          public DocStorage NewDocStorage;
          public PythonComponent_OBSOLETE Component;

          public void SetDoc(object sender, EventArgs e)
          {
            try
            {
              Component.CheckAndSetupActions();

              Component.DocStorageMode = NewDocStorage;
              Component.SetScriptTransientGlobals();
              Component.ExpireSolution(true);
            }
            catch (Exception ex)
            {
              GhPython.Forms.PythonScriptForm.LastHandleException(ex);
            }
          }

        }

        public ToolStripMenuItem GetTargetVariableMenuItem()
        {
          var result = new ToolStripMenuItem("&Rhinoscriptsyntax usage", null, new ToolStripItem[]
            {
                new ToolStripMenuItem("rhinoscriptsyntax / Automatically &marshal Guids", null, new DocSetter{
                    Component = this, NewDocStorage = DocStorage.AutomaticMarshal}.SetDoc)
                {
                     ToolTipText = "Inputs and outputs accept Guids. The " + DOCUMENT_NAME + " variable is available for advanced use",
                },
                new ToolStripMenuItem("RhinoCommon / Provide &" + DOCUMENT_NAME + " variable", null, new DocSetter{
                    Component = this, NewDocStorage = DocStorage.InGrasshopperMemory }.SetDoc)
                {
                     ToolTipText = "Use this option to obtain the " + DOCUMENT_NAME + " variable in your script\nand be able to assign it to the outputs manually",
                },
                new ToolStripMenuItem("Add to &Rhino document", null, new DocSetter{Component = this, NewDocStorage = DocStorage.InRhinoDoc}.SetDoc)
                {
                     ToolTipText = "Use this option to choose to use the traditional Rhino document as output. Not recommanded",
                }
            })
          {
            ToolTipText = "Choose where rhinoscriptsyntax functions have their effects",
          };

          EventHandler update = (sender, args) =>
          {
            result.DropDownItems[0].Image = GetCheckedImage(DocStorageMode == DocStorage.AutomaticMarshal);
            result.DropDownItems[1].Image = GetCheckedImage(DocStorageMode == DocStorage.InGrasshopperMemory);
            result.DropDownItems[2].Image = GetCheckedImage(DocStorageMode == DocStorage.InRhinoDoc);
          };
          update(null, EventArgs.Empty);
          result.DropDownOpening += update;

          return result;
        }
        
        #region Members of IGH_VarParamComponent

        IGH_Param ConstructVariable(GH_VarParamSide side, string nickname)
        {
            if (side == GH_VarParamSide.Input)
            {
                var param = new Param_ScriptVariable();
                if (!string.IsNullOrWhiteSpace(nickname))
                    param.NickName = nickname;
                FixGhInput(param);
                return param;
            }
            if (side == GH_VarParamSide.Output)
            {
                var param = new Param_GenericObject();
                if (string.IsNullOrWhiteSpace(nickname))
                    param.Name = param.NickName;
                else
                {
                    param.NickName = nickname;
                    param.Name = String.Format("Result {0}", nickname);
                }
                param.Description = String.Format("Output parameter {0}", param.NickName);
                return param;
            }
            return null;
        }
      
        public IGH_Param ConstructVariable(GH_VarParamEventArgs e)
        {
            return ConstructVariable(e.Side, null);
        }

        public bool IsInputVariable
        {
            get { return true; }
        }

        public bool IsOutputVariable
        {
            get { return true; }
        }

        public bool IsVariableParam(GH_VarParamEventArgs e)
        {
            return e.Index > (HideCodeInput? -1 : 0);
        }

        public void ManagerConstructed(GH_VarParamSide side, Grasshopper.GUI.GH_VariableParameterManager manager)
        {
            string pool = (side == GH_VarParamSide.Input)?"xyzuvw":"abcdef";
            manager.NameConstructor = new GH_CharPatternParamNameConstructor(pool, 4);
        }

        public void ParametersModified(GH_VarParamSide side)
        {
            switch (side)
            {
                case GH_VarParamSide.Input:
                    foreach (var i in Params.Input)
                    {
                        if (i is Param_ScriptVariable)
                            FixGhInput(i as Param_ScriptVariable);
                    }
                    break;

                case GH_VarParamSide.Output:
                    foreach (var i in Params.Input)
                    {
                        if (i is Param_GenericObject)
                        {
                            i.Name = i.NickName;
                            i.Description = i.NickName;
                        }
                    }
                    break;
            }
        }

        #endregion


        const string TargetDocIdentifier = "GhMemory";

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
          if (!Enum.IsDefined(typeof(DocStorage), DocStorageMode))
            DocStorageMode = DocStorage.InGrasshopperMemory;
          writer.SetInt32(TargetDocIdentifier, (int)DocStorageMode);

          return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
          int val = -1;
          if (reader.TryGetInt32(TargetDocIdentifier, ref val))
            DocStorageMode = (DocStorage)val;

          if (!Enum.IsDefined(typeof(DocStorage), DocStorageMode))
            DocStorageMode = DocStorage.InGrasshopperMemory;

          return base.Read(reader);
        }

        public override bool AppendMenuItems(ToolStripDropDown iMenu)
        {
          var toReturn = base.AppendMenuItems(iMenu);

          {
            var tsi = GetTargetVariableMenuItem();
            iMenu.Items.Insert(Math.Min(iMenu.Items.Count, 1), tsi);
          }

          return toReturn;
        }

        internal override void FixGhInput(Param_ScriptVariable i, bool alsoSetIfNecessary = true)
        {
          i.Name = string.Format("Variable {0}", i.NickName);
          i.Description = string.Format("Script Variable {0}", i.NickName);
          i.AllowTreeAccess = true;
          i.Optional = true;
          i.ShowHints = true;

          i.Hints = new List<IGH_TypeHint>();

          i.Hints.Add(new DynamicHint(this));
          i.Hints.AddRange(PossibleHints);
          i.Hints.AddRange(new IGH_TypeHint[]
            {
                new SpecialBoxHint(this), 
                new GH_HintSeparator(),
                new SpecialLineHint(this),
                new SpecialCircleHint(this),
                new SpecialArcHint(this),
                new SpecialPolylineHint(this),
            });
          i.Hints.AddRange(AlreadyGeometryBaseHints);

          if (alsoSetIfNecessary && i.TypeHint == null)
            i.TypeHint = i.Hints[0];
        }

        static IGH_TypeHint[] AlreadyGeometryBaseHints = 
        { 
            new GH_CurveHint(),
            new GH_SurfaceHint(), new GH_BrepHint(), new GH_MeshHint(),
            new GH_GeometryBaseHint()
        };
    }
}

#pragma warning restore 0618