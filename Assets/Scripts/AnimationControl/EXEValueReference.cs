using System;
using UnityEditor;
using UnityEngine;

namespace OALProgramControl
{
    public class EXEValueReference : EXEValueBase
    {
        public override string TypeName => TypeClass.Name;
        public override bool CanHaveAttributes => true;
        public override bool CanHaveMethods => true;
        public CDClass TypeClass { get; protected set; }
        public CDClassInstance ClassInstance { get; protected set; }
        public EXEValueReference(CDClass typeClass)
        {
            if (typeClass == null)
            {
                throw new ArgumentNullException("typeClass");
            }

            this.ClassInstance = null;
            this.TypeClass = typeClass;
        }
        public EXEValueReference(CDClassInstance classInstance)
        {
            this.ClassInstance = classInstance;
            this.TypeClass = classInstance.OwningClass;
        }
        public EXEValueReference(EXEValueReference original)
        {
            CopyValues(original, this);
            this.TypeClass = original.TypeClass;
        }
        public override EXEValueBase DeepClone()
        {
            return new EXEValueReference(this);
        }
        public override void Accept(IValueVisitor v)
        {
            v.VisitExeValueReference(this);
        }
        public override bool AttributeExists(string attributeName)
        {
            if (!this.WasInitialized)
            {
                return false;
            }

            return this.ClassInstance.OwningClass.GetAttributeByName(attributeName, true) != null;
        }
        public override bool MethodExists(string methodName, bool includeInherited = false)
        {
            if (!this.WasInitialized)
            {
                return false;
            }

            return FindMethod(methodName, includeInherited) != null;
        }
        public override EXEExecutionResult RetrieveAttributeValue(string attributeName)
        {
            if (!this.WasInitialized)
            {
                return UninitializedError();
            }

            EXEValueBase attributeValue = this.ClassInstance.GetAttributeValue(attributeName);

            if (attributeValue == null)
            {
                return EXEExecutionResult.Error("XEC2005", ErrorMessage.AttributeNotFoundOnClass(attributeName, ClassInstance.OwningClass));
            }

            EXEExecutionResult executionResult = EXEExecutionResult.Success();
            executionResult.ReturnedOutput = attributeValue;
            return executionResult;
        }
        public override CDMethod FindMethod(string methodName, bool includeInherited = false)
        {
            if (!this.WasInitialized)
            {
                return null;
            }

            return this.ClassInstance.OwningClass.GetMethodByName(methodName, includeInherited);
        }
        protected override EXEExecutionResult AssignValueFromConcrete(EXEValueBase assignmentSource)
        {
            return assignmentSource.AssignValueTo(this);
        }
        public override EXEExecutionResult AssignValueTo(EXEValueReference assignmentTarget)
        {
            if (!this.WasInitialized)
            {
                return UninitializedError();
            }

            if (this.TypeClass != null && this.ClassInstance == null)
            {
                if (this.TypeClass.CanBeAssignedTo(assignmentTarget.TypeClass))
                {
                    CopyValues(this, assignmentTarget);
                    this.WasInitialized = true;

                    return EXEExecutionResult.Success();
                }
                else
                {
                    return base.AssignValueTo(assignmentTarget);
                }
            }
            else if (!this.TypeClass.CanBeAssignedTo(assignmentTarget.TypeClass))
            {
                return base.AssignValueTo(assignmentTarget);
            }

            CopyValues(this, assignmentTarget);

            return EXEExecutionResult.Success();
        }
        private void CopyValues(EXEValueReference source, EXEValueReference target)
        {
            target.ClassInstance = source.ClassInstance;
        }
        public override EXEExecutionResult ApplyOperator(string operation)
        {
            if (!this.WasInitialized)
            {
                return UninitializedError();
            }

            EXEExecutionResult result = null;

            if ("cardinality".Equals(operation))
            {
                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueInt(this.ClassInstance == null ? 0 : 1);
                return result;
            }
            else if ("empty".Equals(operation))
            {
                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueBool(this.ClassInstance == null);
                return result;
            }
            else if ("not_empty".Equals(operation))
            {
                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueBool(this.ClassInstance != null);
                return result;
            }

            else if("type_name".Equals(operation))
            {
                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueString(string.Format(@"""{0}""", this.TypeName));
                return result;
            }

            return base.ApplyOperator(operation);
        }
        public override EXEExecutionResult ApplyOperator(string operation, EXEValueBase operand)
        {
            if (!this.WasInitialized || !operand.WasInitialized)
            {
                return base.ApplyOperator(operation, operand);
            }

            EXEExecutionResult result = null;

            if ("==".Equals(operation))
            {
                if (operand is not EXEValueReference)
                {
                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueBool(this.ClassInstance?.UniqueID == (operand as EXEValueReference).ClassInstance?.UniqueID);
                return result;
            }
            else if ("!=".Equals(operation))
            {
                if (operand is not EXEValueString)
                {
                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueBool(this.ClassInstance?.UniqueID != (operand as EXEValueReference).ClassInstance?.UniqueID);
                return result;
            }

            return base.ApplyOperator(operation, operand);
        }
        public void Dereference()
        {
            this.ClassInstance = null;
        }

        public override string ToObjectDiagramText()
        {
            return this.ClassInstance == null ? EXETypes.UnitializedName : this.ClassInstance.UniqueID.ToString();
        }

    }
}