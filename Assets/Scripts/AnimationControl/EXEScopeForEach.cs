using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OALProgramControl
{
    public class EXEScopeForEach : EXEScopeLoop
    {
        public String IteratorName { get; set; }
        public EXEASTNodeBase Iterable { get; set; }
        private int CurrentIterableIndex;

        public EXEScopeForEach(String iteratorName, EXEASTNodeBase iterable) : base()
        {
            this.IteratorName = iteratorName;
            this.Iterable = iterable;
            this.CurrentIterableIndex = 0;
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeScopeForEach(this);
        }

        protected override EXEScope CreateDuplicateScope()
        {
            return new EXEScopeForEach(IteratorName, Iterable.Clone());
        }

        protected override EXEExecutionResult HandleIterationStart(OALProgram OALProgram, out bool startNewIteration)
        {
            startNewIteration = false;

            EXEExecutionResult iterableEvaluationResult = this.Iterable.Evaluate(this.SuperScope, OALProgram);

            if (!HandleRepeatableASTEvaluation(iterableEvaluationResult))
            {
                return iterableEvaluationResult;
            }

            VisitorCommandToString visitor2 = new VisitorCommandToString();
            iterableEvaluationResult.ReturnedOutput.Accept(visitor2);
            if (iterableEvaluationResult.ReturnedOutput is not EXEValueArray)
            {
                return Error("XEC2028", ErrorMessage.IsNotIterable(visitor2.GetCommandString()));
            }

            EXEValueArray iterableValue = iterableEvaluationResult.ReturnedOutput as EXEValueArray;

            VisitorCommandToString visitor3 = new VisitorCommandToString();
            iterableValue.Accept(visitor3);
            if (EXETypes.UnitializedName.Equals(visitor3.GetCommandString()))
            {
                return Error("XEC2027", "Cannot iterate over uninitialized collection.");
            }

            EXEVariable iteratorVariable = this.SuperScope.FindVariable(this.IteratorName);

            if (iteratorVariable != null)
            {
                if (!EXETypes.CanBeAssignedTo(iterableValue.ElementTypeName, iteratorVariable.Value.TypeName, OALProgram.ExecutionSpace))
                {
                    VisitorCommandToString visitor = new VisitorCommandToString();
                    this.Iterable.Accept(visitor);
                    return Error
                    (

                        "XEC2029",
                        string.Format
                        (
                            "Iterator of type {0} trying to iterate collection of {1}. At iterable index {2}: Iterable elements are of types: {3}. {4}",
                            iteratorVariable.Value.TypeName,
                            iterableValue.ElementTypeName,
                            this.CurrentIterableIndex,
                            string.Join(", ", iterableValue.Elements.Select(element => element.TypeName)),
                            ErrorMessage.IterableAndIteratorTypeMismatch
                            (
                                visitor.GetCommandString(),
                                iterableValue.ElementTypeName,
                                this.IteratorName,
                                iteratorVariable.Value.TypeName
                            )
                        )
                    );
                }
            }
            else if (this.CurrentIterableIndex == 0)
            {
                iteratorVariable
                        = new EXEVariable
                        (
                            this.IteratorName,
                            EXETypes.DefaultValue(iterableValue.ElementTypeName, OALProgram.ExecutionSpace)
                        );

                EXEExecutionResult iteratorVariableCreationResult = this.SuperScope.AddVariable(iteratorVariable);

                if (!HandleSingleShotASTEvaluation(iteratorVariableCreationResult))
                {
                    return iteratorVariableCreationResult;
                }
            }
            else
            {
                return Error("XEC2034", "Unexpected modification of iterator variable during foreach loop execution.");
            };


            if (this.CurrentIterableIndex < iterableValue.Elements.Count)
            {
                EXEExecutionResult iteratorAssignmentResult
                    = iteratorVariable.Value.AssignValueFrom(iterableValue.GetElementAt(this.CurrentIterableIndex));

                if (!HandleSingleShotASTEvaluation(iteratorAssignmentResult))
                {
                    return iteratorAssignmentResult;
                }

                this.CurrentIterableIndex++;
                startNewIteration = true;
            }
            else
            {
                startNewIteration = false;
            }

            return Success();
        }
    }
}