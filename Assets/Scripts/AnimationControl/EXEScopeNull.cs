using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OALProgramControl
{
    public class EXEScopeNull : EXEScopeBase
    {
        protected static EXEScopeNull instance = null;
        
        public EXEScopeNull() : base()
        {
        }

        public static EXEScopeNull GetInstance()
        {
            if (instance == null)
            {
                instance = new EXEScopeNull();
            }
            return instance;
        }

        public override IEnumerable<EXEScopeBase> ScopesToTop()
        {
            yield break;
        }
        
        protected override EXEExecutionResult Execute(OALProgram OALProgram) 
        {
            return EXEExecutionResult.Success();
        }
        
        public override Dictionary<string, string> AllDeclaredVariables()
        {
            return new Dictionary<string, string>();
        }
                
        public override EXEExecutionResult AddVariable(EXEVariable variable)
        {
            return EXEExecutionResult.Success();
        }
        
        public override bool VariableExists(String seekedVariableName)
        {
            return FindVariable(seekedVariableName) != null;
        }
        
        public override EXEVariable FindVariable(String seekedVariableName)
        {
            return null;
        }       
        
        public override Boolean IsComposite()
        {
            return true;
        }
        
        public override EXECommand CreateClone()
        {
            return GetInstance();
        }

        public override void ToggleActiveRecursiveBottomUp(bool active)
        {
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeScopeNull(this);
        }
    }
}