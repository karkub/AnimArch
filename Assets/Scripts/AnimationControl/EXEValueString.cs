using Assets.Scripts.AnimationControl.BuiltIn;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OALProgramControl
{
    public class EXEValueString : EXEValuePrimitive
    {
        public string Value {get; protected set; }
        public override string TypeName => EXETypes.StringTypeName;
        public override bool CanHaveMethods => true;

        public EXEValueString(EXEValueString original)
        {
            CopyValues(original, this);
        }
        public EXEValueString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("String value cannot be determined from null nor from an empty string.");
            }

            if (!EXETypes.IsValidStringValue(value))
            {
                throw new ArgumentException(string.Format("\"{0}\" is not a valid string value.", value));
            }

            this.Value = value.Substring(1, value.Length - 2);
        }
        public override EXEValueBase DeepClone()
        {
            return new EXEValueString(this);
        }
        public override void Accept(IValueVisitor v)
        {
            v.VisitExeValueString(this);
        }
        protected override EXEExecutionResult AssignValueFromConcrete(EXEValueBase assignmentSource)
        {
            return assignmentSource.AssignValueTo(this);
        }
        public override EXEExecutionResult AssignValueTo(EXEValueString assignmentTarget)
        {
            if (!this.WasInitialized)
            {
                return UninitializedError();
            }

            CopyValues(this, assignmentTarget);
            this.WasInitialized = true;

            return EXEExecutionResult.Success();
        }
        private void CopyValues(EXEValueString source, EXEValueString target)
        {
            target.Value = source.Value;
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
                result.ReturnedOutput = new EXEValueInt(this.Value.Length);
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
                if (operand is not EXEValueString)
                {
                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                Debug.Log(string.Format("[CMP STR] '{0}' vs '{1}'", this.Value, (operand as EXEValueString).Value));
                result.ReturnedOutput = new EXEValueBool(this.Value == (operand as EXEValueString).Value);
                return result;
            }
            else if ("!=".Equals(operation))
            {
                if (operand is not EXEValueString)
                {
                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueBool(this.Value != (operand as EXEValueString).Value);
                return result;
            }
            else if ("+".Equals(operation))
            {
                if (operand is not EXEValueString)
                {
                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueString(this.Value + (operand as EXEValueString).Value);
                return result;
            }
            else if ("*".Equals(operation))
            {
                if (operand is not EXEValueInt)
                {
                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput
                    = new EXEValueString
                    (
                        Enumerable
                            .Range(0, (int)(operand as EXEValueInt).Value)
                            .Aggregate("", (acc, x) => acc + x)
                    );
                return result;
            }
           
            return base.ApplyOperator(operation, operand);
        }

        #region Methods
        private static CDClass DefiningClass = null;
        
        public override bool MethodExists(string methodName, bool includeInherited = false)
        {
            if (DefiningClass == null)
            {
                InitializeMethods();
            }

            return DefiningClass.MethodExists(methodName, includeInherited);
        }

        public override CDMethod FindMethod(string methodName, bool includeInherited = false)
        {
            if (DefiningClass == null)
            {
                InitializeMethods();
            }

            return DefiningClass.GetMethodByName(methodName, includeInherited);
        }

        private void InitializeMethods()
        {
            DefiningClass = new CDClass("String", null);

            InitializeJoinMethod();
            InitializeSplitMethod();
            InitializeFirstIndexOfMethod();
            InitializeAllIndexesOfMethod();
            InititializeLengthMethod();
            InitializeSubstringFromMethod();
            InitializeSubstringMethod();
            InitializeContainsMethod();
            InitializeReplaceMethod();
        }

        private void InitializeJoinMethod()
        {
            CDMethod MethodJoin = new CDMethod(DefiningClass, "Join", EXETypes.StringTypeName);
            MethodJoin.Parameters.Add
            (
                new CDParameter()
                {
                    Name = "joinedElements",
                    Type = EXETypes.StringTypeName + "[]"
                }
            );
            MethodJoin.ExecutableCode = new EXEScopeBuiltInMethod(MethodJoin, new BuiltInMethodStringJoin());
            DefiningClass.AddMethod(MethodJoin);
        }
        private void InitializeSplitMethod()
        {
            CDMethod MethodSplit = new CDMethod(DefiningClass, "Split", EXETypes.StringTypeName + "[]");
            MethodSplit.Parameters.Add
            (
                new CDParameter()
                {
                    Name = "delimiter",
                    Type = EXETypes.StringTypeName
                }
            );
            MethodSplit.ExecutableCode = new EXEScopeBuiltInMethod(MethodSplit, new BuiltInMethodStringSplit());
            DefiningClass.AddMethod(MethodSplit);
        }
        private void InitializeFirstIndexOfMethod()
        {
            CDMethod MethodFirstIndexOf = new CDMethod(DefiningClass, "FirstIndexOf", EXETypes.IntegerTypeName);
            MethodFirstIndexOf.Parameters.Add
            (
                new CDParameter()
                {
                    Name = "substring",
                    Type = EXETypes.StringTypeName
                }
            );
            MethodFirstIndexOf.ExecutableCode = new EXEScopeBuiltInMethod(MethodFirstIndexOf, new BuiltInMethodStringFirstIndexOf());
            DefiningClass.AddMethod(MethodFirstIndexOf);
        }
        private void InitializeAllIndexesOfMethod()
        {
            CDMethod MethodAllIndexOf = new CDMethod(DefiningClass, "AllIndexesOf", EXETypes.IntegerTypeName + "[]");
            MethodAllIndexOf.Parameters.Add
            (
                new CDParameter()
                {
                    Name = "delimiter",
                    Type = EXETypes.StringTypeName
                }
            );
            MethodAllIndexOf.ExecutableCode = new EXEScopeBuiltInMethod(MethodAllIndexOf, new BuiltInMethodStringAllIndexesOf());
            DefiningClass.AddMethod(MethodAllIndexOf);
        }
        private void InititializeLengthMethod()
        {
            CDMethod MethodLength = new CDMethod(DefiningClass, "Length", EXETypes.IntegerTypeName);
            MethodLength.ExecutableCode = new EXEScopeBuiltInMethod(MethodLength, new BuiltInMethodStringLength());
            DefiningClass.AddMethod(MethodLength);
        }
        private void InitializeSubstringFromMethod()
        {
            CDMethod MethodSubstringFrom = new CDMethod(DefiningClass, "SubstringFrom", EXETypes.StringTypeName);
            MethodSubstringFrom.Parameters.Add
            (
                new CDParameter()
                {
                    Name = "startIndex",
                    Type = EXETypes.IntegerTypeName
                }
            );
            MethodSubstringFrom.ExecutableCode = new EXEScopeBuiltInMethod(MethodSubstringFrom, new BuiltInMethodStringSubstringFrom());
            DefiningClass.AddMethod(MethodSubstringFrom);
        }
        private void InitializeSubstringMethod()
        {
            CDMethod MethodSubstringFrom = new CDMethod(DefiningClass, "Substring", EXETypes.StringTypeName);
            MethodSubstringFrom.Parameters.Add
            (
                new CDParameter()
                {
                    Name = "startIndex",
                    Type = EXETypes.IntegerTypeName
                }
            );
            MethodSubstringFrom.Parameters.Add
            (
                new CDParameter()
                {
                    Name = "count",
                    Type = EXETypes.IntegerTypeName
                }
            );
            MethodSubstringFrom.ExecutableCode = new EXEScopeBuiltInMethod(MethodSubstringFrom, new BuiltInMethodStringSubstring());
            DefiningClass.AddMethod(MethodSubstringFrom);
        }
        private void InitializeContainsMethod()
        {
            CDMethod MethodContains = new CDMethod(DefiningClass, "Contains", EXETypes.BooleanTypeName);
            MethodContains.Parameters.Add
            (
                new CDParameter()
                {
                    Name = "substring",
                    Type = EXETypes.StringTypeName
                }
            );
            MethodContains.ExecutableCode = new EXEScopeBuiltInMethod(MethodContains, new BuiltInMethodStringContains());
            DefiningClass.AddMethod(MethodContains);
        }
        private void InitializeReplaceMethod()
        {
            CDMethod MethodReplace = new CDMethod(DefiningClass, "Replace", EXETypes.StringTypeName);
            MethodReplace.Parameters.Add
            (
                new CDParameter()
                {
                    Name = "oldString",
                    Type = EXETypes.StringTypeName
                }
            );
            MethodReplace.Parameters.Add
            (
                new CDParameter()
                {
                    Name = "newString",
                    Type = EXETypes.StringTypeName
                }
            );
            MethodReplace.ExecutableCode = new EXEScopeBuiltInMethod(MethodReplace, new BuiltInMethodStringReplace());
            DefiningClass.AddMethod(MethodReplace);
        }
        #endregion

        public override string ToObjectDiagramText()
        {
            return string.Format("\"{0}\"", this.Value);
        }
    }
}