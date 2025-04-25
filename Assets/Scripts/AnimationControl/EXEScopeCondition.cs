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
        private List<EXEScopeCondition> _ElifScopes { get; set; }
        public IEnumerable<EXEScopeCondition> ElifScopes => _ElifScopes.Select(el => el);

        private EXEScope _ElseScope { get; set; }
        public EXEScope ElseScope
        {
            get => _ElseScope;
            set
            {
                _ElseScope = value;
                if (_ElseScope != null)
                {
                    _ElseScope.PreviousCondition = _ElifScopes.LastOrDefault() ?? this;
                }
            }
        }
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
            this._ElifScopes = ElifScopes;
            this.ElseScope = ElseScope;

            if (_ElifScopes.Any())
            {
                _ElifScopes.First().PreviousCondition = this;
            }

            for (int i = 1; i < _ElifScopes.Count; i++)
            {
                _ElifScopes[i].PreviousCondition = _ElifScopes[i - 1];
            }
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
                ElifScopes?.Select(x => (EXEScopeCondition)x.CreateClone()).ToList() ?? new List<EXEScopeCondition>(),
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

        public override EXECommand FindByCommandID(long CommandID)
        {
            EXECommand result = base.FindByCommandID(CommandID);
            if (result != null)
            {
                return result;
            }
            foreach (EXEScopeCondition elifScope in ElifScopes)
            {
                result = elifScope.FindByCommandID(CommandID);
                if (result != null)
                {
                    return result;
                }
            }
            if (ElseScope != null)
            {
                result = ElseScope.FindByCommandID(CommandID);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}
