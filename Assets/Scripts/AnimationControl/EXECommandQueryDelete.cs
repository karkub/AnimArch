using System;

namespace OALProgramControl
{
    public class EXECommandQueryDelete : EXECommand
    {
        public EXEASTNodeBase DeletedVariable { get; }

        public EXECommandQueryDelete(EXEASTNodeBase deletedVariable)
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

            if (!OALProgram.ExecutionSpace.DestroyInstance(referencedValue.ClassInstance.OwningClass.Name, referencedValue.ClassInstance.UniqueID))
            {
                return Error
                (
                    "XEC2024",
                    string.Format
                    (
                        "Failed to destroy instance \"{0}\" of class \"{1}\".",
                        referencedValue.ClassInstance.UniqueID,
                        referencedValue.TypeClass.Name
                    )
                );
            }

            return Success();
        }
        public override void Accept(Visitor v)
        {
            v.VisitExeCommandQueryDelete(this);
        }

        protected override EXECommand CreateCloneCustom()
        {
            return new EXECommandQueryDelete(this.DeletedVariable.Clone() as EXEASTNodeAccessChain);
        }
    }
}
