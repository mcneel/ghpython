using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GhPython.DocReplacement;
using GhPython.Infrastructure;
using GhPython.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Runtime;

namespace GhPython.Component
{
    public class PythonComponent : SafeComponent, IGH_VarParamComponent
    {
        DocStorage _storage;
        GrasshopperDocument _document;
        PythonScript _py;
        PythonCompiledCode _compiled_py;
        string _previousRunCode;
        StringList _py_output = new StringList();

        const string DOCUMENT_NAME = "ghdoc";

        public PythonComponent()
            : base("Python Script", "Python", "A python scriptable component", "Math", "Script")
        {
        }

        public override void CreateAttributes()
        {
            this.Attributes = new PythonComponentAttributes(this);
        }

        protected override void Initialize()
        {
            base.Initialize();

            if(Doc != null)
                Doc.SolutionEnd += OnDocSolutionEnd;

            _py = PythonScript.Create();
            if (_py != null)
            {
                SetScriptTransientGlobals();
                _py.Output = _py_output.Write;
            }
        }

        public Control CreateEditorControl(Action<string> helpCallback)
        {
            return (_py==null)? null : _py.CreateTextEditorControl("", helpCallback);
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            var codeparam = new Param_String();
            codeparam.Name = "Code";
            codeparam.NickName = "code";
            codeparam.Description = "Python script to execute";
            codeparam.ObjectChanged += new IGH_DocumentObject.ObjectChangedEventHandler(OnCodeChanged);
            pManager.RegisterParam(codeparam);

            pManager.RegisterParam(ConstructVariable(GH_VarParamSide.Input, "x"));
            pManager.RegisterParam(ConstructVariable(GH_VarParamSide.Input, "y"));
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Output", "out", "The execution information, as output and error streams");

            pManager.RegisterParam(ConstructVariable(GH_VarParamSide.Output, "a"));
        }
        
        void OnCodeChanged(IGH_DocumentObject sender, GH_ObjectChangedEventArgs e)
        {
            // Throw away the compiled script. We will just recompile on the next solve
            _compiled_py = null;
        }

        protected override void SafeSolveInstance(IGH_DataAccess DA)
        {
            if (_py == null)
            {
                DA.SetData(0, "No Python engine available. This component needs Rhino v5");
                return;
            }

            DA.DisableGapLogic(0);

            _py_output.Reset();

            var rhdoc = RhinoDoc.ActiveDoc;
            var prevEnabled = (rhdoc == null) ? false : rhdoc.Views.RedrawEnabled;

            try
            {
                // clear all of the output variables
                for (int i = 1; i < Params.Output.Count; i++)
                {
                    string varname = Params.Output[i].NickName;
                    _py.SetVariable(varname, null);
                }
                // Set all of the input variables. Even null variables may be used
                // in the script, so do not attempt to skip these for optimization
                // purposes.
                // First input parameter is the code itself, so we should skip that
                // Please pay attention to the input data structure type
                for (int i = 1; i < Params.Input.Count; i++)
                {
                    string varname = Params.Input[i].NickName;
                    object o;
                    switch (Params.Input[i].Access)
                    {
                        case GH_ParamAccess.item:
                            o = GetItemFromParameter(DA, i);
                            break;

                        case GH_ParamAccess.list:
                            o = GetListFromParameter(DA, i);
                            break;

                        case GH_ParamAccess.tree:
                            o = GetTreeFromParameter(DA, i);
                            break;

                        default:
                            throw new ApplicationException("Wrong parameter in variable access type");
                    }
                    _py.SetVariable(varname, o);
                }

                string script = null;
                if (!DA.GetData(0, ref script))
                    throw new ApplicationException("Impossible to retrive code to execute");

                if (_compiled_py == null ||
                    string.Compare(script, _previousRunCode, StringComparison.InvariantCulture) != 0)
                {
                    _compiled_py = _py.Compile(script);
                    _previousRunCode = script;
                }
              
                if (_compiled_py!=null )
                {
                    _compiled_py.Execute(_py);
                    // Python script completed, attempt to set all of the
                    // output paramerers
                    for (int i=1; i<Params.Output.Count; i++ )
                    {
                        string varname = Params.Output[i].NickName;
                        object o = _py.GetVariable(varname);
                        ReadOneOutput(DA, o, i);
                    }
                }
                else
                {
                    _py_output.Write("There was a permanent error parsing this script. Please report to giulio@mcneel.com.");
                }
            }
            catch (Exception ex)
            {
                AddErrorNicely(_py_output, ex);
                SetErrorOrClearIt(DA, _py_output);
                throw;
            }
            finally
            {
                if ( rhdoc!=null && prevEnabled != rhdoc.Views.RedrawEnabled)
                    rhdoc.Views.RedrawEnabled = true;
            }
            SetErrorOrClearIt(DA, _py_output);
        }

