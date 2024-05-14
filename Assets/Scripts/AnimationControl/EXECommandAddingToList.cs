using System;
using System.Collections;
using System.Collections.Generic;

namespace OALProgramControl
{
    public class EXECommandAddingToList : EXECommandListOperation
    {
        public EXEASTNodeBase Array { get; }
        public EXEASTNodeBase AddedElement { get; }

        public EXECommandAddingToList(EXEASTNodeBase array, EXEASTNodeBase addedElement)
        {
            this.Array = array;
            this.AddedElement = addedElement;
        }

        public override EXEExecutionResult EvaluateItem(OALProgram OALProgram)
        {
           return this.AddedElement.Evaluate(this.SuperScope, OALProgram);
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
            return arrayEvaluationResult.ReturnedOutput.AppendElement(itemEvaluationResult.ReturnedOutput, OALProgram.ExecutionSpace);
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeCommandAddingToList(this);
        }

        public override EXECommand CreateClone()
        {
            return new EXECommandAddingToList(this.Array.Clone(), this.AddedElement.Clone());
        }
        public CDClassInstance GetAssignmentTargetOwner()
        {
            return (this.Array as EXEASTNodeAccessChain)?.GetFinalValueOwner();
        }
        public CDClassInstance GetAppendedElementInstance()
        {
            return (this.AddedElement.EvaluationResult.ReturnedOutput as EXEValueReference)?.ClassInstance;
        }
    }
}
