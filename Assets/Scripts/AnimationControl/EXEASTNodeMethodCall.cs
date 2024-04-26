using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OALProgramControl
{
    public class EXEASTNodeMethodCall : EXEASTNodeBase, IReturnCollector
    {
        public readonly string MethodName;
        public readonly List<EXEASTNodeBase> Arguments;
        public CDMethod Method { get; private set; }
        public EXEValueBase OwningObject { get; private set; }
        private EXEExecutionResult evaluationResult { get; set; }
        public bool ExpectingReturn { get; set; }
        public bool ReturnCollected { get; set; }
        public override EXEExecutionResult EvaluationResult
        {
            get => this.evaluationResult;
            set
            {
                this.evaluationResult = value;

                if (this.evaluationResult != null && this.evaluationResult.IsDone)
                {
                    this.EvaluationState = EEvaluationState.HasBeenEvaluated;
                }
            }
        }

        public EXEASTNodeMethodCall(string methodName) : this(methodName, new List<EXEASTNodeBase>()) {}
        public EXEASTNodeMethodCall(string methodName, List<EXEASTNodeBase> arguments)
        {
            this.MethodName = methodName;
            this.Arguments = arguments;
            this.Method = null;
            this.ExpectingReturn = false;
            this.ReturnCollected = false;
        }

        public CDMethod GetOriginatorData()
        {
            return Method;
        }

        public override EXEExecutionResult Evaluate(EXEScopeBase currentScope, OALProgram currentProgramInstance, EXEASTNodeAccessChainContext valueContext = null)
        {
            // Prevent infinite loop if there is no return in method that is supposed to return something
            if (this.ExpectingReturn && !this.ReturnCollected)
            {
                return EXEExecutionResult.Error
                    (
                        "XEC2042",
                        string.Format
                        (
                            "Tried to retrieve value from a method that did not return anything. The method is '{0}.{1}'.",
                            this.Method.OwningClass.Name,
                            this.MethodName
                        )
                    );
            }

            if (this.EvaluationState == EEvaluationState.HasBeenEvaluated)
            {
                return this.EvaluationResult;
            }

            this.EvaluationState = EEvaluationState.IsBeingEvaluated;

            if (valueContext == null || valueContext.CurrentValue == null)
            {
                // We want to access a method of the current method owning object
                EXEVariable selfVariable = currentScope.FindVariable(EXETypes.SelfReferenceName);
                OwningObject = selfVariable.Value;
                valueContext = new EXEASTNodeAccessChainContext() { CurrentAccessChain = string.Empty };
            }
            else
            {
                // We want to access a method of an object given by the context
                OwningObject = valueContext.CurrentValue;
            }

            if (OwningObject.MethodExists(this.MethodName, true))
            {
                // Method exists
                Method = OwningObject.FindMethod(this.MethodName, true);
            }
            else
            {
                // Method does not exist, time to raise an error
                return EXEExecutionResult.Error("XEC2015", ErrorMessage.MethodNotFoundOnClass(this.MethodName, this.OwningObject.TypeName));
            }

            if (this.Arguments.Count != this.Method.Parameters.Count)
            {
                return EXEExecutionResult.Error
                    (
                        "XEC2016",
                        ErrorMessage.InvalidParameterCount
                        (
                            this.Method.OwningClass.Name,
                            this.MethodName,
                            this.Method.Parameters.Count,
                            this.Arguments.Count
                        )
                    );
            }

            // Now, let us evaluate the args and invoke the method
            EXEExecutionResult argumentExecutionResult;
            for (int i = 0; i < this.Arguments.Count; i++)
            {
                argumentExecutionResult = this.Arguments[i].Evaluate(currentScope, currentProgramInstance);

                if (!argumentExecutionResult.IsSuccess)
                {
                    this.EvaluationState = EEvaluationState.HasBeenEvaluated;
                    this.EvaluationResult = argumentExecutionResult;
                }

                if (!argumentExecutionResult.IsDone || !argumentExecutionResult.IsSuccess)
                {
                    // Either current argument evaluation did not finish or produced an error
                    return argumentExecutionResult;
                }

                // Current argument evaluation finished and did not produce an error

                // Check if the returned value matches the expected parameter type
                if (!EXETypes.CanBeAssignedTo(argumentExecutionResult.ReturnedOutput, this.Method.Parameters[i].Type, currentProgramInstance.ExecutionSpace))
                {
                    VisitorCommandToString visitor = VisitorCommandToString.BorrowAVisitor();
                    argumentExecutionResult.ReturnedOutput.Accept(visitor);
                    return EXEExecutionResult.Error
                    (
                        "XEC2017",
                        ErrorMessage.InvalidParameterValue
                        (
                            this.Method.OwningClass.Name,
                            this.MethodName,
                            this.Method.Parameters[i].Name,
                            this.Method.Parameters[i].Type, visitor.GetCommandStringAndResetStateNow()
                        )
                    );
                }
            }

            // All arguments have been evaluated and without an error
            this.ExpectingReturn = true;
            this.EvaluationResult = EXEExecutionResult.Success();
            this.EvaluationResult.PendingCommand = new EXECommandCall(OwningObject, valueContext.CurrentAccessChain, this);
            return this.EvaluationResult;
        }

        public override EXEASTNodeBase Clone()
        {
            return new EXEASTNodeMethodCall(this.MethodName, this.Arguments.Select(arg => arg.Clone()).ToList());
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeASTNodeMethodCall(this);
        }

        public void CollectReturn(EXEValueBase returnedValue)
        {
            // This actually performs the assignment
            this.EvaluationResult = EXEExecutionResult.Success();
            this.EvaluationResult.ReturnedOutput = returnedValue;
            this.ReturnCollected = true;
        }
    }
}
