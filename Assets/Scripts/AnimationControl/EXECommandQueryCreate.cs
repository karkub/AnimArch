using System;
using UnityEngine;

namespace OALProgramControl
{
    public class EXECommandQueryCreate : EXECommand
    {
        public String ClassName { get; }
        public EXEASTNodeAccessChain AssignmentTarget { get; }

        public EXECommandQueryCreate(String ClassName) : this(ClassName, null)
        {
        }
        public EXECommandQueryCreate(String ClassName, EXEASTNodeAccessChain assignmentTarget)
        {
            this.ClassName = ClassName;
            this.AssignmentTarget = assignmentTarget;
        }

        protected override EXEExecutionResult Execute(OALProgram OALProgram)
        {
            EXEExecutionResult assignmentTargetEvaluationResult = null;

            if (this.AssignmentTarget != null)
            {
                assignmentTargetEvaluationResult
                    = this.AssignmentTarget.Evaluate(this.SuperScope, OALProgram, new EXEASTNodeAccessChainContext() { CreateVariableIfItDoesNotExist = true, VariableCreationType = this.ClassName });

                if (!HandleRepeatableASTEvaluation(assignmentTargetEvaluationResult))
                {
                    return assignmentTargetEvaluationResult;
                }
            }

            EXEExecutionResult classInstanceCreationResult = OALProgram.ExecutionSpace.CreateInstance(this.ClassName);

            if (!HandleSingleShotASTEvaluation(classInstanceCreationResult))
            {
                return classInstanceCreationResult;
            }

            if (this.AssignmentTarget != null)
            {
                EXEExecutionResult assignmentResult
                    = assignmentTargetEvaluationResult.ReturnedOutput.AssignValueFrom(classInstanceCreationResult.ReturnedOutput);

                if (!HandleSingleShotASTEvaluation(assignmentResult))
                {
                    return assignmentResult;
                }
            }

            return Success();
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeCommandQueryCreate(this);
        }

        protected override EXECommand CreateCloneCustom()
        {
            return new EXECommandQueryCreate(this.ClassName, this.AssignmentTarget?.Clone() as EXEASTNodeAccessChain);
        }
        public CDClassInstance GetCreatedInstance()
        {
            return (this.AssignmentTarget.LastElement.EvaluationResult as EXEValueReference)?.ClassInstance;
        }
    }
}