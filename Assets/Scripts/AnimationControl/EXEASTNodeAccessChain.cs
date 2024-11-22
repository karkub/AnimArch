using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OALProgramControl
{
    public class EXEASTNodeAccessChain : EXEASTNodeBase
    {
        public EXEASTNodeAccessChainElement FirstElement { get; private set; }
        public EXEASTNodeAccessChainElement LastElement { get; private set; }
        public bool CreateVariableIfItDoesNotExist;

        public EXEASTNodeAccessChain() : base()
        {
            this.FirstElement = null;
            this.LastElement = null;
            this.CreateVariableIfItDoesNotExist = false;
        }


        public override EXEExecutionResult Evaluate(EXEScopeBase currentScope, OALProgram currentProgramInstance, EXEASTNodeAccessChainContext valueContext = null)
        {
            if (this.EvaluationState == EEvaluationState.HasBeenEvaluated)
            {
                return this.EvaluationResult;
            }

            if (this.FirstElement == null)
            {
                return EXEExecutionResult.Error("XEC2000", "Access chain with no elements.", null);
            }

            valueContext = valueContext ?? new EXEASTNodeAccessChainContext();
            valueContext.CreateVariableIfItDoesNotExist
                = valueContext.CreateVariableIfItDoesNotExist && this.FirstElement == this.LastElement;
            valueContext.VariableCreationType = valueContext.CreateVariableIfItDoesNotExist ? valueContext.VariableCreationType : null;

            this.EvaluationState = EEvaluationState.IsBeingEvaluated;
            
            EXEExecutionResult evaluationResult = FirstElement.Evaluate(currentScope, currentProgramInstance, valueContext);

            this.EvaluationResult = evaluationResult;

            if (this.EvaluationResult.IsDone)
            {
                this.EvaluationState = EEvaluationState.HasBeenEvaluated;
            }

            return evaluationResult;
        }

        public IEnumerable<EXEASTNodeAccessChainElement> GetElements()
        {
            EXEASTNodeAccessChainElement currentElement = this.FirstElement;

            while (currentElement != null)
            {
                yield return currentElement;

                currentElement = currentElement.NextNode;
            }
        }

        public void AddElement(EXEASTNodeBase elementValue)
        {
            EXEASTNodeAccessChainElement element = new EXEASTNodeAccessChainElement(elementValue);

            if (this.FirstElement == null)
            {
                element.PreviousNode = null;
                element.NextNode = null;
                this.FirstElement = element;
                this.LastElement = element;
                return;
            }

            element.PreviousNode = this.LastElement;
            element.NextNode = null;
            this.LastElement.NextNode = element;
            this.LastElement = element;
        }

        public override EXEASTNodeBase Clone()
        {
            EXEASTNodeAccessChain result = new EXEASTNodeAccessChain();

            foreach (EXEASTNodeAccessChainElement chainElement in this.GetElements())
            {
                result.AddElement(chainElement.NodeValue.Clone());
            }

            return result;
        }
        public CDClassInstance GetFinalValueOwner()
        {
            List<EXEASTNodeAccessChainElement> reversedElements = GetElements().SkipLast(1).Reverse().ToList();

            if (!reversedElements.Any())
            {
                return null;
            }

            return (reversedElements.First().EvaluationResult as EXEValueReference)?.ClassInstance;
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeASTNodeAccesChain(this);
        }
    }
}
