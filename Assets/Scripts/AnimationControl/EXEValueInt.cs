using System;
using UnityEditor;
using UnityEngine;

namespace OALProgramControl
{
    public class EXEValueInt : EXEValuePrimitive
    {
        public long Value { get; protected set; }
        public override string TypeName => EXETypes.IntegerTypeName;

        public EXEValueInt(EXEValueInt original)
        {
            CopyValues(original, this);
        }
        public EXEValueInt(string value)
        {
            if (!EXETypes.IsValidIntValue(value))
            {
                throw new ArgumentException(string.Format("\"{0}\" is not a valid int value.", value));
            }

            this.Value = long.Parse(value);
        }
        public EXEValueInt(long value)
        {
            this.Value = value;
        }
        public EXEValueInt(decimal value)
        {
            this.Value = (long)value;
        }
        public override EXEValueBase DeepClone()
        {
            return new EXEValueInt(this);
        }

        public override void Accept(IValueVisitor v)
        {
            v.VisitExeValueInt(this);
        }
        protected override EXEExecutionResult AssignValueFromConcrete(EXEValueBase assignmentSource)
        {
            return assignmentSource.AssignValueTo(this);
        }
        public override EXEExecutionResult AssignValueTo(EXEValueInt assignmentTarget)
        {
            if (!this.WasInitialized)
            {
                return UninitializedError();
            }

            CopyValues(this, assignmentTarget);
            this.WasInitialized = true;

            return EXEExecutionResult.Success();
        }
        public override EXEExecutionResult AssignValueTo(EXEValueReal assignmentTarget)
        {
            if (!this.WasInitialized)
            {
                return UninitializedError();
            }

            if (!EXEExecutionGlobals.AllowPromotionOfIntegerToReal)
            {
                return EXEExecutionResult.Error("XEC2017", "Assigning integer to real is not currently allowed.");
            }

            EXEValueReal.CopyValues(new EXEValueReal(this.Value), assignmentTarget);

            return EXEExecutionResult.Success();
        }
        public static void CopyValues(EXEValueInt source, EXEValueInt target)
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
            if ("type_name".Equals(operation))
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


            if ("<=".Equals(operation))
            {
                if (operand is not EXEValueInt)
                {
                    if (operand is EXEValueReal)
                    {
                        return new EXEValueReal(this).ApplyOperator(operation, operand);
                    }
                    
                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueBool(this.Value <= (operand as EXEValueInt).Value);
                return result;
            }
            else if (">=".Equals(operation))
            {
                if (operand is not EXEValueInt)
                {
                    if (operand is EXEValueReal)
                    {
                        return new EXEValueReal(this).ApplyOperator(operation, operand);
                    }

                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueBool(this.Value >= (operand as EXEValueInt).Value);
                return result;
            }
            else if("<".Equals(operation))
            {
                if (operand is not EXEValueInt)
                {
                    if (operand is EXEValueReal)
                    {
                        return new EXEValueReal(this).ApplyOperator(operation, operand);
                    }

                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueBool(this.Value < (operand as EXEValueInt).Value);
                return result;
            }
            else if (">".Equals(operation))
            {
                if (operand is not EXEValueInt)
                {
                    if (operand is EXEValueReal)
                    {
                        return new EXEValueReal(this).ApplyOperator(operation, operand);
                    }

                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueBool(this.Value > (operand as EXEValueInt).Value);
                return result;
            }
            else if ("==".Equals(operation))
            {
                if (operand is not EXEValueInt)
                {
                    if (operand is EXEValueReal)
                    {
                        return new EXEValueReal(this).ApplyOperator(operation, operand);
                    }

                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueBool(this.Value == (operand as EXEValueInt).Value);
                return result;
            }
            else if ("!=".Equals(operation))
            {
                if (operand is not EXEValueInt)
                {
                    if (operand is EXEValueReal)
                    {
                        return new EXEValueReal(this).ApplyOperator(operation, operand);
                    }

                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueBool(this.Value != (operand as EXEValueInt).Value);
                return result;
            }
            else if ("+".Equals(operation))
            {
                if (operand is not EXEValueInt)
                {
                    if (operand is EXEValueReal)
                    {
                        return new EXEValueReal(this).ApplyOperator(operation, operand);
                    }

                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueInt(this.Value + (operand as EXEValueInt).Value);
                return result;
            }
            else if ("-".Equals(operation))
            {
                if (operand is not EXEValueInt)
                {
                    if (operand is EXEValueReal)
                    {
                        return new EXEValueReal(this).ApplyOperator(operation, operand);
                    }

                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueInt(this.Value - (operand as EXEValueInt).Value);
                return result;
            }
            else if ("*".Equals(operation))
            {
                if (operand is not EXEValueInt)
                {
                    if (operand is EXEValueReal)
                    {
                        return new EXEValueReal(this).ApplyOperator(operation, operand);
                    }

                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueInt(this.Value * (operand as EXEValueInt).Value);
                return result;
            }
            else if ("/".Equals(operation))
            {
                if (operand is not EXEValueInt)
                {
                    if (operand is EXEValueReal)
                    {
                        return new EXEValueReal(this).ApplyOperator(operation, operand);
                    }

                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueInt(this.Value / (operand as EXEValueInt).Value);
                return result;
            }
            else if ("%".Equals(operation))
            {
                if (operand is not EXEValueInt)
                {
                    return base.ApplyOperator(operation, operand);
                }

                result = EXEExecutionResult.Success();
                result.ReturnedOutput = new EXEValueInt(this.Value % (operand as EXEValueInt).Value);
                return result;
            }

            return base.ApplyOperator(operation, operand);
        }
        public override string ToObjectDiagramText()
        {
            return this.Value.ToString();
        }

        
    }
}