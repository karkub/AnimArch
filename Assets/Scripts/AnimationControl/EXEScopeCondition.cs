using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OALProgramControl
{
    public class EXEScopeCondition : EXEScope
    {
        public EXEASTNodeBase Condition { get; set; }
        public List<EXEScopeCondition> ElifScopes { get; private set; }
        public EXEScope ElseScope { get; set; }
        private IEnumerable<EXEScopeCondition> AllConditionedScopes
        {
            get
            {
                yield return this;

                foreach (EXEScopeCondition elifScope in this.ElifScopes)
                {
                    yield return elifScope;
                }
            }
        }

        public EXEScopeCondition(EXEASTNodeBase Condition) : this(Condition, new List<EXEScopeCondition>(), null) {}
        public EXEScopeCondition(EXEASTNodeBase Condition, List<EXEScopeCondition> ElifScopes, EXEScope ElseScope) : base()
        {
            this.Condition = Condition;
            this.ElifScopes = ElifScopes;
            this.ElseScope = ElseScope;
        }

        public override void SetSuperScope(EXEScopeBase SuperScope)
        {
            base.SetSuperScope(SuperScope);

            if (this.ElifScopes != null)
            {
                foreach (EXEScope ElifScope in this.ElifScopes)
                {
                    ElifScope.SetSuperScope(this.GetSuperScope());
                }
            }

            if (this.ElseScope != null)
            {
                this.ElseScope.SetSuperScope(this.GetSuperScope());
            }
        }

        public void AddElifScope(EXEScopeCondition ElifScope)
        {
            if (this.ElifScopes == null)
            {
                this.ElifScopes = new List<EXEScopeCondition>();
            }

            this.ElifScopes.Add(ElifScope);
            ElifScope.SetSuperScope(this.GetSuperScope());
        }

        protected override EXEExecutionResult Execute(OALProgram OALProgram)
        {
            foreach (EXEScopeCondition scope in this.AllConditionedScopes)
            {
                EXEExecutionResult conditionEvaluationResult = scope.Condition.Evaluate(scope.SuperScope, OALProgram);

                if (!HandleRepeatableASTEvaluation(conditionEvaluationResult))
                {
                    return conditionEvaluationResult;
                }

                VisitorCommandToString visitor = new VisitorCommandToString();
                conditionEvaluationResult.ReturnedOutput.Accept(visitor);
                if (conditionEvaluationResult.ReturnedOutput is not EXEValueBool)
                {
                    return Error("XEC2027", ErrorMessage.InvalidValueForType(visitor.GetCommandString(), EXETypes.BooleanTypeName));
                }

                if ((conditionEvaluationResult.ReturnedOutput as EXEValueBool).Value)
                {
                    AddCommandsToStack(scope.Commands);
                    return Success();
                }
            }

            if (this.ElseScope != null)
            {
                AddCommandsToStack(this.ElseScope.Commands);
            }

            return Success();
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeScopeCondition(this);
        }

        protected override EXEScope CreateDuplicateScope()
        {
            return new EXEScopeCondition
            (
                Condition.Clone(),
                ElifScopes?.Select(x => (EXEScopeCondition)x.CreateCloneCustom()).ToList() ?? new List<EXEScopeCondition>(),
                ElseScope == null ? null : (EXEScope)ElseScope.CreateClone()
            );
        }

        public override void SetCommandID()
        {
            base.SetCommandID();
            foreach (EXECommand command in ElifScopes)
            {
                command.SetCommandID();
            }
            
            if (ElseScope != null)
            {
                ElseScope.SetCommandID();
            }
        }
    }
}
