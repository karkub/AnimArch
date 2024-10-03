using System;
using System.Collections.Generic;
using System.Linq;

namespace OALProgramControl
{
    public class EXEASTNodeLeaf : EXEASTNodeBase
    {
        public String Value { get; }

        public EXEASTNodeLeaf(String Value) : base()
        {
            this.Value = Value;
        }

        public override EXEExecutionResult Evaluate(EXEScopeBase currentScope, OALProgram currentProgramInstance, EXEASTNodeAccessChainContext valueContext = null)
        {
            if (this.EvaluationState == EEvaluationState.HasBeenEvaluated)
            {
                return this.EvaluationResult;
            }

            // Whatever happens, this node will have been evaluated
            this.EvaluationState = EEvaluationState.HasBeenEvaluated;

            if (valueContext == null || valueContext.CurrentValue == null)
            {
                // Either we are looking for a variable or we have a primitive literal value
                EXEValuePrimitive literalValue = EXETypes.DeterminePrimitiveValue(this.Value);

                if (literalValue != null)
                {
                    // We have a primitive literal value
                    this.EvaluationResult = EXEExecutionResult.Success();
                    this.EvaluationResult.ReturnedOutput = literalValue;
                    return this.EvaluationResult;
                }

                // We are looking for a variable

                EXEVariable variable = currentScope.FindVariable(this.Value);

                if (variable == null)
                {
                    // Variable not found

                    // It also might be implicit reference to attribute of current class
                    EXEVariable selfVariable = currentScope.FindVariable(EXETypes.SelfReferenceName);

                    if (selfVariable.Value.AttributeExists(this.Value))
                    {
                        this.EvaluationResult = selfVariable.Value.RetrieveAttributeValue(this.Value);
                        return this.EvaluationResult;
                    }

                    if (valueContext != null && valueContext.CreateVariableIfItDoesNotExist)
                    {
                        // We want to create a new variable - so let us do it

                        variable = new EXEVariable(this.Value, EXETypes.DefaultValue(valueContext.VariableCreationType, currentProgramInstance.ExecutionSpace));

                        EXEExecutionResult variableCreationResult = currentScope.AddVariable(variable);

                        if (!variableCreationResult.IsSuccess)
                        {
                            this.EvaluationResult = variableCreationResult;
                        }
                        else
                        {
                            this.EvaluationResult = EXEExecutionResult.Success();
                            this.EvaluationResult.ReturnedOutput = variable.Value;
                        }

                        return this.EvaluationResult;
                    }
                    else
                    {
                        // We want to access an existing variable, but it was not found - so let us report the error
                        this.EvaluationResult = EXEExecutionResult.Error("XEC2001", ErrorMessage.VariableNotFound(this.Value, currentScope));
                        return this.EvaluationResult;
                    }
                }
                else
                {
                    // Variable found
                    // We want to access an existing variable and it already exists - so let us just retrieve its value
                    this.EvaluationResult = EXEExecutionResult.Success();
                    this.EvaluationResult.ReturnedOutput = variable.Value;
                    return this.EvaluationResult;
                }
            }
            else
            {
                // We are looking for an attribute of an object
                this.EvaluationResult = valueContext.CurrentValue.RetrieveAttributeValue(this.Value);
                return this.EvaluationResult;
            }
        }

        public override EXEASTNodeBase Clone()
        {
            return new EXEASTNodeLeaf(this.Value);
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeASTNodeLeaf(this);
        }
    }
}
