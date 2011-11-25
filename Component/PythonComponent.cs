using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;


#pragma warning disable 0618

namespace GhPython.Component
{
    public class PythonComponent : ScriptingAncestorComponent, IGH_VarParamComponent
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
        
        public override Guid ComponentGuid
        {
            get { return new Guid("{CEAB6E56-CEEC-A646-84D5-363C57440969}"); }
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
    }
}

#pragma warning restore 0618