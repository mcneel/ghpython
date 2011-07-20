using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GhPython.DocReplacement;
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
        DocStorage _storage = DocStorage.AutomaticMarshal;
        GrasshopperDocument _document;
        ComponentIOMarshal _marshal;
        PythonScript _py;
        PythonCompiledCode _compiled_py;
        string _previousRunCode;
        StringList _py_output = new StringList();
        PythonEnvironment _env;

        bool _hideCodeInput;
        string _codeInput;
        public string CodeInput
        {
            get
            {
                if (_hideCodeInput)
                    return _codeInput;
                else
                    return PythonComponent.ExtractCodeString((Param_String)Params.Input[0]);
            }
            set
            {
                if (!_hideCodeInput)
                    throw new InvalidOperationException("Cannot assign to CodeInput while parameter exists");
                _codeInput = value;
                _compiled_py = null;
            }
        }
        public Param_String CodeInputParam
        {
            get
            {
                if (!_hideCodeInput)
                    return (Param_String)Params.Input[0];
                return null;
            }
        }

        bool _hideOutOutput;

        internal const string DOCUMENT_NAME = "ghdoc";
        internal const string PARENT_ENVIRONMENT_NAME = "ghenv";

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
                _py.SetVariable("__name__", "__main__");
                _env = new PythonEnvironment(this, _py);
                _py.SetVariable(PARENT_ENVIRONMENT_NAME, _env);
                _py.SetIntellisenseVariable(PARENT_ENVIRONMENT_NAME, _env);

                // 22 May 2011 S. Baer
                // Use reflection to set context Id for now. Change in a couple weeks once
                // we feel confident people having updated to the latest WIP
                Type t = _py.GetType();
                var pi = t.GetProperty("ContextId");
                if (pi != null)
                  pi.SetValue(_py, 2, null);
            }
        }

        public Control CreateEditorControl(Action<string> helpCallback)
        {
            return (_py==null)? null : _py.CreateTextEditorControl("", helpCallback);
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            if (!_hideCodeInput)
            {
                pManager.RegisterParam(ConstructCodeInputParameter());
            }
            pManager.RegisterParam(ConstructVariable(GH_VarParamSide.Input, "x"));
            pManager.RegisterParam(ConstructVariable(GH_VarParamSide.Input, "y"));
        }

        private Param_String ConstructCodeInputParameter()
        {
            var code = new Param_String()
            {
                Name = "Code",
                NickName = "code",
                Description = "Python script to execute",
            };
            // Throw away the compiled script when code changes. We will just recompile on the next solve
            code.ObjectChanged += (sender, e) => { _compiled_py = null; };
            return code;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            if (!_hideOutOutput)
            {
                pManager.RegisterParam(ConstructOutOutputParam());
            }

            pManager.RegisterParam(ConstructVariable(GH_VarParamSide.Output, "a"));
        }

        private static Param_String ConstructOutOutputParam()
        {
            var outText = new Param_String()
            {
                Name = "Output",
                NickName = "out",
                Description = "The execution information, as output and error streams",
            };
            return outText;
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
                for (int i = _hideOutOutput ? 0 : 1; i < Params.Output.Count; i++)
                {
                    string varname = Params.Output[i].NickName;
                    _py.SetVariable(varname, null);
                }
                // Set all of the input variables. Even null variables may be used
                // in the script, so do not attempt to skip these for optimization
                // purposes.
                // First input parameter is the code itself, so we should skip that
                // Please pay attention to the input data structure type
                for (int i = _hideCodeInput ? 0 : 1; i < Params.Input.Count; i++)
                {
                    string varname = Params.Input[i].NickName;
                    object o = _marshal.GetInput(DA, i);
                    _py.SetVariable(varname, o);
                    _py.SetIntellisenseVariable(varname, o);
                }

                // the "code" string could either be embedded in the component
                // itself or a dynamic string that is input from some other component
                bool codeIsEmbedded = _hideCodeInput || Params.Input[0].SourceCount == 0;
                if (!codeIsEmbedded || _compiled_py == null)
                {
                    string script = CodeInput;

                    if (string.IsNullOrWhiteSpace(script))
                        throw new ApplicationException("Empty code parameter");

                    if (_compiled_py == null ||
                        string.Compare(script, _previousRunCode, StringComparison.InvariantCulture) != 0)
                    {
                        _compiled_py = _py.Compile(script);
                        _previousRunCode = script;
                    }
                }
              
                if (_compiled_py!=null )
                {
                    _compiled_py.Execute(_py);
                    // Python script completed, attempt to set all of the
                    // output paramerers
                    for (int i = _hideOutOutput? 0 : 1; i < Params.Output.Count; i++)
                    {
                        string varname = Params.Output[i].NickName;
                        object o = _py.GetVariable(varname);
                        _marshal.SetOutput(o, DA, i);
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
                SetFormErrorOrClearIt(DA, _py_output);
                throw;
            }
            finally
            {
                if ( rhdoc!=null && prevEnabled != rhdoc.Views.RedrawEnabled)
                    rhdoc.Views.RedrawEnabled = true;
            }
            SetFormErrorOrClearIt(DA, _py_output);
        }

        private void AddErrorNicely(StringList sw, Exception ex)
        {
            sw.Write(string.Format("Runtime error ({0}): {1}", ex.GetType().Name, ex.Message));

            string error = _py.GetStackTraceFromException(ex);

            error = error.Replace(", in <module>, \"<string>\"", ", in script");
            error = error.Trim();

            sw.Write(error);
        }

        private void SetFormErrorOrClearIt(IGH_DataAccess DA, StringList sl)
        {
            var attr = (PythonComponentAttributes)Attributes;

            if (sl.Result.Count > 0)
            {
                if(!_hideOutOutput)
                    DA.SetDataList(0, sl.Result);
                attr.TrySetLinkedFormHelpText(sl.GetResultAsOne());
            }
            else
            {
                 attr.TrySetLinkedFormHelpText("Execution completed successfully.");
            }
        }

        public static string ExtractCodeString(Param_String inputCode)
        {
            if (inputCode.VolatileDataCount > 0 && inputCode.VolatileData.PathCount > 0)
            {
                var goo = inputCode.VolatileData.get_Branch(0)[0] as IGH_Goo;
                if (goo != null)
                {
                    string code;
                    if (goo.CastTo(out code) && !string.IsNullOrEmpty(code))
                        return code;
                }
            }
            else if (inputCode.PersistentDataCount > 0) // here to handle the lock and disabled components
            {
                var stringData = inputCode.PersistentData[0];
                if (stringData != null && !string.IsNullOrEmpty(stringData.Value))
                {
                    return stringData.Value;
                }
            }
            return string.Empty;
        }


        private void SetScriptTransientGlobals()
        {
            switch (_storage)
            {
                case DocStorage.InGrasshopperMemory:
                case DocStorage.AutomaticMarshal:
                    {
                        _document = new GrasshopperDocument();
                        _py.ScriptContextDoc = _document;
                        _marshal = new ComponentIOMarshal(_document, this);
                        _py.SetVariable(DOCUMENT_NAME, _document);
                        _py.SetIntellisenseVariable(DOCUMENT_NAME, _document);
                        break;
                    }
                case DocStorage.InRhinoDoc:
                    {
                        _py.ScriptContextDoc = Rhino.RhinoDoc.ActiveDoc;
                        _marshal = new ComponentIOMarshal(Rhino.RhinoDoc.ActiveDoc, this);
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

            try
            {
                {
                    var tsi = GetTargetVariableMenuItem();
                    iMenu.Items.Insert(Math.Min(iMenu.Items.Count, 1), tsi);
                }

                // This should be uncommented when the hide option is ready...
                
                {
                    var tsi = new ToolStripMenuItem("&Presentation style", null, new ToolStripItem[]
                    {
                        new ToolStripMenuItem("Show \"code\" input parameter", GetCheckedImage(!_hideCodeInput), new TargetGroupToggler()
                            {
                                Component = this,
                                Params = Params.Input,
                                GetIsShowing = ()=>{return _hideCodeInput;},
                                SetIsShowing = (value)=>{_hideCodeInput = value;},
                                Side = GH_VarParamSide.Input,
                            }.Toggle)
                        {
                             ToolTipText = string.Format("Code input is {0}. Click to {1} it.", _hideCodeInput ? "hidden" : "shown", _hideCodeInput ? "show" : "hide"),
                        },
                        new ToolStripMenuItem("Show output \"out\" parameter", GetCheckedImage(!_hideOutOutput), new TargetGroupToggler()
                            {
                                Component = this,
                                Params = Params.Output,
                                GetIsShowing = ()=>{return _hideOutOutput;},
                                SetIsShowing = (value)=>{_hideOutOutput = value;},
                                Side = GH_VarParamSide.Output,
                            }.Toggle)
                        {
                             ToolTipText = string.Format("Print output is {0}. Click to {1} it.", _hideCodeInput ? "hidden" : "shown", _hideCodeInput ? "show" : "hide"),
                             Height = 32,
                        }
                    });

                    iMenu.Items.Insert(Math.Min(iMenu.Items.Count, 2), tsi);
                }
                

                {
                    var tsi = new ToolStripMenuItem("&Open editor...", null, (sender, e) =>
                    {
                        var attr = Attributes as PythonComponentAttributes;
                        if (attr != null)
                            attr.OpenEditor();
                    });
                    tsi.Font = new Font(tsi.Font, FontStyle.Bold);

                    if (Locked) tsi.Enabled = false;

                    iMenu.Items.Insert(Math.Min(iMenu.Items.Count, 3), tsi);
                }

                iMenu.Items.Insert(Math.Min(iMenu.Items.Count, 4), new ToolStripSeparator());

            }
            catch (Exception ex)
            {
                GhPython.Forms.PythonScriptForm.LastHandleException(ex);
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
                result.DropDownItems[0].Image = GetCheckedImage(_storage == DocStorage.AutomaticMarshal);
                result.DropDownItems[1].Image = GetCheckedImage(_storage == DocStorage.InGrasshopperMemory);
                result.DropDownItems[2].Image = GetCheckedImage(_storage == DocStorage.InRhinoDoc);
            };
            update(null, EventArgs.Empty);
            result.DropDownOpening += update;

            return result;
        }

        internal DocStorage DocStorageMode
        {
            get
            {
                return _storage;
            }
        }

        class DocSetter
        {
            public DocStorage NewDocStorage;
            public PythonComponent Component;

            public void SetDoc(object sender, EventArgs e)
            {
                try
                {
                    Component.CheckAndSetupActions();

                    Component._storage = NewDocStorage;
                    Component.SetScriptTransientGlobals();
                    Component.ExpireSolution(true);
                }
                catch (Exception ex)
                {
                    GhPython.Forms.PythonScriptForm.LastHandleException(ex);
                }
            }

        }

        class TargetGroupToggler
        {
            public PythonComponent Component { get; set; }
            public List<IGH_Param> Params { get; set; }
            public Func<bool> GetIsShowing { get; set; }
            public Action<bool> SetIsShowing { get; set; }
            public GH_VarParamSide Side { get; set; }

            public void Toggle(object sender, EventArgs e)
            {
                try
                {
                    var code = Component.CodeInput;

                    SetIsShowing(!GetIsShowing());

                    bool recompute = false;

                    if (GetIsShowing())
                    {
                        Component.Params.UnregisterParameter(Params[0]);

                        if (Side == GH_VarParamSide.Input)
                            Component.CodeInput = code;
                    }
                    else
                    {
                        Param_String param;
                        if (Side == GH_VarParamSide.Input)
                        {

                            param = Component.ConstructCodeInputParameter();
                            Component.Params.RegisterInputParam(param, 0);
                            param.AddPersistentData(new GH_String(code));
                            recompute = true;
                        }
                        else
                        {
                            param = ConstructOutOutputParam();
                            Component.Params.RegisterOutputParam(param, 0);
                        }
                    }

                    Component.Params.OnParametersChanged();
                    Component.OnDisplayExpired(true);

                    if (recompute) Component.ExpireSolution(true);
                }
                catch (Exception ex)
                {
                    GhPython.Forms.PythonScriptForm.LastHandleException(ex);
                }
            }
        }

        const string TargetDocIdentifier = "GhMemory";
        const string HideInputIdentifier = "HideInput";
        const string CodeInputIdentifier = "CodeInput";
        const string HideOutputIdentifier = "HideOutput";

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            var toReturn = base.Write(writer);

            if (!Enum.IsDefined(typeof(DocStorage), _storage))
                _storage = DocStorage.InGrasshopperMemory;
            writer.SetInt32(TargetDocIdentifier, (int)_storage);

            writer.SetBoolean(HideInputIdentifier, _hideCodeInput);
            if (_hideCodeInput)
                writer.SetString(CodeInputIdentifier, CodeInput);
            
            writer.SetBoolean(HideOutputIdentifier, _hideOutOutput);

            return toReturn;
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            {
                var hideInput = false;
                if (reader.TryGetBoolean(HideInputIdentifier, ref hideInput))
                    _hideCodeInput = hideInput;
            }

            if (_hideCodeInput)
            {
                string code = null;
                if (reader.TryGetString(CodeInputIdentifier, ref code))
                    CodeInput = code;
            }

            {
                var hideOutput = false;
                if (reader.TryGetBoolean(HideOutputIdentifier, ref hideOutput))
                    _hideOutOutput = hideOutput;
            }

            int val = -1;
            if (reader.TryGetInt32(TargetDocIdentifier, ref val))
                _storage = (DocStorage)val;

            if (!Enum.IsDefined(typeof(DocStorage), _storage))
                _storage = DocStorage.InGrasshopperMemory;

            if (_hideCodeInput)
                Params.Input.RemoveAt(0);
            if (_hideOutOutput)
                Params.Output.RemoveAt(0);

            var toReturn = base.Read(reader);

            // Dynamic input fix for existing scripts
            // Always assign DynamicHint or Grasshopper
            // will set Line and not LineCurve, etc...
            if (Params != null && Params.Input != null)
                for (int i = _hideCodeInput ? 0 : 1; i < Params.Input.Count; i++)
                {
                    var p = Params.Input[i] as Param_ScriptVariable;
                    if (p != null && p.TypeHint == null)
                    {
                        p.TypeHint = p.Hints[0];
                    }
                }

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
                    Doc.SolutionEnd -= OnDocSolutionEnd;

                var attr = Attributes as PythonComponentAttributes;
                if (attr != null)
                    attr.DisableLinkedForm(true);
            }
        }

        protected override string HelpDescription
        {
            get
            {
                return Resources.helpText;
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
            return e.Index > (_hideCodeInput? -1 : 0);
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

        static IGH_TypeHint[] PossibleHints = 
        { 
            new GH_HintSeparator(),
            new GH_BooleanHint_CS(),new GH_IntegerHint_CS(), new GH_DoubleHint_CS(), new GH_ComplexHint(),
            new GH_StringHint_CS(), new GH_DateTimeHint(), new GH_ColorHint(), new GH_GuidHint(),
            new GH_HintSeparator(),
            new GH_Point3dHint(),
            new GH_Vector3dHint(), new GH_PlaneHint(), new GH_IntervalHint(),
            new GH_UVIntervalHint()
        };

        static IGH_TypeHint[] AlreadyGeometryBaseHints = 
        { 
            new GH_CurveHint(),
            new GH_SurfaceHint(), new GH_BrepHint(), new GH_MeshHint(),
            new GH_GeometryBaseHint()
        };

        void FixGhInput(Param_ScriptVariable i)
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
            i.TypeHint = i.Hints[0];
        }

        #endregion
    }
}
