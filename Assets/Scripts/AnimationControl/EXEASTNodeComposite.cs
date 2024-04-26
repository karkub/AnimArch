using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OALProgramControl
{
    public class EXEASTNodeComposite : EXEASTNodeBase
    {
        public String Operation { get; set; }
        public List<EXEASTNodeBase> Operands { get; }

        public EXEASTNodeComposite(String Operation) : base()
        {
            this.Operation = Operation;
            this.Operands = new List<EXEASTNodeBase>();
        }
        public EXEASTNodeComposite(String Operation, EXEASTNodeBase[] Operands) : base()
        {
            this.Operation = Operation;
            this.Operands = new List<EXEASTNodeBase>(Operands);
        }

        public void AddOperand(EXEASTNodeBase Operand)
        {
            if (Operand == null)
            {
                return;
            }

            this.Operands.Add(Operand);
        }

        public String GetNodeValue()
        {
            return this.Operation;
        }
        public override EXEExecutionResult Evaluate(EXEScopeBase currentScope, OALProgram currentProgramInstance, EXEASTNodeAccessChainContext valueContext = null)
        {
            if (this.EvaluationState == EEvaluationState.HasBeenEvaluated)
            {
                return this.EvaluationResult;
            }

            this.EvaluationState = EEvaluationState.IsBeingEvaluated;

            EXEExecutionResult operandExecutionResult;
            EXEExecutionResult operatorExecutionResult;
            foreach (EXEASTNodeBase operand in this.Operands)
            {
                operandExecutionResult = operand.Evaluate(currentScope, currentProgramInstance);

                if (!operandExecutionResult.IsSuccess)
                {
                    this.EvaluationState = EEvaluationState.HasBeenEvaluated;
                    this.EvaluationResult = operandExecutionResult;
                }

                if (!operandExecutionResult.IsDone || !operandExecutionResult.IsSuccess)
                {
                    // Either current operand evaluation did not finish or produced an error
                    return operandExecutionResult;
                }

                if (this.Operands.Count == 1)
                {
                    operatorExecutionResult = operandExecutionResult.ReturnedOutput.ApplyOperator(this.Operation);
                }
                else
                {
                    // Current operand evaluation finished and did not produce an error
                    if (this.EvaluationResult == null)
                    {
                        this.EvaluationResult = EXEExecutionResult.Success();
                        this.EvaluationResult.ReturnedOutput = operandExecutionResult.ReturnedOutput;
                        continue;
                    }
                    else
                    {
                        operatorExecutionResult = this.EvaluationResult.ReturnedOutput.ApplyOperator(this.Operation, operandExecutionResult.ReturnedOutput);
                    }
                }

                if (!operatorExecutionResult.IsSuccess)
                {
                    this.EvaluationResult = operatorExecutionResult;
                    this.EvaluationState = EEvaluationState.HasBeenEvaluated;
                }

                if (!operatorExecutionResult.IsDone || !operatorExecutionResult.IsSuccess)
                {
                    // Either current operand evaluation did not finish or produced an error
                    return operatorExecutionResult;
                }

                this.EvaluationResult = EXEExecutionResult.Success();
                this.EvaluationResult.ReturnedOutput = operatorExecutionResult.ReturnedOutput;
            }

            this.EvaluationState = EEvaluationState.HasBeenEvaluated;

            return this.EvaluationResult;
        }

        public override EXEASTNodeBase Clone()
        {
            return new EXEASTNodeComposite(this.Operation, this.Operands.Select(operand => operand.Clone()).ToArray());
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeASTNodeComposite(this);
        }
    }
}
