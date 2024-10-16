using System;
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

            EXEExecutionResult evaluationResultOfAssignmentTarget
                = this.AssignmentTarget.Evaluate(this.SuperScope, OALProgram, new EXEASTNodeAccessChainContext() { CreateVariableIfItDoesNotExist = true, VariableCreationType = evaluationResultOfAssignedExpression.ReturnedOutput.TypeName });

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

        public override void Accept(Visitor v)
        {
            v.VisitExeCommandAssignment(this);
        }

        protected override EXECommand CreateCloneCustom()
        {
            return new EXECommandAssignment((EXEASTNodeAccessChain)this.AssignmentTarget.Clone(), this.AssignedExpression.Clone());
        }
        public CDClassInstance GetAssignmentTargetOwner()
        {
            return this.AssignmentTarget.GetFinalValueOwner();
        }
    }
}
