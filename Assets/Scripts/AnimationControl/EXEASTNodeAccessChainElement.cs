namespace OALProgramControl
{
    public class EXEASTNodeAccessChainElement
    {
        public readonly EXEASTNodeBase NodeValue;

        public EXEASTNodeAccessChainElement PreviousNode;
        public EXEASTNodeAccessChainElement NextNode;
        public EXEValueBase EvaluationResult;

        public EXEASTNodeAccessChainElement(EXEASTNodeBase nodeValue)
        {
            this.NodeValue = nodeValue;
            this.PreviousNode = null;
            this.NextNode = null;
            this.EvaluationResult = null;
        }

        public EXEExecutionResult Evaluate(EXEScopeBase currentScope, OALProgram currentProgramInstance, EXEASTNodeAccessChainContext valueContext)
        {
            VisitorCommandToString visitor = new VisitorCommandToString();
            this.NodeValue.Accept(visitor);

            valueContext.CreateVariableIfItDoesNotExist = valueContext.CreateVariableIfItDoesNotExist && this.PreviousNode == null;
            valueContext.VariableCreationType = valueContext.CreateVariableIfItDoesNotExist ? valueContext.VariableCreationType : null;
            valueContext.CurrentValue = this.PreviousNode == null ? null : PreviousNode.EvaluationResult;
            valueContext.CurrentAccessChain = (valueContext.CurrentAccessChain ?? string.Empty) + visitor.GetCommandString();

            EXEExecutionResult executionResult = this.NodeValue.Evaluate(currentScope, currentProgramInstance, valueContext);

            if (!executionResult.IsSuccess || !executionResult.IsDone)
            {
                return executionResult;
            }

            this.EvaluationResult = executionResult.ReturnedOutput;

            if (this.NextNode == null)
            {
                return executionResult;
            }

            return this.NextNode.Evaluate(currentScope, currentProgramInstance, valueContext);
        }

        public EXEASTNodeAccessChainElement Clone()
        {
            return new EXEASTNodeAccessChainElement(this.NodeValue.Clone());
        }
    }
}