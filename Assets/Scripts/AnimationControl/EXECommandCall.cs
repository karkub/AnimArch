using System.Linq;

namespace OALProgramControl
{
    public class EXECommandCall : EXECommand
    {
        public readonly EXEASTNodeAccessChain MethodAccessChain;
        public readonly EXEASTNodeMethodCall MethodCall;
        public EXEValueBase ReturnedValue;
        public MethodInvocationInfo CallInfo { get; private set; }
        private EXEValueBase CalledObject { get; set; }
        public string MethodAccessChainS { get; }
        public EXEScopeMethod InvokedMethod { get; private set; } 

        public EXECommandCall(EXEASTNodeAccessChain methodAccessChain, EXEASTNodeMethodCall methodCall)
        {
            if (methodAccessChain == null)
            {
                methodAccessChain = new EXEASTNodeAccessChain();
                methodAccessChain.AddElement(new EXEASTNodeLeaf(EXETypes.SelfReferenceName));
            }

            this.MethodAccessChain = methodAccessChain;
            this.MethodCall = methodCall;
            this.ReturnedValue = null;
            this.CallInfo = null;
            this.CalledObject = null;

            this.MethodAccessChainS = null;
            if (methodAccessChain != null)
            {
                VisitorCommandToString visitor = new VisitorCommandToString();
                methodAccessChain.Accept(visitor);
                this.MethodAccessChainS = visitor.GetCommandString();
            }
        }
        public EXECommandCall(EXEValueBase methodOwningObject, string accessChain, EXEASTNodeMethodCall methodCall)
        {
            this.MethodAccessChain = null;
            this.MethodCall = methodCall;
            this.ReturnedValue = null;
            this.CallInfo = null;
            this.CalledObject = methodOwningObject;
            this.MethodAccessChainS = accessChain;
        }

        protected override EXECommand CreateCloneCustom()
        {
            return new EXECommandCall(this.MethodAccessChain?.Clone() as EXEASTNodeAccessChain, this.MethodCall.Clone() as EXEASTNodeMethodCall);
        }

        protected override EXEExecutionResult Execute(OALProgram OALProgram)
        {
            if (this.CalledObject == null)
            {
                EXEExecutionResult methodOwningObjectEvaluationResult = this.MethodAccessChain.Evaluate(this.SuperScope, OALProgram);

                if (!HandleRepeatableASTEvaluation(methodOwningObjectEvaluationResult))
                {
                    return methodOwningObjectEvaluationResult;
                }

                this.CalledObject = methodOwningObjectEvaluationResult.ReturnedOutput;
            }

            EXEExecutionResult methodCallResolvingResult
                = this.MethodCall.Evaluate
                (
                    this.SuperScope,
                    OALProgram,
                    new EXEASTNodeAccessChainContext() { CurrentValue = this.CalledObject }
                );

            if (this.MethodCall.EvaluationState != EEvaluationState.HasBeenEvaluated)
            {
                if (!HandleRepeatableASTEvaluation(methodCallResolvingResult))
                {
                    return methodCallResolvingResult;
                }
            }

            // We are here, which means that all parameter values have been resolved successfully

            EXEScopeMethod MethodCode = this.MethodCall.Method.ExecutableCode;
            MethodCode.OwningObject = this.CalledObject;
            MethodCode.SetSuperScope(EXECommand.NullScope);
            MethodCode.CommandStack = this.CommandStack;
            MethodCode.MethodCallOrigin = this.MethodCall;

            this.CommandStack.Enqueue(MethodCode);

            this.InvokedMethod = MethodCode;

            EXEExecutionResult variableInitializationResult
                = MethodCode
                    .InitializeVariables
                    (
                        this.MethodCall
                            .Arguments
                            .Zip
                            (
                                this.MethodCall.Method.Parameters,
                                (EXEASTNodeBase argument, CDParameter parameter)
                                    => new EXEVariable(parameter.Name, argument.EvaluationResult.ReturnedOutput)
                            )
                    );

            if (!HandleSingleShotASTEvaluation(variableInitializationResult))
            {
                return variableInitializationResult;
            }

            // If invoking method of primitive type, no need to create the call info
            if (this.CalledObject is EXEValueReference)
            {
                CDClassInstance calledObject = (this.CalledObject as EXEValueReference).ClassInstance;
                CDMethod callerMethod = GetCurrentMethodScope().MethodDefinition;
                CDMethod calledMethod = MethodCode.MethodDefinition;
                CDRelationship relationship
                    = OALProgram
                        .RelationshipSpace
                        .GetRelationshipByClasses
                        (
                            GetCurrentMethodScope().MethodDefinition.OwningClass.Name,
                            MethodCode.MethodDefinition.OwningClass.Name
                        );
                CDClassInstance callerObject = (GetCurrentMethodScope().OwningObject as EXEValueReference).ClassInstance;

                this.CallInfo
                    = new MethodInvocationInfo
                    (
                        callerMethod, calledMethod, relationship, callerObject, calledObject
                    );
            }

            return Success();
        }
        public override void Accept(Visitor v) {
            v.VisitExeCommandCall(this);
        }
    }
}