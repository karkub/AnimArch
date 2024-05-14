using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OALProgramControl
{
    public class EXEScopeLoopWhile : EXEScopeLoop
    {
        public EXEASTNodeBase Condition;

        public EXEScopeLoopWhile(EXEASTNodeBase Condition) : base()
        {
            this.Condition = Condition;
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeScopeLoopWhile(this);
        }

        protected override EXEScope CreateDuplicateScope()
        {
            return new EXEScopeLoopWhile(Condition.Clone());
        }

        protected override EXEExecutionResult HandleIterationStart(OALProgram OALProgram, out bool startNewIteration)
        {
            EXEExecutionResult conditionEvaluationResult = this.Condition.Evaluate(this.SuperScope, OALProgram);

            if (!HandleRepeatableASTEvaluation(conditionEvaluationResult))
            {
                startNewIteration = false;
                return conditionEvaluationResult;
            }

            if (conditionEvaluationResult.ReturnedOutput is not EXEValueBool)
            {
                startNewIteration = false;
                VisitorCommandToString visitor = new VisitorCommandToString();
                conditionEvaluationResult.ReturnedOutput.Accept(visitor);
                return Error("XEC2028", ErrorMessage.InvalidValueForType(visitor.GetCommandString(), EXETypes.BooleanTypeName));
            }

            startNewIteration = (conditionEvaluationResult.ReturnedOutput as EXEValueBool).Value;

            this.Condition = this.Condition.Clone();

            return Success();
        }
    }
}
