using System;
using UnityEditor;
using UnityEngine;

namespace OALProgramControl
{
    public abstract class EXEValueBase
    {
        public abstract string TypeName { get; }
        public virtual bool CanHaveAttributes { get { return false; } }
        public virtual bool CanHaveMethods { get { return false; } }
        public bool WasInitialized = true;
        public abstract EXEValueBase DeepClone();
        public abstract string ToObjectDiagramText();
        public virtual bool AttributeExists(string attributeName)
        {
            return false;
        }
        public virtual bool MethodExists(string methodName, bool includeInherited = false)
        {
            return false;
        }
        public virtual EXEExecutionResult RetrieveAttributeValue(string attributeName)
        {
            return EXEExecutionResult.Error("XEC2004", string.Format("Tried to find attribute \"{0}\" on something that does not have any attribute. {1}", attributeName, this.ToString()));
        }
        public virtual CDMethod FindMethod(string methodName, bool includeInherited = false)
        {
            return null;
        }
        public EXEExecutionResult AssignValueFrom(EXEValueBase assignmentSource)
        {
            EXEExecutionResult assignmentResult = AssignValueFromConcrete(assignmentSource);

            if (!assignmentResult.IsDone || !assignmentResult.IsSuccess)
            {
                return assignmentResult;
            }

            this.WasInitialized = true;

            return EXEExecutionResult.Success();
        }
        protected abstract EXEExecutionResult AssignValueFromConcrete(EXEValueBase assignmentSource);
        public virtual EXEExecutionResult AssignValueTo(EXEValueBool assignmentTarget)
        {
            return EXEExecutionResult.Error("XEC2007", AssignmentErrorMessage(this.TypeName, assignmentTarget.TypeName));
        }
        public virtual EXEExecutionResult AssignValueTo(EXEValueInt assignmentTarget)
        {
            return EXEExecutionResult.Error("XEC2008", AssignmentErrorMessage(this.TypeName, assignmentTarget.TypeName));
        }
        public virtual EXEExecutionResult AssignValueTo(EXEValueReal assignmentTarget)
        {
            return EXEExecutionResult.Error("XEC2009", AssignmentErrorMessage(this.TypeName, assignmentTarget.TypeName));
        }
        public virtual EXEExecutionResult AssignValueTo(EXEValueString assignmentTarget)
        {
            return EXEExecutionResult.Error("XEC2010", AssignmentErrorMessage(this.TypeName, assignmentTarget.TypeName));
        }
        public virtual EXEExecutionResult AssignValueTo(EXEValueReference assignmentTarget)
        {
            return EXEExecutionResult.Error("XEC2011", AssignmentErrorMessage(this.TypeName, assignmentTarget.TypeName));
        }
        public virtual EXEExecutionResult AssignValueTo(EXEValueArray assignmentTarget)
        {
            return EXEExecutionResult.Error("XEC2012", AssignmentErrorMessage(this.TypeName, assignmentTarget.TypeName));
        }
        public virtual EXEExecutionResult IsEqualTo(EXEValueBase comparedValue)
        {
            return ApplyOperator("==", comparedValue);
        }
        public virtual EXEExecutionResult AppendElement(EXEValueBase appendedElement, CDClassPool classPool)
        {
            VisitorCommandToString visitor = VisitorCommandToString.BorrowAVisitor();
            appendedElement.Accept(visitor);
            return EXEExecutionResult.Error("XEC2018", string.Format("Cannot append element \"{0}\" of type \"{1}\" to \"{2}\".", visitor.GetCommandStringAndResetStateNow(), appendedElement.TypeName, this.TypeName));
        }
        public virtual EXEExecutionResult RemoveElement(EXEValueBase removedElement, CDClassPool classPool)
        {
            VisitorCommandToString visitor = VisitorCommandToString.BorrowAVisitor();
            removedElement.Accept(visitor);
            return EXEExecutionResult.Error("XEC2019", string.Format("Cannot remove element \"{0}\" of type \"{1}\" from \"{2}\".", visitor.GetCommandStringAndResetStateNow(), removedElement.TypeName, this.TypeName));
        }
        public virtual EXEExecutionResult ApplyOperator(string operation)
        {
            VisitorCommandToString visitor = VisitorCommandToString.BorrowAVisitor();
            this.Accept(visitor);
            return EXEExecutionResult.Error("XEC2017", string.Format("Cannot apply unary operation \"{0}\" on operand \"{1}\".", operation, visitor.GetCommandStringAndResetStateNow()));
        }
        public virtual EXEExecutionResult ApplyOperator(string operation, EXEValueBase operand)
        {
            EXEExecutionResult result = null;

            if ("==".Equals(operation))
            {
                if (operand is not EXEValueString)
                {
                    return BinaryOperatorError(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueBool(this.WasInitialized == operand.WasInitialized);
                return result;
            }
            else if ("!=".Equals(operation))
            {
                if (operand is not EXEValueString)
                {
                    return BinaryOperatorError(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueBool(this.WasInitialized != operand.WasInitialized);
                return result;
            }

            return BinaryOperatorError(operation, operand);
        }
        private EXEExecutionResult BinaryOperatorError(string operation, EXEValueBase operand)
        {
            VisitorCommandToString visitor = VisitorCommandToString.BorrowAVisitor();
            this.Accept(visitor);
            VisitorCommandToString visitor2 = VisitorCommandToString.BorrowAVisitor();
            operand.Accept(visitor2);
            return EXEExecutionResult.Error("XEC2018", string.Format("Cannot apply binary operation \"{0}\" on operands \"{1}\" and \"{2}\".", operation, visitor.GetCommandStringAndResetStateNow(), visitor2.GetCommandStringAndResetStateNow()));
        }
        protected virtual EXEExecutionResult UninitializedError()
        {
            return EXEExecutionResult.Error("XEC2013", "Tried to manipulate uninitialized value.");
        }
        protected string AssignmentErrorMessage(string sourceType, string targetType)
        {
            return string.Format("Cannot assign value of type \"{0}\" to a variable of type \"{1}\".", sourceType, targetType);
        }
        public override string ToString()
        {
            return string.Format("EXEValueType: '{0}', TypeName: '{1}'", this.GetType().Name, this.TypeName ?? string.Empty);
        }

        public abstract void Accept(IValueVisitor v);

        public virtual EXEExecutionResult GetValueAt(UInt32 index)
        {
            return EXEExecutionResult.Error("XEC3007", "EXEValueBase cannot return value at index!");
        }
    }
}