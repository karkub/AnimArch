using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OALProgramControl
{
    public class EXETypes
    {
        public static String IntegerTypeName = "integer";
        public const String RealTypeName = "real";
        public const String BooleanTypeName = "boolean";
        public const String StringTypeName = "string";
        public const String DateTypeName = "date";
        public const String WildCardTypeName = "___any___";
        public const String UniqueIDTypeName = "unique_ID";
        public const String SelfReferenceName = "self";
        //public const String ReferenceTypeName = "reference";

        public const String BooleanTrue = "TRUE";
        public const String BooleanFalse = "FALSE";

        public const String UnitializedName = "UNDEFINED";
        public const String UniqueIDAttributeName = "ID";

        private static readonly List<string> UnaryOperators = new List<string>() { "cardinality", "empty", "not_empty", "not", "NOT", "type_name"};
        private static readonly List<string> BinaryOperators = new List<string>() { "*", "/", "%", "+", "-", "<", ">", "<=", ">=", "==", "!=", "and", "AND", "or", "OR" };

        private static readonly List<String> IntNames = new List<string>(new String[] { "int", "integer", "long", "long int", "long integer" });
        private static readonly List<String> RealNames = new List<string>(new String[] { "real", "float", "double", "decimal", "floating", "floating point" });
        private static readonly List<String> BoolNames = new List<string>(new String[] { "bool", "boolean" });
        private static readonly List<String> StringNames = new List<string>(new String[] { "string", "char" });
        private static readonly List<String> PrimitiveNames = new List<string>(new String[] { IntegerTypeName, RealTypeName, BooleanTypeName, StringTypeName, DateTypeName, UniqueIDTypeName });
        private static readonly Dictionary<Char, String> EscapeChars = new Dictionary<Char, String>() { { '\"', "\"" }, { '\'', "\'" }, { 't', "\t" }, { 'n', "\n" }, { '\\', "\\" } };

        private const string NameRegex = @"[a-zA-Z_]+[a-zA-Z0-9_]";

        public static bool IsPrimitive(String typeName)
        {
            return PrimitiveNames.Contains(typeName.ToLower());
        }
        public static bool IsValidIntName(string typeName)
        {
            return IntNames.Contains(typeName.ToLower());
        }
        public static bool IsValidRealName(string typeName)
        {
            return RealNames.Contains(typeName.ToLower());
        }
        public static bool IsValidBoolName(string typeName)
        {
            return BoolNames.Contains(typeName.ToLower());
        }
        public static bool IsValidStringName(string typeName)
        {
            return StringNames.Contains(typeName.ToLower());
        }
        public static EPrimitiveType DeterminePrimitiveType(string value)
        {
            if (IsValidIntValue(value))
            {
                return EPrimitiveType.Integer;
            }

            if (IsValidRealValue(value))
            {
                return EPrimitiveType.Real;
            }

            if (IsValidBoolValue(value))
            {
                return EPrimitiveType.Bool;
            }

            if (IsValidStringValue(value))
            {
                return EPrimitiveType.String;
            }

            return EPrimitiveType.NotPrimitive;
        }
        public static EXEValuePrimitive DeterminePrimitiveValue(string value)
        {
            EPrimitiveType primitiveType = DeterminePrimitiveType(value);

            switch (primitiveType)
            {
                case EPrimitiveType.Integer:
                    return new EXEValueInt(value);
                case EPrimitiveType.Real:
                    return new EXEValueReal(value);
                case EPrimitiveType.String:
                    return new EXEValueString(value);
                case EPrimitiveType.Bool:
                    return new EXEValueBool(value);
                case EPrimitiveType.NotPrimitive:
                    return null;
                default:
                    throw new Exception(string.Format("Invalid value of enum EPrimitiveType type: \"{0}\".", primitiveType));
            }
        }
        public static bool DetermineBoolValue(string value)
        {
            if (IsBoolTrue(value))
            {
                return true;
            }
            else if (IsBoolFalse(value))
            {
                return false;
            }
            else
            {
                throw new ArgumentException(string.Format("\"{0}\" is not a valid bool value.", value));
            }
        }
        public static bool IsValidBoolValue(string value)
        {
            return IsBoolTrue(value) || IsBoolFalse(value);
        }
        public static bool IsBoolTrue(string value)
        {
            return BooleanTrue.Equals(value.ToUpper());
        }
        public static bool IsBoolFalse(string value)
        {
            return BooleanFalse.Equals(value.ToUpper());
        }
        public static bool IsValidIntValue(string value)
        {
            return Regex.IsMatch(value, @"^(-)?((0)|([1-9]+[0-9]*))$");
        }
        public static bool IsValidRealValue(string value)
        {
            return Regex.IsMatch(value, @"^(-)?(((0)|([1-9]+[0-9]*))[\.\,][0-9]+)$");
        }
        public static bool IsValidStringValue(string value)
        {
            return value.Length > 1 && value[0] == '"' && value[value.Length - 1] == '"';
        }
        public static bool IsValidArrayType(string typeName)
        {
            return !string.IsNullOrEmpty(typeName) && typeName.Length > 2 && "[]".Equals(typeName.Substring(typeName.Length - 2, 2));
        }
        public static EXEValueBase DefaultValue(string typeName, CDClassPool classPool)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentException("Type name cannot be null nor an empty string.");
            }

            EXEValueBase result = null;

            if (IntNames.Contains(typeName))
            {
                result = new EXEValueInt("0");
            }
            else if (RealNames.Contains(typeName))
            {
                result = new EXEValueReal("0.0");
            }
            else if (StringNames.Contains(typeName))
            {
                result = new EXEValueString("\"\"");
            }
            else if (BoolNames.Contains(typeName))
            {
                result = new EXEValueBool(BooleanFalse);
            }
            else if (classPool.ClassExists(typeName))
            {
                result = new EXEValueReference(classPool.getClassByName(typeName));
            }
            else if (typeName.Length > 2 && "[]".Equals(typeName.Substring(typeName.Length - 2, 2)))
            {
                result = new EXEValueArray(typeName);
                ((EXEValueArray)result).InitializeEmptyArray();
            }

            if (result != null)
            {
                return result;
            }

            throw new Exception(string.Format("Cannot create default value of unknown type \"{0}\".", typeName ?? "NULL"));
        }
        public static bool CanBeAssignedTo(string sourceType, string targetType, CDClassPool classPool)
        {
            if (WildCardTypeName.Equals(targetType))
            {
                return true;
            }

            return DefaultValue(targetType, classPool).AssignValueFrom(DefaultValue(sourceType, classPool)).IsSuccess;
        }
        public static bool CanBeAssignedTo(EXEValueBase sourceValue, string targetType, CDClassPool classPool)
        {
            if (WildCardTypeName.Equals(targetType))
            {
                return true;
            }

            return DefaultValue(targetType, classPool).AssignValueFrom(sourceValue).IsSuccess;
        }
        public static bool CanBeAssignedTo(string sourceType, EXEValueBase targetValue, CDClassPool classPool)
        {
            return targetValue.DeepClone().AssignValueFrom(DefaultValue(sourceType, classPool)).IsSuccess;
        }
        public static bool IsUnaryOperator(string _operator)
        {
            return UnaryOperators.Contains(_operator);
        }
        public static bool IsBinaryOperator(string _operator)
        {
            return BinaryOperators.Contains(_operator);
        }
        public static String ConvertEATypeName(String EAType)
        {
            String Input = EAType.ToLower();
            if (IntNames.Contains(Input))
            {
                return IntegerTypeName;
            }
            if (RealNames.Contains(Input))
            {
                return RealTypeName;
            }
            if (BoolNames.Contains(Input))
            {
                return BooleanTypeName;
            }
            if (StringNames.Contains(Input))
            {
                return StringTypeName;
            }

            return EAType;
        }
        public static bool IsValidClassName(string className)
        {
            return IsValidName(className);
        }
        public static bool IsValidMethodName(string className)
        {
            return IsValidName(className);
        }
        public static bool IsValidVariableName(string className)
        {
            return IsValidName(className);
        }
        // Attribute/Class/Variable/Method
        private static bool IsValidName(string name)
        {
            return Regex.IsMatch(name, NameRegex);
        }
    }
}
