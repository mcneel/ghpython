using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GhPython.DocReplacement;
using GhPython.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Runtime;

namespace GhPython.Component
{
  public abstract class ScriptingAncestorComponent : SafeComponent
  {
    static internal GrasshopperDocument _document = new GrasshopperDocument();
    internal ComponentIOMarshal _marshal;
    protected PythonScript _py;
    PythonCompiledCode _compiled_py;
    protected string _previousRunCode;
    internal StringList _py_output = new StringList();
    protected PythonEnvironment _env;
    bool _inDocStringsMode = false;

    // The component defaults per se to having a code input, but if necessary this can be removed
    // and the HideCodeInput property can be set to the appropriate value.
    public bool HideCodeInput { get; set; }

    string _codeInput;
    public string CodeInput
    {
      get
      {
        if (HideCodeInput)
          return _codeInput;
        else
          return ScriptingAncestorComponent.ExtractCodeString((Param_String)Params.Input[0]);
      }
      set
      {
        if (!HideCodeInput)
          throw new InvalidOperationException("Cannot assign to CodeInput while parameter exists");
        _codeInput = value;
        _compiled_py = null;
      }
    }
    public Param_String CodeInputParam
    {
      get
      {
        if (!HideCodeInput)
          return (Param_String)Params.Input[0];
        return null;
      }
    }

    public bool HideCodeOutput { get; set; }

    internal const string DOCUMENT_NAME = "ghdoc";
    internal const string PARENT_ENVIRONMENT_NAME = "ghenv";
    internal const string DESCRIPTION = "A python scriptable component";

    public ScriptingAncestorComponent()
      : base("Python Script", "Python", DESCRIPTION, "Math", "Script")
    {
    }

    public override void CreateAttributes()
    {
      this.Attributes = new PythonComponentAttributes(this);
    }

    protected override void Initialize()
    {
      base.Initialize();

      if (Doc != null)
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

        _py.ContextId = 2; // 2 is Grasshopper
      }
    }

