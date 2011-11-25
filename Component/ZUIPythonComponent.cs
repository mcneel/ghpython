using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;

namespace GhPython.Component
{
    public class ZuiPythonComponent : ScriptingAncestorComponent, IGH_VariableParameterComponent
    {

        protected override void AddDefaultInput(GH_Component.GH_InputParamManager pManager)
        {
            pManager.RegisterParam(CreateParameter(GH_ParameterSide.Input, pManager.ParamCount));
            pManager.RegisterParam(CreateParameter(GH_ParameterSide.Input, pManager.ParamCount));
        }

        protected override void AddDefaultOutput(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(CreateParameter(GH_ParameterSide.Output, pManager.ParamCount));
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
                      Name = "Input " + index.ToString(),
                      Description = "Variable script input " + index.ToString(),
			            };
		            }
		            case GH_ParameterSide.Output:
		            {
			            return new Param_GenericObject
			            {
				              NickName = GH_ComponentParamServer.InventUniqueNickname("abcdefghijklmn", this.Params.Output),
                      Name = "Result " + index.ToString(),
                      Description = "Variable script output " + index.ToString()
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
                return index > (HideCodeInput ? -1 : 0);
            else if(side == GH_ParameterSide.Output)
                return index > (HideCodeOutput ? -1 : 0);
            return false;
        }

        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return (this as IGH_VariableParameterComponent).CanInsertParameter(side, index);
        }

        public override void VariableParameterMaintenance()
        {
            foreach (var i in Params.Input)
            {
                if (i is Param_ScriptVariable)
                    FixGhInput(i as Param_ScriptVariable);
            }
            foreach (var i in Params.Output)
            {
                if (i is Param_GenericObject)
                {
                    i.Name = i.NickName;
                    i.Description = i.NickName;
                }
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

        #endregion
    }
}
