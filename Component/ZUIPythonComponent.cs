using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GhPython.Component
{
  [Guid("410755B1-224A-4C1E-A407-BF32FB45EA7E")]
  public class ZuiPythonComponent : ScriptingAncestorComponent, IGH_VariableParameterComponent
  {
    protected override void AddDefaultInput(GH_InputParamManager pManager)
    {
      pManager.AddParameter(CreateParameter(GH_ParameterSide.Input, pManager.ParamCount));
      pManager.AddParameter(CreateParameter(GH_ParameterSide.Input, pManager.ParamCount));
    }

    protected override void AddDefaultOutput(GH_OutputParamManager pManager)
    {
      pManager.RegisterParam(CreateParameter(GH_ParameterSide.Output, pManager.ParamCount));
    }

    internal override void FixGhInput(Param_ScriptVariable i, bool alsoSetIfNecessary = true)
    {
      i.Name = i.NickName;

      if (string.IsNullOrEmpty(i.Description))
        i.Description = string.Format("Script variable {0}", i.NickName);
      i.AllowTreeAccess = true;
      i.Optional = true;
      i.ShowHints = true;
      i.Hints = GetHints();

      if (alsoSetIfNecessary && i.TypeHint == null)
        i.TypeHint = i.Hints[1];
    }

    static readonly List<IGH_TypeHint> g_hints = new List<IGH_TypeHint>();
    static List<IGH_TypeHint> GetHints()
    {
      lock (g_hints)
      {
        if (g_hints.Count == 0)
        {
          g_hints.Add(new NoChangeHint());
          g_hints.Add(new GhDocGuidHint());

          g_hints.AddRange(PossibleHints);

          g_hints.RemoveAll(t =>
            {
              var y = t.GetType();
              return (y == typeof (GH_DoubleHint_CS) || y == typeof (GH_StringHint_CS));
            });
          g_hints.Insert(4, new NewFloatHint());
          g_hints.Insert(6, new NewStrHint());

          g_hints.Add(new GH_BoxHint());

          g_hints.Add(new GH_HintSeparator());

          g_hints.Add(new GH_LineHint());
          g_hints.Add(new GH_CircleHint());
          g_hints.Add(new GH_ArcHint());
          g_hints.Add(new GH_PolylineHint());

          g_hints.Add(new GH_HintSeparator());

          g_hints.Add(new GH_CurveHint());
          g_hints.Add(new GH_MeshHint());
          g_hints.Add(new GH_SurfaceHint());
          g_hints.Add(new GH_BrepHint());
          g_hints.Add(new GH_GeometryBaseHint());
        }
      }
      return g_hints;
    }
    
    #region IGH_VariableParameterComponent implementation

    public IGH_Param CreateParameter(GH_ParameterSide side, int index)
    {
      switch (side)
      {
        case GH_ParameterSide.Input:
          {
            return new Param_ScriptVariable
              {
                NickName = GH_ComponentParamServer.InventUniqueNickname("xyzuvwst", this.Params.Input),
                Name = NickName,
                Description = "Script variable " + NickName,
              };
          }
        case GH_ParameterSide.Output:
          {
            return new Param_GenericObject
              {
                NickName = GH_ComponentParamServer.InventUniqueNickname("abcdefghijklmn", this.Params.Output),
                Name = NickName,
                Description = "Script variable " + NickName,
              };
          }
        default:
          {
            return null;
          }
      }
    }

    bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
    {
      if(side == GH_ParameterSide.Input && !HiddenCodeInput && index == 0)
          m_inner_codeInput = Code;

      return true;
    }

    bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
    {
      return index > -1;
    }

    bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
    {
      return (this as IGH_VariableParameterComponent).CanInsertParameter(side, index);
    }

    public override void VariableParameterMaintenance()
    {
      foreach (Param_ScriptVariable variable in Params.Input.OfType<Param_ScriptVariable>())
        FixGhInput(variable);

      foreach (Param_GenericObject i in Params.Output.OfType<Param_GenericObject>())
      {
        i.Name = i.NickName;
        if (string.IsNullOrEmpty(i.Description))
          i.Description = i.NickName;
      }
    }

    protected override void SetScriptTransientGlobals()
    {
      base.SetScriptTransientGlobals();

      m_py.ScriptContextDoc = m_document;
      m_marshal = new NewComponentIOMarshal(m_document, this);
      m_py.SetVariable(DOCUMENT_NAME, m_document);
      m_py.SetIntellisenseVariable(DOCUMENT_NAME, m_document);
    }

    public override Guid ComponentGuid
    {
      get { return typeof(ZuiPythonComponent).GUID; }
    }

    #endregion
  }
}
