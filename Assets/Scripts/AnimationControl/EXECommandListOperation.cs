using System.Linq;

namespace OALProgramControl
{
    public abstract class EXECommandListOperation : EXECommand
    {
        public abstract EXEExecutionResult EvaluateItem(OALProgram OALProgram);
        public abstract EXEExecutionResult EvaluateArray(OALProgram OALProgram, out bool success);
        public abstract EXEExecutionResult PerformOperation(OALProgram OALProgram, EXEExecutionResult arrayEvaluationResult, EXEExecutionResult itemEvaluationResult);

        protected override EXEExecutionResult Execute(OALProgram OALProgram)
        {
            EXEExecutionResult itemEvaluationResult = EvaluateItem(OALProgram);

            if (!HandleRepeatableASTEvaluation(itemEvaluationResult))
            {
                return itemEvaluationResult;
            }

            bool success = false;
            EXEExecutionResult arrayEvaluationResult = EvaluateArray(OALProgram, out success);
            
            if (!success) {
                return arrayEvaluationResult;
            }

            EXEExecutionResult finalEvaluationResult = PerformOperation(OALProgram, arrayEvaluationResult, itemEvaluationResult);

            if (!HandleSingleShotASTEvaluation(finalEvaluationResult))
            {
                return finalEvaluationResult;
            } 

            return Success();
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeCommandListOperation(this);
        }
    }
}