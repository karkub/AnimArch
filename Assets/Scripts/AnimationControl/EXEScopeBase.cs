using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OALProgramControl
{
    public abstract class EXEScopeBase : EXECommand 
    {
        public List<EXEVariable> LocalVariables;
        public List<EXEVariable> Variables
        {
            get
            {
                return this.LocalVariables.Select(x => x).ToList();
            }
        }
        public EXEScopeBase()
        {
            this.LocalVariables = new List<EXEVariable>();
        }
        
        public abstract Dictionary<string, string> AllDeclaredVariables();
       
        public abstract EXEExecutionResult AddVariable(EXEVariable variable);
        
        public abstract bool VariableExists(String seekedVariableName);
        
        public abstract EXEVariable FindVariable(String seekedVariableName);
    }
}