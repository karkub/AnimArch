using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OALProgramControl
{
    public class EXEASTNodeIndexation : EXEASTNodeBase
    {
        public EXEASTNodeBase List {get;}
        public EXEASTNodeBase Index {get;}

        public EXEASTNodeIndexation(EXEASTNodeBase list, EXEASTNodeBase index) : base()
        {
            this.List = list;
            this.Index = index;
        }

        public override EXEExecutionResult Evaluate(EXEScopeBase currentScope, OALProgram currentProgramInstance, EXEASTNodeAccessChainContext valueContext = null)
        {
            if (this.EvaluationState == EEvaluationState.HasBeenEvaluated)
            {
                return this.EvaluationResult;
            }            

            this.EvaluationState = EEvaluationState.IsBeingEvaluated;

            EXEExecutionResult executionResult;
            EXEValueArray evaluatedList;
            EXEValueInt evaluatedIndex;

            executionResult = Index.Evaluate(currentScope, currentProgramInstance);
            if (!executionResult.IsSuccess)
            {
                this.EvaluationState = EEvaluationState.HasBeenEvaluated;
                this.EvaluationResult = executionResult;
            }
            if (!executionResult.IsDone || !executionResult.IsSuccess)
            {
                // Either current operand evaluation did not finish or produced an error
                return executionResult;
            }
            
            evaluatedIndex = executionResult.ReturnedOutput as EXEValueInt;
            if (evaluatedIndex == null)
            {
                this.EvaluationState = EEvaluationState.HasBeenEvaluated;
                this.EvaluationResult = EXEExecutionResult.Error("XEC3000", "Index used for indexing must be int!");
                return this.EvaluationResult;
            }
            if (evaluatedIndex.Value > UInt32.MaxValue)
            {
                this.EvaluationState = EEvaluationState.HasBeenEvaluated;
                this.EvaluationResult = EXEExecutionResult.Error("XEC3001", "Index used for indexing must not be bigger than uint32 max!");
                return this.EvaluationResult;
            }
            if (evaluatedIndex.Value < 0)
            {
                this.EvaluationState = EEvaluationState.HasBeenEvaluated;
                this.EvaluationResult = EXEExecutionResult.Error("XEC3002", "Index used for indexing must be bigger than 0!");
                return this.EvaluationResult;
            }


            executionResult = List.Evaluate(currentScope, currentProgramInstance, valueContext.Clone());
            if (!executionResult.IsSuccess)
            {
                this.EvaluationState = EEvaluationState.HasBeenEvaluated;
                this.EvaluationResult = executionResult;
            }
            if (!executionResult.IsDone || !executionResult.IsSuccess)
            {
                // Either current operand evaluation did not finish or produced an error
                return executionResult;
            }
            evaluatedList = executionResult.ReturnedOutput as EXEValueArray;
            if (evaluatedList == null)
            {
                this.EvaluationState = EEvaluationState.HasBeenEvaluated;
                this.EvaluationResult = EXEExecutionResult.Error("XEC3003", "List used for indexing to must be an array!");
                return this.EvaluationResult;
            }


            executionResult = evaluatedList.GetValueAt((UInt32)evaluatedIndex.Value);

            this.EvaluationState = EEvaluationState.HasBeenEvaluated;
            this.EvaluationResult = executionResult;

            return this.EvaluationResult;
        }

        public override EXEASTNodeBase Clone()
        {
            return new EXEASTNodeIndexation(this.List.Clone(), this.Index.Clone());
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeASTNodeIndexation(this);
        }
    }
}