        private void SetErrorOrClearIt(IGH_DataAccess DA, StringList sl)
        {
            var attr = Attributes as PythonComponentAttributes;

            if (sl.Result.Count > 0)
            {
                DA.SetDataList(0, sl.Result);

                if (attr != null)
                    attr.TrySetLinkedFormHelpText(sl.GetResultAsOne());
            }
            else
            {
                if (attr != null)
                    attr.TrySetLinkedFormHelpText("Execution completed successfully.");
            }
        }

        private void AddErrorNicely(StringList sw, Exception ex)
        {
            sw.Write(string.Format("Runtime error ({0}): {1}", ex.GetType().Name, ex.Message));

            string error = _py.GetStackTraceFromException(ex);
            if (error.Contains("File \"\", line"))
                error = error.Replace("File \"\", line", "line");
            error = error.Trim();

            sw.Write(error);
        }

        private object GetItemFromParameter(IGH_DataAccess DA, int index)
        {
            IGH_Goo destination = null;
            DA.GetData<IGH_Goo>(index, ref destination);
            return this.TypeCast(destination, index);
        }

        private object GetListFromParameter(IGH_DataAccess DA, int index)
        {
            List<IGH_Goo> list2 = new List<IGH_Goo>();
            DA.GetDataList<IGH_Goo>(index, list2);
            IGH_TypeHint typeHint = ((Param_ScriptVariable)this.Params.Input[index]).TypeHint;
            List<object> list = new List<object>();

            for (int i = 0; i < list2.Count; i++)
            {
                list.Add(this.TypeCast(list2[i], typeHint));
            }
            return list;
        }

        private object GetTreeFromParameter(IGH_DataAccess DA, int index)
        {
            GH_Structure<IGH_Goo> structure = new GH_Structure<IGH_Goo>();
            DA.GetDataTree<IGH_Goo>(index, out structure);
            IGH_TypeHint typeHint = ((Param_ScriptVariable)this.Params.Input[index]).TypeHint;
            DataTree<object> tree = new DataTree<object>();

            for (int i = 0; i < structure.PathCount; i++)
            {
                GH_Path path = structure.get_Path(i);
                List<IGH_Goo> list = structure.Branches[i];
                List<object> data = new List<object>();

                for (int j = 0; j < list.Count; j++)
                {
                    data.Add(this.TypeCast(list[j], typeHint));
                }
                tree.AddRange(data, path);
            }
            return tree;
        }

        private object TypeCast(IGH_Goo data, int index)
        {
            Param_ScriptVariable variable = (Param_ScriptVariable)this.Params.Input[index];
            return this.TypeCast(data, variable.TypeHint);
        }

        private object TypeCast(IGH_Goo data, IGH_TypeHint hint)
        {
            if (data == null)
            {
                return null;
            }
            if (hint == null)
            {
                return data.ScriptVariable();
            }
            object objectValue = data.ScriptVariable();
            object target = null;
            hint.Cast(objectValue, out target);
            return target;
        }

        private void ReadOneOutput(IGH_DataAccess DA, object o, int loc)
        {
            if (o == null)
                return;

            if (o is GrasshopperDocument)
            {
                var ogh = o as GrasshopperDocument;
                DA.SetDataList(loc, ogh.Objects.Geometries);
                ogh.Objects.Clear();
            }
            else if (o is string) //string is IEnumerable, so we need to check first
            {
                DA.SetData(loc, o);
            }
            else if (o is IEnumerable)
            {
                DA.SetDataList(loc, o as IEnumerable);
            }
            else if (o is IGH_DataTree)
            {
                DA.SetDataTree(loc, o as IGH_DataTree);
            }
            else
            {
                DA.SetData(loc, o);
            }
        }

