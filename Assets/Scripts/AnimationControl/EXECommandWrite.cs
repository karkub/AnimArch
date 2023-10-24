﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Visualization.UI;

namespace OALProgramControl
{
    public class EXECommandWrite : EXECommand
    {
        private List<EXEASTNodeBase> Arguments { get; }
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

            string result = string.Join(", ", this.Arguments.Select(argument => argument.EvaluationResult.ReturnedOutput.ToText()));

            ConsolePanel.Instance.YieldOutput(result);

            return Success();
        }

        public override string ToCodeSimple()
        {
            String Result = "write(" + string.Join(", ", this.Arguments.Select(argument => argument.ToCode())) + ")";

            return Result;
        }

        public override EXECommand CreateClone()
        {
            return new EXECommandWrite(Arguments.Select(argument => argument.Clone()).ToList());
        }
    }
}