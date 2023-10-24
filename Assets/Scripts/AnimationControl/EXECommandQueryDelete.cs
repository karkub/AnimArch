﻿using System;

namespace OALProgramControl
{
    public class EXECommandQueryDelete : EXECommand
    {
        private EXEASTNodeAccessChain DeletedVariable;

        public EXECommandQueryDelete(EXEASTNodeAccessChain deletedVariable)
        {
            this.DeletedVariable = deletedVariable;
        }

        protected override EXEExecutionResult Execute(OALProgram OALProgram)
        {
            EXEExecutionResult deletedVariableExecutionResult = DeletedVariable.Evaluate(this.SuperScope, OALProgram);

            if (!HandleRepeatableASTEvaluation(deletedVariableExecutionResult))
            {
                return deletedVariableExecutionResult;
            }

            EXEValueReference referencedValue = deletedVariableExecutionResult.ReturnedOutput as EXEValueReference;

            if (!OALProgram.ExecutionSpace.DestroyInstance(referencedValue.TypeClass.Name, referencedValue.ClassInstance.UniqueID))
            {
                return Error
                (
                    string.Format
                    (
                        "Failed to destroy instance \"{0}\" of class \"{1}\".",
                        referencedValue.ClassInstance.UniqueID,
                        referencedValue.TypeClass.Name
                    ),
                    "XEC2024"
                );
            }

            return Success();
        }
        public override string ToCodeSimple()
        {
            return "delete object instance " + this.DeletedVariable.ToCode();
        }

        public override EXECommand CreateClone()
        {
            return new EXECommandQueryDelete(this.DeletedVariable.Clone() as EXEASTNodeAccessChain);
        }
    }
}