        private void SetScriptTransientGlobals()
        {
            if (_storage == DocStorage.InGrasshopperMemory)
            {
                _document = new GrasshopperDocument();
                _py.ScriptContextDoc = _document;
                _py.SetVariable(DOCUMENT_NAME, _document);
                _py.SetIntellisenseVariable(DOCUMENT_NAME, _document);              
            }
            else if (_storage == DocStorage.InRhinoDoc)
            {
                _py.ScriptContextDoc = Rhino.RhinoDoc.ActiveDoc;
                Rhino.RhinoDoc.ActiveDoc.UndoRecordingEnabled = true;
                if (_py.ContainsVariable(DOCUMENT_NAME))
                {
                    _py.RemoveVariable(DOCUMENT_NAME);
                    _py.SetIntellisenseVariable(DOCUMENT_NAME, null);
                }
            }
            else if (_storage == DocStorage.None)
            {
                _py.ScriptContextDoc = new object();
                if (_py.ContainsVariable(DOCUMENT_NAME))
                {
                    _py.RemoveVariable(DOCUMENT_NAME);
                    _py.SetIntellisenseVariable(DOCUMENT_NAME, null);
                }
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{CEAB6E56-CEEC-A646-84D5-363C57440969}"); }
        }

        protected override Bitmap Icon
        {
            get { return Resources.python; }
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        public override void Menu_AppendDerivedItems(ToolStripDropDown iMenu)
        {
            base.Menu_AppendDerivedItems(iMenu);
            var ti = GetTargetVariableMenuItem();
            iMenu.Items.Insert(Math.Min(iMenu.Items.Count, 1), ti);
        }

        public ToolStripMenuItem GetTargetVariableMenuItem()
        {
            var result = new ToolStripMenuItem("&Target for rhinoscriptsyntax", null, new ToolStripItem[]
            {
                new ToolStripMenuItem("In &" + DOCUMENT_NAME + " variable", null, SetPythonDocAsGhMem)
                {
                     ToolTipText = "Use this option to obtain the " + DOCUMENT_NAME + " variable in your script\nand be able to assign it to the outputs",
                },
                new ToolStripMenuItem("In &standard Rhino document", null, SetPythonDocAsDoc)
                {
                     ToolTipText = "Use this option to choose to use the traditional Rhino document as output",
                },
                new ToolStripMenuItem("&No document", null, SetPythonDocAsNone)
                {
                     ToolTipText = "Use this option if you do not wish to use rhinoscriptsyntax functions, but only RhinoCommon",
                },
            })
            {
                ToolTipText = "Choose where rhinoscriptsyntax functions have their effects",
            };

            EventHandler update = (sender, args) =>
            {
                result.DropDownItems[0].Image = GetCheckedImage(_storage == DocStorage.InGrasshopperMemory);
                result.DropDownItems[1].Image = GetCheckedImage(_storage == DocStorage.InRhinoDoc);
                result.DropDownItems[2].Image = GetCheckedImage(_storage == DocStorage.None);
            };
            update(null, EventArgs.Empty);
            result.DropDownOpening += update;

            return result;
        }

        private void SetPythonDocAsDoc(object sender, EventArgs e)
        {
            _storage = DocStorage.InRhinoDoc;
            SetScriptTransientGlobals();
            this.ExpireSolution(true);
        }

        private void SetPythonDocAsGhMem(object sender, EventArgs e)
        {
            _storage = DocStorage.InGrasshopperMemory;
            SetScriptTransientGlobals();
            this.ExpireSolution(true);
        }

        private void SetPythonDocAsNone(object sender, EventArgs e)
        {
            _storage = DocStorage.None;
            SetScriptTransientGlobals();
            this.ExpireSolution(true);
        }

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            var toReturn = base.Write(writer);

            writer.SetInt32("GhMemory", (int)_storage);

            return toReturn;
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            var toReturn = base.Read(reader);

            int val = -1;
            if (reader.TryGetInt32("GhMemory", ref val))
                _storage = (DocStorage)val;
            else
                _storage = DocStorage.InGrasshopperMemory;

            return toReturn;
        }

        private static Bitmap GetCheckedImage(bool check)
        {
            return check? Resources._checked: Resources._unchecked;
        }

        protected override void OnLockedChanged(bool nowIsLocked)
        {
            base.OnLockedChanged(nowIsLocked);
        }

        void OnDocSolutionEnd(object sender, GH_SolutionEventArgs e)
        {
            if (_storage == DocStorage.InGrasshopperMemory)
            {
                GrasshopperDocument ghd = _document as GrasshopperDocument;

                if (ghd != null)
                    ghd.Objects.Clear();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (Doc != null)
                {
                    Doc.SolutionEnd -= OnDocSolutionEnd;
                }

                PythonComponentAttributes attr = Attributes as PythonComponentAttributes;
                if (attr != null)
                {
                    attr.DisableLinkedForm(true);
                }
            }
        }


        //------------------------------------------------------------------------------------------
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
            return e.Index != 0;
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

        static void FixGhInput(Param_ScriptVariable i)
        {
            i.Name = string.Format("Variable {0}", i.NickName);
            i.Description = string.Format("Script Variable {0}", i.NickName);
            i.AllowTreeAccess = true;
            i.Optional = true;
            i.ShowHints = true;

            i.Hints = new List<IGH_TypeHint> { 
            new DynamicHint(), new GH_HintSeparator(), new GH_BooleanHint_CS(),
            new GH_IntegerHint_CS(), new GH_DoubleHint_CS(), new GH_ComplexHint(),
            new GH_StringHint_CS(), new GH_DateTimeHint(), new GH_ColorHint(),
            new GH_GuidHint(), new GH_HintSeparator(), new GH_Point3dHint(),
            new GH_Vector3dHint(), new GH_PlaneHint(), new GH_IntervalHint(),
            new GH_UVIntervalHint(), new GH_BoxHint(), new GH_HintSeparator(),
            new GH_LineHint(), new GH_CircleHint(), new GH_ArcHint(),
            new GH_PolylineHint(), new GH_CurveHint(), new GH_SurfaceHint(),
            new GH_BrepHint(), new GH_MeshHint(), new GH_GeometryBaseHint()
            };
        }

        #endregion
    }
}
