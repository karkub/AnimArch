using System;
using System.Collections.Generic;
using System.Linq;

namespace OALProgramControl
{
    public class EXECommandRemovingFromList : EXECommandListOperation
    {
        public EXEASTNodeBase Array { get; }
        public EXEASTNodeBase Item { get; }

        public EXECommandRemovingFromList(EXEASTNodeBase array, EXEASTNodeBase item)
        {
            this.Array = array;
            this.Item = item;
        }
        public override EXEExecutionResult EvaluateItem(OALProgram OALProgram)
        {
            return this.Item.Evaluate(this.SuperScope, OALProgram);
        }

         public override EXEExecutionResult EvaluateArray(OALProgram OALProgram, out bool success)
         {
            EXEExecutionResult arrayEvaluationResult = this.Array.Evaluate(this.SuperScope, OALProgram);
            success = false;

             if (!HandleRepeatableASTEvaluation(arrayEvaluationResult))
            {
                return arrayEvaluationResult;
            }

            success = true;
            return arrayEvaluationResult;
        }


        public override EXEExecutionResult PerformOperation(OALProgram OALProgram, EXEExecutionResult arrayEvaluationResult, EXEExecutionResult itemEvaluationResult)
        {
            return arrayEvaluationResult.ReturnedOutput.RemoveElement(itemEvaluationResult.ReturnedOutput, OALProgram.ExecutionSpace);
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeCommandRemovingFromList(this);
        }

        public override EXECommand CreateClone()
        {
            return new EXECommandRemovingFromList(this.Array.Clone() as EXEASTNodeAccessChain, this.Item.Clone());
        }
    }
}
