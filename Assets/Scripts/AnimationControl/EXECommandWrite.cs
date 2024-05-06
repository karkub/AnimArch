using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Visualization.UI;

namespace OALProgramControl
{
    public class EXECommandWrite : EXECommand
    {
        public List<EXEASTNodeBase> Arguments { get; }
        public string PromptText { get; set; }
        public EXECommandWrite() : this(new List<EXEASTNodeBase>()) {}
        public EXECommandWrite(List<EXEASTNodeBase> Arguments)
        {
            this.Arguments = Arguments;
        }

        protected override EXEExecutionResult Execute(OALProgram OALProgram)
        {
            EXEExecutionResult argumentEvaluationResult;
            foreach (EXEASTNodeBase argument in this.Arguments)
            {
                argumentEvaluationResult = argument.Evaluate(this.SuperScope, OALProgram);

                if (!HandleRepeatableASTEvaluation(argumentEvaluationResult))
                {
                    return argumentEvaluationResult;
                }
            }

            string result = string.Join(", ", this.Arguments.Select(argument => {
                        VisitorCommandToString visitor = VisitorCommandToString.BorrowAVisitor();
                        argument.EvaluationResult.ReturnedOutput.Accept(visitor);
                        return visitor.GetCommandStringAndResetStateNow();
            }));

            this.PromptText = result;
            
            MenuManager.Instance.Strategy.Write(result);

            return Success();
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeCommandWrite(this);
        }

        public override EXECommand CreateClone()
        {
            return new EXECommandWrite(Arguments.Select(argument => argument.Clone()).ToList());
        }
    }
}
