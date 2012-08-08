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
    public ZuiPythonComponent()
    {
      CodeInputVisible = false;
    }

    protected override void AddDefaultInput(GH_Component.GH_InputParamManager pManager)
    {
      pManager.RegisterParam(CreateParameter(GH_ParameterSide.Input, pManager.ParamCount));
      pManager.RegisterParam(CreateParameter(GH_ParameterSide.Input, pManager.ParamCount));
    }

    protected override void AddDefaultOutput(GH_Component.GH_OutputParamManager pManager)
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

    static readonly List<IGH_TypeHint> m_hints = new List<IGH_TypeHint>();
    static List<IGH_TypeHint> GetHints()
    {
      lock (m_hints)
      {
        if (m_hints.Count == 0)
        {
          m_hints.Add(new NoChangeHint());
          m_hints.Add(new GhDocGuidHint());

          m_hints.AddRange(PossibleHints);

          m_hints.RemoveAll(t =>
            {
              var y = t.GetType();
              return (y == typeof (GH_DoubleHint_CS) || y == typeof (GH_StringHint_CS));
            });
          m_hints.Insert(4, new NewFloatHint());
          m_hints.Insert(6, new NewStrHint());

          m_hints.Add(new GH_BoxHint());

          m_hints.Add(new GH_HintSeparator());

          m_hints.Add(new GH_LineHint());
          m_hints.Add(new GH_CircleHint());
          m_hints.Add(new GH_ArcHint());
          m_hints.Add(new GH_PolylineHint());

          m_hints.Add(new GH_HintSeparator());

          m_hints.Add(new GH_CurveHint());
          m_hints.Add(new GH_MeshHint());
          m_hints.Add(new GH_SurfaceHint());
          m_hints.Add(new GH_BrepHint());
          m_hints.Add(new GH_GeometryBaseHint());
        }
      }
      return m_hints;
    }
    
    #region Members of IGH_VariableParameterComponent

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
      return true;
    }

    bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
    {
      if (side == GH_ParameterSide.Input)
        return index > (!CodeInputVisible ? -1 : 0);
      if (side == GH_ParameterSide.Output)
        return index > (HideCodeOutput ? -1 : 0);
      return false;
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

      _py.ScriptContextDoc = _document;
      _marshal = new NewComponentIOMarshal(_document, this);
      _py.SetVariable(DOCUMENT_NAME, _document);
      _py.SetIntellisenseVariable(DOCUMENT_NAME, _document);
    }

    public override Guid ComponentGuid
    {
      get { return typeof(ZuiPythonComponent).GUID; }
    }

    #endregion
  }
}
