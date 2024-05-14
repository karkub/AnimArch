using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Visualization.UI;
using UnityEngine;

namespace OALProgramControl
{
    public class EXECommandRead : EXECommand
    {
        public String AssignmentType { get; }
        public EXEASTNodeAccessChain AssignmentTarget { get; }
        public EXEASTNodeBase Prompt { get; }  // Must be String type
        public string PromptText { get; set; }

        public EXECommandRead(String assignmentType, EXEASTNodeAccessChain assignmentTarget, EXEASTNodeBase prompt)
        {
            this.AssignmentType = assignmentType ?? EXETypes.StringTypeName;
            this.AssignmentTarget = assignmentTarget;
            this.Prompt = prompt;
        }

        protected override EXEExecutionResult Execute(OALProgram OALProgram)
        {
            EXEExecutionResult assignmentTargetEvaluationResult
                = this.AssignmentTarget.Evaluate(this.SuperScope, OALProgram, new EXEASTNodeAccessChainContext() { CreateVariableIfItDoesNotExist = true, VariableCreationType = this.AssignmentType });

            if (!HandleRepeatableASTEvaluation(assignmentTargetEvaluationResult))
            {
                return assignmentTargetEvaluationResult;
            }

            EXEExecutionResult promptEvaluationResult = null;
            if (this.Prompt != null)
            {
                promptEvaluationResult = this.Prompt.Evaluate(this.SuperScope, OALProgram);

                if (!HandleRepeatableASTEvaluation(promptEvaluationResult))
                {
                    return promptEvaluationResult;
                }
            }

            if (promptEvaluationResult.ReturnedOutput is not EXEValueString)
            {
                return Error("XEC2025", string.Format("Tried to read from console with prompt that is not string. Instead, it is \"{0}\".", promptEvaluationResult.ReturnedOutput.TypeName));
            }

            string prompt = string.Empty;
            EXEValueString retOutput = promptEvaluationResult.ReturnedOutput as EXEValueString;
            
            if (retOutput != null) {
                VisitorCommandToString visitor = new VisitorCommandToString();
                retOutput.Accept(visitor);
                prompt = visitor.GetCommandString();
            }

            this.PromptText = prompt;
            MenuManager.Instance.Strategy.Read(this, OALProgram);

            return Success();
        }

        public EXEExecutionResult AssignReadValue(String Value, OALProgram OALProgram)
        {
            EPrimitiveType readValueType = EXETypes.DeterminePrimitiveType(Value);

            if (readValueType == EPrimitiveType.NotPrimitive)
            {
                return Error("XEC2026", ErrorMessage.InvalidValueForType(Value, this.AssignmentType));
            }

            EXEValuePrimitive readValue = EXETypes.DeterminePrimitiveValue(Value);

            AssignmentTarget.EvaluationResult.ReturnedOutput.AssignValueFrom(readValue);

            return Success();
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeCommandRead(this);
        }

        public override EXECommand CreateClone()
        {
            return new EXECommandRead(this.AssignmentType, this.AssignmentTarget.Clone() as EXEASTNodeAccessChain, this.Prompt.Clone());
        }
    }
}
