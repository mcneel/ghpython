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
    // python output stream is piped to m_py_output
    readonly StringList m_py_output = new StringList();


    internal static GrasshopperDocument _document = new GrasshopperDocument();
    internal ComponentIOMarshal _marshal;
    protected PythonScript _py;
    private PythonCompiledCode _compiled_py;
    protected string _previousRunCode;
    protected PythonEnvironment _env;
    private bool _inDocStringsMode;
    private string _codeInput;

    internal const string DOCUMENT_NAME = "ghdoc";
    const string PARENT_ENVIRONMENT_NAME = "ghenv";
    const string DESCRIPTION = "A python scriptable component";


    protected ScriptingAncestorComponent()
      : base("Python Script", "Python", DESCRIPTION, "Math", "Script")
    {
    }

    /// <summary>
    /// Show/Hide the "Code" input no this component.  This is not something that
    /// users typically need to work with so it defaults to off and is only useful
    /// when users are dynamically generating scripts through some other component
    /// </summary>
    public bool CodeInputVisible
    {
      get
      {
        if (Params.Input.Count < 1)
          return false;
        var param = Params.Input[0] as Grasshopper.Kernel.Parameters.Param_String;
        return (param != null && String.Compare(param.Name, "code", StringComparison.InvariantCultureIgnoreCase) == 0);
      }
      set
      {
        if (value != CodeInputVisible)
        {
          if (value)
          {
            var param = ConstructCodeInputParameter();
            Params.RegisterInputParam(param, 0);
            param.SetPersistentData(new GH_String(CodeInput));
          }
          else
          {
            Params.UnregisterParameter(Params.Input[0]);
          }
          Params.OnParametersChanged();
          OnDisplayExpired(true);
          if(value) 
            ExpireSolution(true);
        }
      }
    }
    


    public string CodeInput
    {
      get
      {
        if (!CodeInputVisible)
          return _codeInput;
        return ScriptingAncestorComponent.ExtractCodeString((Param_String) Params.Input[0]);
      }
      set
      {
        if (CodeInputVisible)
          throw new InvalidOperationException("Cannot assign to CodeInput while parameter exists");
        _codeInput = value;
        _compiled_py = null;
      }
    }

    /// <summary>
    /// Returns true if the "Code" input parameter is visible AND wired up to
    /// another component
    /// </summary>
    /// <returns></returns>
    public bool CodeInputIsLinked()
    {
      if (!CodeInputVisible)
        return false;
      var param = Params.Input[0];
      return param.SourceCount > 0;
    }

    public Param_String CodeInputParam
    {
      get
      {
        if (CodeInputVisible)
          return (Param_String) Params.Input[0];
        return null;
      }
    }

    public bool HideCodeOutput { get; set; }

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
        _py.Output = m_py_output.Write;
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
      if (CodeInputVisible)
      {
        pManager.RegisterParam(ConstructCodeInputParameter());
      }
      AddDefaultInput(pManager);
    }

    protected abstract void AddDefaultInput(GH_Component.GH_InputParamManager pManager);

    public Param_String ConstructCodeInputParameter()
    {
      var code = new Param_String
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
      var outText = new Param_String
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

      m_py_output.Reset();

      var rhdoc = RhinoDoc.ActiveDoc;
      var prevEnabled = (rhdoc != null) && rhdoc.Views.RedrawEnabled;

      try
      {
        // set output variables to "None"
        for (int i = HideCodeOutput ? 0 : 1; i < Params.Output.Count; i++)
        {
          string varname = Params.Output[i].NickName;
          _py.SetVariable(varname, null);
        }

        // caching variable to keep things as fast as possible
        bool showing_code_input = CodeInputVisible; 
        // Set all input variables. Even null variables may be used in the
        // script, so do not attempt to skip these for optimization purposes.
        // Skip "Code" input parameter
        // Please pay attention to the input data structure type
        for (int i = showing_code_input ? 1 : 0; i < Params.Input.Count; i++)
        {
          string varname = Params.Input[i].NickName;
          object o = _marshal.GetInput(DA, i);
          _py.SetVariable(varname, o);
          _py.SetIntellisenseVariable(varname, o);
        }

        // the "code" string could be embedded in the component itself
        if (showing_code_input || _compiled_py == null)
        {
          string script;
          if (!showing_code_input)
            script = CodeInput;
          else
          {
            script = null;
            DA.GetData(0, ref script);
          }

          if (string.IsNullOrWhiteSpace(script))
          {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No script to execute");
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
          m_py_output.Write("There was a permanent error parsing this script. Please report to steve@mcneel.com.");
        }
      }
      catch (Exception ex)
      {
        AddErrorNicely(m_py_output, ex);
        SetFormErrorOrClearIt(DA, m_py_output);
        throw;
      }
      finally
      {
        if (rhdoc != null && prevEnabled != rhdoc.Views.RedrawEnabled)
          rhdoc.Views.RedrawEnabled = true;
      }
      SetFormErrorOrClearIt(DA, m_py_output);
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
      var attr = (PythonComponentAttributes) Attributes;

      if (sl.Result.Count > 0)
      {
        if (!HideCodeOutput)
          DA.SetDataList(0, sl.Result);
        attr.TrySetLinkedFormHelpText(sl.ToString());
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

      for (int i = !CodeInputVisible ? 0 : 1; i < Params.Input.Count; i++)
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
              new ToolStripMenuItem("Show \"code\" input parameter", GetCheckedImage(CodeInputVisible),
                                    new TargetGroupToggler
                                      {
                                        Component = this,
                                        Params = Params.Input,
                                        GetIsShowing = () => CodeInputVisible,
                                        SetIsShowing = value => { CodeInputVisible = value; },
                                        Side = GH_ParameterSide.Input,
                                      }.Toggle)
                {
                  ToolTipText =
                    string.Format("Code input is {0}. Click to {1} it.", CodeInputVisible ? "shown": "hidden",
                                  CodeInputVisible ? "hide" : "show"),
                },
              new ToolStripMenuItem("Show output \"out\" parameter", GetCheckedImage(!HideCodeOutput),
                                    new TargetGroupToggler
                                      {
                                        Component = this,
                                        Params = Params.Output,
                                        GetIsShowing = () => HideCodeOutput,
                                        SetIsShowing = value => { HideCodeOutput = value; },
                                        Side = GH_ParameterSide.Output,
                                      }.Toggle)
                {
                  ToolTipText =
                    string.Format("Print output is {0}. Click to {1} it.", HideCodeOutput ? "hidden" : "shown",
                                  HideCodeOutput ? "show" : "hide"),
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

    private class TargetGroupToggler
    {
      public ScriptingAncestorComponent Component { private get; set; }
      public List<IGH_Param> Params { private get; set; }
      public Func<bool> GetIsShowing { private get; set; }
      public Action<bool> SetIsShowing { private get; set; }
      public GH_ParameterSide Side { private get; set; }

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

    // used as keys for file IO
    const string HideInputIdentifier = "HideInput";
    const string CodeInputIdentifier = "CodeInput";
    const string HideOutputIdentifier = "HideOutput";

    public override bool Write(GH_IO.Serialization.GH_IWriter writer)
    {
      bool rc = base.Write(writer);

      writer.SetBoolean(HideInputIdentifier, !CodeInputVisible);
      if (!CodeInputVisible)
        writer.SetString(CodeInputIdentifier, CodeInput);

      writer.SetBoolean(HideOutputIdentifier, HideCodeOutput);

      return rc;
    }

    public override bool Read(GH_IO.Serialization.GH_IReader reader)
    {
      // 8 Aug. 2012
      // There are a couple of "hacks" in here to get IO code to work properly.
      // I'll discuss fixes for this so we can skip over the code in future
      // versions of grasshopper

      bool perform_hacks = this is IGH_VarParamComponent;
      if (perform_hacks)
      {
        // only perform these hacks on 0.9.6 and below. Assuming that this
        // will get fixed in the very next GH release
        var version = Grasshopper.Versioning.Version;
        if (version.major > 0 || version.minor > 9 || (version.minor == 9 && version.revision > 6))
          perform_hacks = false;
      }

      if( perform_hacks )
      {
        // Hack #1
        // When deserializing, this component is constructed and the I can't tell
        // that this component was created for deserializing from disk and the
        // "AddDefaultInput" / "AddDefaultOutput" functions are called. The low level
        // parameter reading code skips reading of params when they already exists
        // (see Read_IGH_VarParamParamList in GH_ComponentParamServer.vb)
        //   ... If (i<params.Count) Then Continue For
        // Clear out the default input parameters so the GH variable
        // parameter reader doesn't get hosed
        for (int i = Params.Input.Count - 1; i >= 0; i--)
          Params.UnregisterParameter(Params.Input[0]);
        for (int i = Params.Output.Count - 1; i >= 0; i--)
          Params.UnregisterParameter(Params.Output[0]);
      }

      bool rc = base.Read(reader);

      if (perform_hacks)
      {
        // Hack #2
        // The IO code in checks to see if "Access" exists when it looks like
        // it should be checking if "Access" at index exists
        // (see Read_IGH_VarParamParamList in GH_ComponentParamServer.vb)
        //   ...If( reader.ItemExists("Access")) Then
        //   probably should be
        //   ...If( reader.ItemExists("Access", i)) Then
        //
        // Working around this issue by manually digging through the chuncks
        if (reader.ChunkCount > 1)
        {
          var chunk = reader.Chunks[1];
          for (int i = 0; i < chunk.ItemCount; i++)
          {
            var item = chunk.Items[i];
            if (item != null && string.Compare(item.Name, "Access", StringComparison.InvariantCulture) == 0)
            {
              int index = item.Index;
              if (index >= 0 && index < Params.Input.Count)
              {
                int access = item._int32;
                if (1 == access)
                  Params.Input[index].Access = GH_ParamAccess.list;
                else if (2 == access)
                  Params.Input[index].Access = GH_ParamAccess.tree;
              }
            }
          }
        }
      }

      bool hideInput = false;
      if (reader.TryGetBoolean(HideInputIdentifier, ref hideInput))
        CodeInputVisible = !hideInput;

      if (!CodeInputVisible)
      {
        string code = null;
        if (reader.TryGetString(CodeInputIdentifier, ref code))
          CodeInput = code;
      }


      bool hideOutput = false;
      if (reader.TryGetBoolean(HideOutputIdentifier, ref hideOutput))
        HideCodeOutput = hideOutput;

      if (HideCodeOutput)
        Params.Output.RemoveAt(0);


      // Dynamic input fix for existing scripts
      // Always assign DynamicHint or Grasshopper
      // will set Line and not LineCurve, etc...
      if (Params != null && Params.Input != null)
      {
        for (int i = CodeInputVisible ? 1 : 0; i < Params.Input.Count; i++)
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
      }
      return rc;
    }

    protected static Bitmap GetCheckedImage(bool check)
    {
      return check ? Resources._checked : Resources._unchecked;
    }

    private void OnDocSolutionEnd(object sender, GH_SolutionEventArgs e)
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
          return base.HelpDescription +
                 "<br><br>\n<small>Remarks: <i>" +
                 DocStringUtils.Htmlify(SpecialPythonHelpContent) +
                 "</i></small>";
        }
        return Resources.helpText;
      }
    }

    protected override string HtmlHelp_Source()
    {
      return base.HtmlHelp_Source().Replace("\nPython Script", "\n" + NickName);
    }

    protected static IGH_TypeHint[] PossibleHints =
      {
        new GH_HintSeparator(),
        new GH_BooleanHint_CS(), new GH_IntegerHint_CS(), new GH_DoubleHint_CS(), new GH_ComplexHint(),
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