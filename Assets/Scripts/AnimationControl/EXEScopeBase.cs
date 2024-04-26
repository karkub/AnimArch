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
        public List<EXECommand> Commands { get; protected set; }
        public String OALCode;

        public EXEScopeBase()
        {
            this.LocalVariables = new List<EXEVariable>();
            this.Commands = new List<EXECommand>();
        }
        
        public abstract Dictionary<string, string> AllDeclaredVariables();

        protected abstract void AddCommandsToStack(List<EXECommand> Commands);
        
        public abstract EXEExecutionResult AddVariable(EXEVariable variable);
        
        public abstract bool VariableExists(String seekedVariableName);
        
        public abstract EXEVariable FindVariable(String seekedVariableName);
    }
}