using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OALProgramControl
{
    public abstract class EXEASTNodeBase
    {
        public EEvaluationState EvaluationState { get; protected set; }
        public virtual EXEExecutionResult EvaluationResult { get; set; }
        public int BracketLevel { get; private set; }

        public EXEASTNodeBase()
        {
            this.EvaluationState = EEvaluationState.NotYetEvaluated;
            this.EvaluationResult = null;
        }

        public abstract EXEExecutionResult Evaluate(EXEScope currentScope, OALProgram currentProgramInstance, EXEASTNodeAccessChainContext valueContext = null);
        public abstract EXEASTNodeBase Clone();
        public void IncrementBracketLevel()
        {
            this.BracketLevel++;
        }

        public abstract void Accept(Visitor v);
    }
}
