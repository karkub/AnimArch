using System;
using System.Collections.Generic;
using System.Linq;

namespace OALProgramControl
{
    public class EXECommandCreateList : EXECommandListOperation
    {
        public string ArrayType { get; }
        private string VariableType { get { return this.ArrayType + "[]"; } }
        public EXEASTNodeAccessChain AssignmentTarget { get; }
        public List<EXEASTNodeBase> Items { get; }

        public EXECommandCreateList(string arrayType, EXEASTNodeAccessChain assignmentTarget, List<EXEASTNodeBase> items)
        {
            this.ArrayType = arrayType;
            this.AssignmentTarget = assignmentTarget;
            this.Items = items;
        }

        public override EXEExecutionResult EvaluateItem(OALProgram OALProgram)
        {
           return this.AssignmentTarget.Evaluate(this.SuperScope, OALProgram, new EXEASTNodeAccessChainContext() { CreateVariableIfItDoesNotExist = true, VariableCreationType = this.VariableType });
        }

        public override EXEExecutionResult EvaluateArray(OALProgram OALProgram, out bool success)
        {
            success = false;
            EXEExecutionResult itemEvaluationResult = null;
            // Prepare the items to populate the array with
            foreach (EXEASTNodeBase item in this.Items)
            {
                itemEvaluationResult = item.Evaluate(this.SuperScope, OALProgram);

                if (!HandleRepeatableASTEvaluation(itemEvaluationResult))
                {   
                    return itemEvaluationResult;
                }
            }

            // Create the array
            EXEExecutionResult arrayCreationResult = EXEValueArray.CreateArray(this.VariableType);
            if (!HandleSingleShotASTEvaluation(arrayCreationResult))
            {
                return arrayCreationResult;
            }

            // Populate the array
            EXEValueArray array = arrayCreationResult.ReturnedOutput as EXEValueArray;
            array.InitializeEmptyArray();

            EXEExecutionResult itemAppendmentResult = null;
            foreach (EXEASTNodeBase item in this.Items)
            {
                itemAppendmentResult = array.AppendElement(item.EvaluationResult.ReturnedOutput, OALProgram.ExecutionSpace);
                if (!HandleSingleShotASTEvaluation(itemAppendmentResult))
                {
                    return itemAppendmentResult;
                }
            }
            
            success = true;
            return arrayCreationResult;
        }
        public override EXEExecutionResult PerformOperation(OALProgram OALProgram, EXEExecutionResult arrayEvaluationResult, EXEExecutionResult itemEvaluationResult){
            return itemEvaluationResult.ReturnedOutput.AssignValueFrom(arrayEvaluationResult.ReturnedOutput);
        }
        
        public override void Accept(Visitor v)
        {
            v.VisitExeCommandCreateList(this);
        }

        public override EXECommand CreateClone()
        {
            return new EXECommandCreateList
            (
                this.ArrayType,
                this.AssignmentTarget.Clone() as EXEASTNodeAccessChain,
                this.Items
                    .Select(item => item.Clone())
                    .ToList()
            );
        }
    }
}
