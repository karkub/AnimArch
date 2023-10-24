﻿using System;
using System.Linq;

namespace OALProgramControl
{
    public class EXECommandAssignment : EXECommand
    {
        public EXEASTNodeAccessChain AssignmentTarget { get; }
        public EXEASTNodeBase AssignedExpression { get; }

        public EXECommandAssignment(EXEASTNodeAccessChain assignmentTarget, EXEASTNodeBase assignedExpression)
        {
            this.AssignmentTarget = assignmentTarget;
            this.AssignmentTarget.CreateVariableIfItDoesNotExist = true;
            this.AssignedExpression = assignedExpression;
        }

        protected override EXEExecutionResult Execute(OALProgram OALProgram)
        {
            EXEExecutionResult evaluationResultOfAssignedExpression = this.AssignedExpression.Evaluate(this.SuperScope, OALProgram);
            if (!HandleRepeatableASTEvaluation(evaluationResultOfAssignedExpression))
            {
                return evaluationResultOfAssignedExpression;
            }

            EXEExecutionResult evaluationResultOfAssignmentTarget = this.AssignmentTarget.Evaluate(this.SuperScope, OALProgram);

            if (!HandleRepeatableASTEvaluation(evaluationResultOfAssignmentTarget))
            {
                return evaluationResultOfAssignmentTarget;
            }

            EXEExecutionResult assignmentResult
                = evaluationResultOfAssignmentTarget
                    .ReturnedOutput
                    .AssignValueFrom(evaluationResultOfAssignedExpression.ReturnedOutput);

            if (!HandleSingleShotASTEvaluation(assignmentResult))
            {
                return assignmentResult;
            }

            return Success();
        }

        public override String ToCodeSimple()
        {
            return this.AssignmentTarget.ToCode() + " = " + this.AssignedExpression.ToCode();
        }

        public override EXECommand CreateClone()
        {
            return new EXECommandAssignment((EXEASTNodeAccessChain)this.AssignmentTarget.Clone(), this.AssignedExpression.Clone());
        }
        public CDClassInstance GetAssignmentTargetOwner()
        {
            return this.AssignmentTarget.GetFinalValueOwner();
        }
    }
}