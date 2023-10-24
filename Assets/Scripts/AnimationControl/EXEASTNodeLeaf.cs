﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OALProgramControl
{
    public class EXEASTNodeLeaf : EXEASTNodeBase
    {
        private String Value { get; }

        public EXEASTNodeLeaf(String Value) : base()
        {
            this.Value = Value;
        }

        public override EXEExecutionResult Evaluate(EXEScope currentScope, OALProgram currentProgramInstance, EXEASTNodeAccessChainContext valueContext = null)
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

                    if (valueContext != null && valueContext.CreateVariableIfItDoesNotExist)
                    {
                        // We want to create a new variable - so let us do it

                        variable = new EXEVariable(this.Value);

                        EXEExecutionResult variableCreationResult = currentScope.AddVariable(variable);

                        if (!variableCreationResult.IsSuccess)
                        {
                            this.EvaluationResult = variableCreationResult;
                            return this.EvaluationResult;
                        }
                        else
                        {
                            this.EvaluationResult = EXEExecutionResult.Success();
                            this.EvaluationResult.ReturnedOutput = variable.Value;
                            return this.EvaluationResult;
                        }
                    }
                    else
                    {
                        // Either we want to access an existing variable or an attribute of the method owning object
                        EXEVariable selfVariable = currentScope.FindVariable(EXETypes.SelfReferenceName);
                        if (selfVariable.Value.AttributeExists(this.Value))
                        {
                            this.EvaluationResult = selfVariable.Value.RetrieveAttributeValue(this.Value);
                            return this.EvaluationResult;
                        }


                        // We want to access an existing variable, but it was not found - so let us report the error
                        this.EvaluationResult = EXEExecutionResult.Error(ErrorMessage.VariableNotFound(this.Value, currentScope), "XEC2001");
                        return this.EvaluationResult;
                    }
                }
                else
                {
                    // Variable found

                    if (valueContext != null && valueContext.CreateVariableIfItDoesNotExist)
                    {
                        // We want to create a new variable, but it already exists - so let us report the error
                        this.EvaluationResult = EXEExecutionResult.Error(ErrorMessage.CreatingExistingVariable(this.Value), "XEC2002");
                        return this.EvaluationResult;
                    }
                    else
                    {
                        // We want to access an existing variable and it already exists - so let us just retrieve its value
                        this.EvaluationResult = EXEExecutionResult.Success();
                        this.EvaluationResult.ReturnedOutput = variable.Value;
                        return this.EvaluationResult;
                    }
                }
            }
            else
            {
                // We are looking for an attribute of an object
                this.EvaluationResult = valueContext.CurrentValue.RetrieveAttributeValue(this.Value);
                return this.EvaluationResult;
            }
        }

        public override void PrintPretty(string indent, bool last)
        {
            throw new NotImplementedException();
        }

        public override string ToCode()
        {
            throw new NotImplementedException();
        }

        public override EXEASTNodeBase Clone()
        {
            return new EXEASTNodeLeaf(this.Value);
        }
    }
}