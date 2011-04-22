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

        const string INPUTS_NAME = "inputs", OUTPUTS_NAME = "outputs", DOCUMENT_NAME = "doc";

        public PythonComponent()
            : base("Python Interpreter", "Python", "A small pythonic interpreter", "Math", "Script")
        {
        }

        public override void CreateAttributes()
        {
            //base.CreateAttributes();
            this.Attributes = new PythonComponentAttributes(this);
        }

        protected override void Initialize()
        {
            base.Initialize();

            if(Doc != null)
                Doc.SolutionEnd += OnDocSolutionEnd;

            _py = PythonScript.Create();
            if (_py != null)
                SetScriptTransientGlobals();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_StringParam("Code", "code", "This is the python code");

            var t = new Param_ScriptVariable();
            t.NickName = "i";
            FixGhInput(t);
            pManager.RegisterParam(t);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Output", "out", "The execution information, as error and info streams");
            pManager.Register_GenericParam("Result", "a", "The result(s)");
        }

        protected override void SafeSolveInstance(IGH_DataAccess DA)
        {
            if (_py == null)
            {
                DA.SetData(0, "No Python engine available. This component needs Rhino v5");
                return;
            }

            DA.DisableGapLogic(0);

            string s = null;
            StringList sl = new StringList();
            _py.Output = sl.Write;

            string inputName, outputName;
            SetScriptOutputGlobals(DA, out outputName);

            var prevEnambled = Rhino.RhinoDoc.ActiveDoc.Views.RedrawEnabled;
            RhinoDoc.ActiveDoc.Views.RedrawEnabled = false;

            try
            {
                object input = FillInputVariables(DA);
                SetScriptInputGlobals(input, out inputName);

                if (!DA.GetData(0, ref s))
                    throw new ApplicationException("Impossible to retrive code to execute");

                if (_py.ExecuteScript(s))
                {
                    RetrieveOutput(DA, sl, outputName);

                }
                else
                {
                    sl.Write("There was a permanent error parsing this script. Please report to giulio@mcneel.com.");
                }
            }
            catch (Exception ex)
            {
                AddErrorNicely(sl, ex);
                SetErrorOrClearIt(DA, sl);
                throw;
            }
            finally
            {
                if (prevEnambled != RhinoDoc.ActiveDoc.Views.RedrawEnabled)
                    RhinoDoc.ActiveDoc.Views.RedrawEnabled = true;
            }
            SetErrorOrClearIt(DA, sl);
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

        private object FillInputVariables(IGH_DataAccess DA)
        {
            var input = new DynamicTextList(Params.Input.Count - 1);

            for (int i = 1; i < Params.Input.Count; i++)
            {
                switch (Params.Input[i].Access)
                {
                    case GH_ParamAccess.item:
                        input.Add(Params.Input[i].NickName, GetItemFromParameter(DA, i));
                        break;

                    case GH_ParamAccess.list:
                        input.Add(Params.Input[i].NickName, GetListFromParameter(DA, i));
                        break;

                    case GH_ParamAccess.tree:
                        input.Add(Params.Input[i].NickName, GetTreeFromParameter(DA, i));
                        break;

                    default:
                        throw new ApplicationException("Wrong parameter in variable access type");
                }
            }
            input.Close();
            return input;
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

        private void RetrieveOutput(IGH_DataAccess DA, StringList sw, string outputName)
        {
            object o = _py.GetVariable(outputName);

            if (o is DynamicTextList)
            {
                var lo = (DynamicTextList)o;
                for (int i = 0; i < lo.Count; i++)
                {
                    ReadOneOutput(DA, sw, outputName, lo[i], i + 1);
                }
            }
            else //the user overrode the whole variable. We can still rescue it...
            {
                if (Params.Output.Count > 1)
                {
                    if (Params.Output.Count > 2) //Warn
                        sw.Write(
                            string.Format("The \"{0}\" variable was overridden with a single result, but it accepted many. You can set the first input with \"{0}.{1} = expr\", the second with \"{0}.{2} = expr\"",
                            OUTPUTS_NAME, Params.Output[1].NickName, Params.Output[2].NickName));
                    ReadOneOutput(DA, sw, outputName, o, 1);
                }
                else
                {
                    //Maybe not. Error
                    throw new ArgumentException("There is no output to set the results... Right click to add one.");
                }
            }
        }

        private void ReadOneOutput(IGH_DataAccess DA, StringList sw, string outputName, object o, int loc)
        {
            if (o == null)
            {
                sw.Write(string.Format("There are no results in {2}: \"{0}\" is NoneType.\r\n\r\nPlease use\r\n{1}[\"{2}\"] = something\r\nto return geometry.", outputName, OUTPUTS_NAME, Params.Output.Count > 1 ? Params.Output[loc].NickName : "Name"));
            }
            else if (o is GrasshopperDocument)
            {
                DA.SetDataList(loc, (o as GrasshopperDocument).Objects.Geometries);
                (o as GrasshopperDocument).Objects.Clear();
            }
            else if (o is string) //string is IEnumerable, so we need to check first
            {
                DA.SetData(loc, o);
            }
            else if (o is IEnumerable)
            {
                DA.SetDataList(loc, o as IEnumerable);
            }
            else
            {
                DA.SetData(loc, o);
            }
        }

        private void SetScriptOutputGlobals(IGH_DataAccess DA, out string outputName)
        {
            outputName = OUTPUTS_NAME;

            var list = new DynamicTextList(this.Params.Output.Count - 1);
            for (int i = 1; i < Params.Output.Count; i++)
            {
                list.Add(Params.Output[i].NickName, null);
            }

            _py.SetVariable(outputName, list);
        }

        private void SetScriptInputGlobals(object val, out string inputName)
        {
            inputName = INPUTS_NAME;
            _py.SetVariable(inputName, val);
        }

        private void SetScriptTransientGlobals()
        {
            if (_storage == DocStorage.InGrasshopperMemory)
            {
                _document = new GrasshopperDocument();
                _py.ScriptContextDoc = _document;
                _py.SetVariable(DOCUMENT_NAME, _document);
            }
            else if (_storage == DocStorage.InRhinoDoc)
            {
                _py.ScriptContextDoc = Rhino.RhinoDoc.ActiveDoc;
                Rhino.RhinoDoc.ActiveDoc.UndoRecordingEnabled = true;
                if (_py.ContainsVariable(DOCUMENT_NAME))
                {
                    _py.RemoveVariable(DOCUMENT_NAME);
                }
            }
            else if (_storage == DocStorage.None)
            {
                _py.ScriptContextDoc = new object();
                if (_py.ContainsVariable(DOCUMENT_NAME))
                {
                    _py.RemoveVariable(DOCUMENT_NAME);
                }
            }
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{CEAB6E56-CEEC-A646-84D5-363C57440969}");
            }
        }

        protected override Bitmap Icon
        {
            get
            {
                return Resources.python;
            }
        }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.secondary;
            }
        }

        public override void Menu_AppendDerivedItems(ToolStripDropDown iMenu)
        {
            base.Menu_AppendDerivedItems(iMenu);

            ToolStripMenuItem ti = new ToolStripMenuItem("RhinoScript target", null,
                new ToolStripItem[]
            {
                new ToolStripMenuItem("In " + DOCUMENT_NAME + " variable", GetCheckedImage(_storage == DocStorage.InGrasshopperMemory), SetPythonDocAsGhMem)
                {
                     ToolTipText = 
                     string.Format("Use this option to obtain the " + DOCUMENT_NAME + " variable in your script\nand be able to assign it to the outputs")
                },
                new ToolStripMenuItem("In standard Rhino document", GetCheckedImage(_storage == DocStorage.InRhinoDoc), SetPythonDocAsDoc)
                {
                     ToolTipText = "Use this option to choose to use the traditional Rhino document as output"
                },
                new ToolStripMenuItem("No document", GetCheckedImage(_storage == DocStorage.None), SetPythonDocAsNone)
                {
                     ToolTipText = "Use this option if you do not wish to use RhinoScript methods, but only RhinoCommon"
                },
            })
            {
                ToolTipText = "Choose where RhinoScript functions have their effects"
            };

            iMenu.Items.Insert(Math.Min(iMenu.Items.Count, 1), ti);
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
            if (check)
                return Resources._checked;
            else
                return Resources._unchecked;
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
                    attr.CloseLinkedForm();
                }
            }
        }


        //------------------------------------------------------------------------------------------
        #region Members of IGH_VarParamComponent

        public IGH_Param ConstructVariable(GH_VarParamEventArgs e)
        {
            if (e.Side == GH_VarParamSide.Input)
            {
                Param_ScriptVariable p = new Param_ScriptVariable();
                FixGhInput(p);
                return p;
            }
            if (e.Side == GH_VarParamSide.Output)
            {
                Param_GenericObject p = new Param_GenericObject();
                p.Name = p.NickName;
                p.Description = string.Format("Variable {0}", p.NickName);
                return p;
            }
            return null;
        }

        public bool IsInputVariable
        {
            get
            {
                return true;
            }
        }

        public bool IsOutputVariable
        {
            get
            {
                return true;
            }
        }

        public bool IsVariableParam(GH_VarParamEventArgs e)
        {
            return e.Index != 0;
        }

        public void ManagerConstructed(GH_VarParamSide side, Grasshopper.GUI.GH_VariableParameterManager manager)
        {
            switch (side)
            {
                case GH_VarParamSide.Input:
                    {
                        ExtendedRomanNumeralsConstructor constructor = new ExtendedRomanNumeralsConstructor();
                        manager.NameConstructor = constructor;
                        break;
                    }
                case GH_VarParamSide.Output:
                    {
                        GH_CharPatternParamNameConstructor constructor2 = new GH_CharPatternParamNameConstructor
                        {
                            StackDepth = 4
                        };
                        constructor2.SetCharPool("abcdefghijklmnopqrstuvwxyz");
                        manager.NameConstructor = constructor2;
                        break;
                    }
            }
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

        private static void FixGhInput(Param_ScriptVariable i)
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