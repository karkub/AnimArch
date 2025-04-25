using System;
using System.Linq;
using System.Collections.Generic;

namespace OALProgramControl
{
    public class EXEScopeMethod : EXEScope
    {
        public static long CommandIDSeed { get; set; } = 1;

        public CDMethod MethodDefinition;
        public IReturnCollector MethodCallOrigin;
        public EXEValueBase OwningObject;

        public static EXEScopeMethod Create() => Create(null);
        public static EXEScopeMethod Create(CDMethod methodDefinition = null)
        {
            EXEScopeMethod method = new EXEScopeMethod(methodDefinition);
            method.SetCommandID();
            return method;
        }

        protected EXEScopeMethod() : this(null) { }
        protected EXEScopeMethod(CDMethod methodDefinition) : base()
        {
            this.MethodDefinition = methodDefinition;
            this.MethodCallOrigin = null;
            this.OwningObject = null;
        }
        public override bool SubmitReturn(EXEValueBase returnedValue, OALProgram programInstance)
        {
            // Check compatibility of types
            if (!EXETypes.CanBeAssignedTo(returnedValue, this.MethodDefinition.ReturnType, programInstance.ExecutionSpace))
            {
                return false;
            }

            // TODO - once replace with null object for the animation starting method
            if (MethodCallOrigin != null)
            {
                // This actually performs the assignment
                this.MethodCallOrigin.CollectReturn(returnedValue);
            }

            return true;
        }
        protected override EXEExecutionResult Execute(OALProgram OALProgram)
        {
            EXECommand returnCommand = new EXECommandReturn(null);
            returnCommand.SetCommandID();

            AddCommandsToStack(new List<EXECommand>() { returnCommand });
            AddCommandsToStack(this.Commands);
            return Success();
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeScopeMethod(this);
        }

        public override void AddCommand(EXECommand Command)
        {
            base.AddCommand(Command);
            if (Command.CommandID <= 0)
            {
                Command.SetCommandID();
            }
        }
        protected override EXEScope CreateDuplicateScope()
        {
            return new EXEScopeMethod(this.MethodDefinition);
        }
        public override EXEExecutionResult AddVariable(EXEVariable variable)
        {
            if (this.MethodDefinition.Parameters.Select(parameter => parameter.Name).Contains(variable.Name))
            {
                return Error("XEC2030", string.Format("Cannot create variable \"{0}\" as it is already name of a parameter of the current method.", variable.Name));
            }

            if (this.MethodDefinition.OwningClass.GetAttributes(true).Select(attribute => attribute.Name).Contains(variable.Name))
            {
                return Error("XEC2031", string.Format("Cannot create variable \"{0}\" as it is already name of an attribute of the current class.", variable.Name));
            }

            if (this.MethodDefinition.OwningClass.GetMethods(true).Select(method => method.Name).Contains(variable.Name))
            {
                return Error("XEC2032", string.Format("Cannot create variable \"{0}\" as it is already name of a method of the current class.", variable.Name));
            }

            return base.AddVariable(variable);
        }
        public EXEExecutionResult AddParameterVariable(EXEVariable variable)
        {
            if (this.MethodDefinition.OwningClass.GetAttributes(true).Select(attribute => attribute.Name).Contains(variable.Name))
            {
                return Error("XEC2031", string.Format("Cannot create variable \"{0}\" as it is already name of an attribute of the current class.", variable.Name));
            }

            if (this.MethodDefinition.OwningClass.GetMethods(true).Select(method => method.Name).Contains(variable.Name))
            {
                return Error("XEC2032", string.Format("Cannot create variable \"{0}\" as it is already name of a method of the current class.", variable.Name));
            }

            return base.AddVariable(variable);
        }
        public EXEExecutionResult InitializeVariables(OALProgram programInstance)
        {
            return InitializeVariables
            (
                this.MethodDefinition
                    .Parameters
                    .Select(parameter => new EXEVariable(parameter.Name, EXETypes.DefaultValue(parameter.Type, programInstance.ExecutionSpace)))
            );
        }
        public EXEExecutionResult InitializeVariables(IEnumerable<EXEVariable> argumentVariables)
        {
            EXEExecutionResult variableCreationSuccess
                = AddVariable(new EXEVariable(EXETypes.SelfReferenceName, this.OwningObject));

            if (!HandleSingleShotASTEvaluation(variableCreationSuccess))
            {
                return variableCreationSuccess;
            }

            foreach (EXEVariable variable in argumentVariables)
            {
                variableCreationSuccess = AddParameterVariable(variable);

                if (!HandleSingleShotASTEvaluation(variableCreationSuccess))
                {
                    return variableCreationSuccess;
                }
            }

            return Success();
        }
    }
}