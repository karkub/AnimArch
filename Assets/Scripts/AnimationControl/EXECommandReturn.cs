
namespace OALProgramControl
{
    public class EXECommandReturn : EXECommand
    {
        public EXEASTNodeBase Expression { get; }

        public EXECommandReturn(EXEASTNodeBase Expression)
        {
            this.Expression = Expression;
        }

        protected override EXEExecutionResult Execute(OALProgram OALProgram)
        {
            if (Expression != null)
            {
                EXEExecutionResult returnedExpressionEvaluationResult = Expression.Evaluate(this.SuperScope, OALProgram);

                if (!HandleRepeatableASTEvaluation(returnedExpressionEvaluationResult))
                {
                    return returnedExpressionEvaluationResult;
                }

                EXEScopeMethod owningMethodScope = GetCurrentMethodScope();
                if (!owningMethodScope.SubmitReturn(returnedExpressionEvaluationResult.ReturnedOutput, OALProgram))
                {
                    return Error("XEC2014", "Failed to deliver return value to owning scope.");
                }
            }

            this.CommandStack.RemoveOwnedCommands(this.GetCurrentMethodScope());

            return Success();
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeCommandReturn(this);
        }

        protected override EXECommand CreateCloneCustom()
        {
            return new EXECommandReturn(Expression.Clone());
        }
    }
}