    public Control CreateEditorControl(Action<string> helpCallback)
    {
      return (_py == null) ? null : _py.CreateTextEditorControl("", helpCallback);
    }

    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
      if (!HideCodeInput)
      {
        pManager.RegisterParam(ConstructCodeInputParameter());
      }
      AddDefaultInput(pManager);
    }

    protected abstract void AddDefaultInput(GH_Component.GH_InputParamManager pManager);

    public Param_String ConstructCodeInputParameter()
    {
      var code = new Param_String()
      {
        Name = "code",
        NickName = "code",
        Description = "Python script to execute",
      };
      return code;
    }

    protected abstract void AddDefaultOutput(GH_Component.GH_OutputParamManager pManager);

    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
      if (!HideCodeOutput)
      {
        pManager.RegisterParam(ConstructOutOutputParam());
      }

      AddDefaultOutput(pManager);
      VariableParameterMaintenance();
    }

    public abstract void VariableParameterMaintenance();

    private static Param_String ConstructOutOutputParam()
    {
      var outText = new Param_String()
      {
        Name = "out",
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
        for (int i = HideCodeOutput ? 0 : 1; i < Params.Output.Count; i++)
        {
          string varname = Params.Output[i].NickName;
          _py.SetVariable(varname, null);
        }
        // Set all of the input variables. Even null variables may be used
        // in the script, so do not attempt to skip these for optimization
        // purposes.
        // First input parameter is the code itself, so we should skip that
        // Please pay attention to the input data structure type
        for (int i = HideCodeInput ? 0 : 1; i < Params.Input.Count; i++)
        {
          string varname = Params.Input[i].NickName;
          object o = _marshal.GetInput(DA, i);
          _py.SetVariable(varname, o);
          _py.SetIntellisenseVariable(varname, o);
        }

        // the "code" string could be embedded in the component itself
        if (!HideCodeInput || _compiled_py == null)
        {
          string script;
          if (HideCodeInput)
            script = CodeInput;
          else
          {
            script = null;
            DA.GetData(0, ref script);
          }

          if (string.IsNullOrWhiteSpace(script))
          {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No Python code is ");
            return;
          }

          if (_compiled_py == null ||
              string.Compare(script, _previousRunCode, StringComparison.InvariantCulture) != 0)
          {
            if (!(_inDocStringsMode = DocStringUtils.FindApplyDocString(script, this)))
              ResetAllDescriptions();
            _compiled_py = _py.Compile(script);
            _previousRunCode = script;
          }
        }

        if (_compiled_py != null)
        {
          _compiled_py.Execute(_py);
          // Python script completed, attempt to set all of the
          // output paramerers
          for (int i = HideCodeOutput ? 0 : 1; i < Params.Output.Count; i++)
          {
            string varname = Params.Output[i].NickName;
            object o = _py.GetVariable(varname);
            _marshal.SetOutput(o, DA, i);
          }
        }
        else
        {
          _py_output.Write("There was a permanent error parsing this script. Please report to steve@mcneel.com.");
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
        if (rhdoc != null && prevEnabled != rhdoc.Views.RedrawEnabled)
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
        if (!HideCodeOutput)
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
        var flat_list = inputCode.PersistentData.FlattenData();
        if (flat_list != null && flat_list.Count > 0)
        {
          var stringData = flat_list[0];
          if (stringData != null && !string.IsNullOrEmpty(stringData.Value))
          {
            return stringData.Value;
          }
        }
      }
      return string.Empty;
    }

    private void ResetAllDescriptions()
    {
      Description = DESCRIPTION;

      for (int i = HideCodeInput ? 0 : 1; i < Params.Input.Count; i++)
      {
        Params.Input[i].Description = "Script input " + Params.Input[i].NickName + ".";
      }
      for (int i = HideCodeOutput ? 0 : 1; i < Params.Output.Count; i++)
      {
        Params.Output[i].Description = "Script output " + Params.Output[i].NickName + ".";
      }

      SpecialPythonHelpContent = null;
    }

    protected virtual void SetScriptTransientGlobals()
    {
    }

    protected override Bitmap Icon
    {
      get { return Resources.python; }
    }

    public override GH_Exposure Exposure
    {
      get { return GH_Exposure.secondary; }
    }

    public override bool AppendMenuItems(ToolStripDropDown iMenu)
    {
      var toReturn = base.AppendMenuItems(iMenu);

      try
      {

        {
          var tsi = new ToolStripMenuItem("&Presentation style", null, new ToolStripItem[]
                    {
                        new ToolStripMenuItem("Show \"code\" input parameter", GetCheckedImage(!HideCodeInput), new TargetGroupToggler()
                            {
                                Component = this,
                                Params = Params.Input,
                                GetIsShowing = ()=>{return HideCodeInput;},
                                SetIsShowing = (value)=>{HideCodeInput = value;},
                                Side = GH_ParameterSide.Input,
                            }.Toggle)
                        {
                             ToolTipText = string.Format("Code input is {0}. Click to {1} it.", HideCodeInput ? "hidden" : "shown", HideCodeInput ? "show" : "hide"),
                        },
                        new ToolStripMenuItem("Show output \"out\" parameter", GetCheckedImage(!HideCodeOutput), new TargetGroupToggler()
                            {
                                Component = this,
                                Params = Params.Output,
                                GetIsShowing = ()=>{return HideCodeOutput;},
                                SetIsShowing = (value)=>{HideCodeOutput = value;},
                                Side = GH_ParameterSide.Output,
                            }.Toggle)
                        {
                             ToolTipText = string.Format("Print output is {0}. Click to {1} it.", HideCodeOutput ? "hidden" : "shown", HideCodeOutput ? "show" : "hide"),
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

      return toReturn;
    }

    class TargetGroupToggler
    {
      public ScriptingAncestorComponent Component { get; set; }
      public List<IGH_Param> Params { get; set; }
      public Func<bool> GetIsShowing { get; set; }
      public Action<bool> SetIsShowing { get; set; }
      public GH_ParameterSide Side { get; set; }

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

            if (Side == GH_ParameterSide.Input)
              Component.CodeInput = code;
          }
          else
          {
            Param_String param;
            if (Side == GH_ParameterSide.Input)
            {
              param = Component.ConstructCodeInputParameter();
              Component.Params.RegisterInputParam(param, 0);
              param.SetPersistentData(new GH_String(code));
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

    const string HideInputIdentifier = "HideInput";
    const string CodeInputIdentifier = "CodeInput";
    const string HideOutputIdentifier = "HideOutput";

    public override bool Write(GH_IO.Serialization.GH_IWriter writer)
    {
      var toReturn = base.Write(writer);

      writer.SetBoolean(HideInputIdentifier, HideCodeInput);
      if (HideCodeInput)
        writer.SetString(CodeInputIdentifier, CodeInput);

      writer.SetBoolean(HideOutputIdentifier, HideCodeOutput);

      return toReturn;
    }

    public override bool Read(GH_IO.Serialization.GH_IReader reader)
    {
      {
        var hideInput = false;
        if (reader.TryGetBoolean(HideInputIdentifier, ref hideInput))
          HideCodeInput = hideInput;
      }

      if (HideCodeInput)
      {
        string code = null;
        if (reader.TryGetString(CodeInputIdentifier, ref code))
          CodeInput = code;
      }

      {
        var hideOutput = false;
        if (reader.TryGetBoolean(HideOutputIdentifier, ref hideOutput))
          HideCodeOutput = hideOutput;
      }

      if (HideCodeInput)
        Params.Input.RemoveAt(0);
      if (HideCodeOutput)
        Params.Output.RemoveAt(0);

      var toReturn = base.Read(reader);

      // Dynamic input fix for existing scripts
      // Always assign DynamicHint or Grasshopper
      // will set Line and not LineCurve, etc...
      if (Params != null && Params.Input != null)
        for (int i = HideCodeInput ? 0 : 1; i < Params.Input.Count; i++)
        {
          var p = Params.Input[i] as Param_ScriptVariable;
          if (p != null)
          {
            FixGhInput(p, false);
            if (p.TypeHint == null)
            {
              p.TypeHint = p.Hints[0];
            }
          }
        }

      return toReturn || true;
    }

    protected static Bitmap GetCheckedImage(bool check)
    {
      return check ? Resources._checked : Resources._unchecked;
    }

    protected override void OnLockedChanged(bool nowIsLocked)
    {
      base.OnLockedChanged(nowIsLocked);
    }

    void OnDocSolutionEnd(object sender, GH_SolutionEventArgs e)
    {
      if (_document != null)
        _document.Objects.Clear();
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
        if (_inDocStringsMode)
        {
          if (SpecialPythonHelpContent == null)
            return base.HelpDescription;
          else
            return base.HelpDescription +
              "<br><br>\n<small>Remarks: <i>" +
              DocStringUtils.Htmlify(SpecialPythonHelpContent) +
              "</i></small>";
        }
        else
        {
          return Resources.helpText;
        }
      }
    }

    protected override string HtmlHelp_Source()
    {
      var b = base.HtmlHelp_Source() ?? string.Empty;
      return base.HtmlHelp_Source().Replace("\nPython Script", "\n" + NickName);
    }

    protected static IGH_TypeHint[] PossibleHints = 
        { 
            new GH_HintSeparator(),
            new GH_BooleanHint_CS(),new GH_IntegerHint_CS(), new GH_DoubleHint_CS(), new GH_ComplexHint(),
            new GH_StringHint_CS(), new GH_DateTimeHint(), new GH_ColorHint(),
            new GH_HintSeparator(),
            new GH_Point3dHint(),
            new GH_Vector3dHint(), new GH_PlaneHint(), new GH_IntervalHint(),
            new GH_UVIntervalHint()
        };

    internal abstract void FixGhInput(Param_ScriptVariable i, bool alsoSetIfNecessary = true);

    public string SpecialPythonHelpContent { get; set; }
  }
}