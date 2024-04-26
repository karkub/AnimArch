using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace OALProgramControl
{
    public static class ErrorMessage
    {

        public static string ClassNotFound(string className, CDClassPool classPool)
        {
            return string
                    .Format
                    (
                        "Class '{0}' not found. Known classes are:\n{1}.",
                        Stringify(className),
                        ClassNameList(classPool)
                    );
        }
        public static string AttributeNotFoundOnClass(string attributeName, CDClass owningClass)
        {
            return string
                    .Format
                    (
                        "Attribute '{0}' not found on class '{1}'. Known attributes are:\n{2}.",
                        Stringify(attributeName),
                        Stringify(owningClass.Name),
                        AttributeNameList(owningClass)
                    );
        }
        public static string MethodNotFoundOnClass(string methodName, string owningClassName)
        {
            return string
                    .Format
                    (
                        "Method '{0}' not found on class '{1}'.",
                        Stringify(methodName),
                        Stringify(owningClassName)
                    );
        }
        public static string AttributeNotFoundOnClassInstance(string attributeName, CDClassInstance classInstance)
        {
            return string
                    .Format
                    (
                        "Attribute '{0}' not found on instance with ID '{1}'. Known attributes are:\n{2}.",
                        Stringify(attributeName),
                        classInstance.UniqueID,
                        AttributeNameList(classInstance)
                    );
        }
        public static string VariableNotFound(string variableName, EXEScopeBase currentScope)
        {
            return string
                    .Format
                    (
                        "Variable '{0}' not found. Variables available in current scope are:\n{1}.",
                        Stringify(variableName),
                        VariableNameList(currentScope)
                    );
        }
        public static string InstanceNotFound(long id, CDClass owningClass)
        {
            return string
                    .Format
                    (
                        "Class instance with ID '{0}' not found on class '{1}'. Available instance IDs are '{2}'.",
                        id,
                        Stringify(owningClass.Name),
                        InstanceIdList(owningClass)
                    );
        }
        public static string InstanceNotFoundRecursive(long id, CDClass owningClass)
        {
            return string
                    .Format
                    (
                        "Class instance with ID '{0}' not found on class '{1}' or its subclasses. Available instance IDs are '{2}'.",
                        id,
                        Stringify(owningClass.Name),
                        InstanceIdListRecursive(owningClass)
                    );
        }
        public static string InvalidParameterCount(string className, string methodName, int expectedParamCount, int actualArgumentCount)
        {
            return string
                    .Format
                    (
                        "Invalid count of arguments of of method '{0}' of class '{1}'. Expected {2}, provided {3}.",
                        Stringify(methodName),
                        Stringify(className),
                        expectedParamCount,
                        actualArgumentCount
                    );
        }
        public static string InvalidParameterValue(string className, string methodName, string parameterName, string parameterType, string value)
        {
            return string
                    .Format
                    (
                        "Invalid value of parameter '{0}' of method '{1}' of class '{2}'. Value '{3}' is not of '{4}' type.",
                        Stringify(parameterName),
                        Stringify(methodName),
                        Stringify(className),
                        Stringify(value),
                        Stringify(parameterType)
                    );
        }
        public static string UnresolvedParameterValue(string className, string methodName, string parameterName, string value)
        {
            return string
                    .Format
                    (
                        "Could not resolve value '{0}' parameter '{1}' of method '{2}' of class '{3}'.",
                        Stringify(value),
                        Stringify(parameterName),
                        Stringify(methodName),
                        Stringify(className)
                    );
        }
        public static string AddingToNotList(string variableName, string variableType)
        {
            return string
                    .Format
                    (
                        "Cannot add element to '{0}'. The variable is not a list, its type is '{1}'.",
                        Stringify(variableName),
                        Stringify(variableType)
                    );
        }
        public static string RemovingFromNotList(string variableName, string variableType)
        {
            return string
                    .Format
                    (
                        "Cannot remove an element from '{0}'. The variable is not a list, its type is '{1}'.",
                        Stringify(variableName),
                        Stringify(variableType)
                    );
        }
        public static string AddingToInvalidTypeList(string addedElementType, string listType)
        {
            return string
                    .Format
                    (
                        "Cannot add element of type '{0}' to list of '{1}'.",
                        Stringify(addedElementType),
                        Stringify(listType)
                    );
        }
        public static string RemovingFromInvalidTypeList(string addedElementType, string listType)
        {
            return string
                    .Format
                    (
                        "Cannot remove element of type '{0}' from list of '{1}'.",
                        Stringify(addedElementType),
                        Stringify(listType)
                    );
        }
        public static string AssignNewListToVariableHoldingListOfAnotherType(string variableName, string currentListType, string newListType)
        {
            return string
                    .Format
                    (
                        "Tried to assign new list of '{0}' to existing variable '{1}' of type list of '{2}'.",
                        Stringify(newListType),
                        Stringify(variableName),
                        Stringify(currentListType)
                    );
        }
        public static string AddInvalidValueToList(string listVariableName, string listType, string itemReference, string itemType)
        {
            return string
                    .Format
                    (
                        "Tried to add item '{0}' of type '{1}' into list '{2}' of '{3}' type.",
                        Stringify(itemReference),
                        Stringify(itemType),
                        Stringify(listVariableName),
                        Stringify(listType)
                    );
        }
        public static string IsNotReference(string variableName, string variableType)
        {
            return string
                    .Format
                    (
                        "Variable '{0}' is not a valid class reference type. Instead, it is '{1}'.",
                        Stringify(variableName),
                        Stringify(variableType)
                    );
        }
        public static string FailedExpressionEvaluation(EXEASTNodeBase expression, EXEScope currentScope)
        {
            VisitorCommandToString visitor = VisitorCommandToString.BorrowAVisitor();
            expression.Accept(visitor);
            return string
                    .Format
                    (
                        "Failed to evaluate expression {0}. Variables available in current scope are:\n{1}.",
                        Stringify(visitor.GetCommandStringAndResetStateNow()),
                        VariableNameList(currentScope)
                    );
        }
        public static string FailedExpressionTypeDetermination(EXEASTNodeBase expression)
        {
            VisitorCommandToString visitor = VisitorCommandToString.BorrowAVisitor();
            expression.Accept(visitor);
            return FailedExpressionTypeDetermination(visitor.GetCommandStringAndResetStateNow());
        }
        public static string FailedExpressionTypeDetermination(string expression)
        {
            return string
                    .Format
                    (
                        "Failed to determine type of expression {0}.",
                        Stringify(expression)
                    );
        }
        public static string CreatingUndefinedVariable(string variableName)
        {
            return string
                    .Format
                    (
                        "Cannot create variable '{0}' of type UNDEFINED. This is very wrong.",
                        Stringify(variableName)
                    );
        }
        public static string CreatingExistingVariable(string variableName)
        {
            return string
                    .Format
                    (
                        "Cannot create variable '{0}', as it already exists.",
                        Stringify(variableName)
                    );
        }
        public static string ExistingUndefinedVariable(string variableName)
        {
            return string
                    .Format
                    (
                        "Existing primitive variable '{0}' is of type UNDEFINED. This is very wrong.",
                        Stringify(variableName)
                    );
        }
        public static string InvalidAssignment(string assignedValue, string assignedType, string variableName, string variableType)
        {
            return string
                    .Format
                    (
                        "Cannot assign expression '{0}' of type '{1}' to variable '{2}' of type '{3}'.",
                        Stringify(assignedValue),
                        Stringify(assignedType),
                        Stringify(variableName),
                        Stringify(variableType)
                    );
        }
        public static string InvalidObjectCreation(string assignedType, string variableName, string variableType)
        {
            return string
                    .Format
                    (
                        "Cannot assign newly created object of type '{0}' to variable '{1}' of type '{2}'.",
                        Stringify(assignedType),
                        Stringify(variableName),
                        Stringify(variableType)
                    );
        }
        public static string FailedObjectCreation(string className)
        {
            return string
                    .Format
                    (
                        "Failed to instantiate an isntance of '{0}'.",
                        Stringify(className)
                    );
        }
        public static string FailedObjectDestruction(string variable)
        {
            return string
                    .Format
                    (
                        "Failed to destroy instance in '{0}'.",
                        Stringify(variable)
                    );
        }
        public static string RelationNotFound(string class1Name, string class2Name)
        {
            return string
                    .Format
                    (
                        "Relation between classes '{0}' and '{1}' not found.",
                        Stringify(class1Name),
                        Stringify(class2Name)
                    );
        }
        public static string RelationNotFound(long id1, long id2, CDRelationship relationship)
        {
            return string
                    .Format
                    (
                        "Relation between instances '{0}' and '{1}' not found. Current instance relationships of relationship '{2}' are: {3}",
                        id1,
                        id2,
                        Stringify(relationship.RelationshipName),
                        RelationshipInstanceList(relationship)
                    );
        }
        public static string FailedRelationCreation(string variable1, string variable2)
        {
            return string
                    .Format
                    (
                        "Failed to create relation between instances '{0}' and '{1}'.",
                        Stringify(variable1),
                        Stringify(variable2)
                    );
        }
        public static string InvalidReference(string variableName, string value)
        {
            return string
                    .Format
                    (
                        "Invalid reference in variable '{0}'. '{1}' is not a valid reference value.",
                        Stringify(variableName),
                        Stringify(value)
                    );
        }
        public static string InvalidValueForType(string value, string expectedType)
        {
            return string
                    .Format
                    (
                        "'{0}' is not a valid value of type '{1}'.",
                        Stringify(value),
                        Stringify(expectedType)
                    );
        }
        public static string AssignmentToReadonlyProperty(string propertyName)
        {
            return string
                    .Format
                    (
                        "Cannot to assign to readonly property '{0}'",
                        Stringify(propertyName)
                    );
        }
        public static string IsNotArray(string variable)
        {
            return string
                    .Format
                    (
                        "'{0}' is not an array when it is supposed to be.",
                        Stringify(variable)
                    );
        }
        public static string SelectingAnyIntoAnArray()
        {
            return "Tried to perform select of type ANY, and assign it into variable of array type. ANY select can only assign its result into a single reference variable.";
        }
        public static string SelectingManyIntoAReference()
        {
            return "Tried to perform select of type MANY, and assign it into variable of single reference type. MANY select can only assign its result into an array variable.";
        }
        public static string FailedRetrievingAllClassInstanceIds(CDClass searchedClass)
        {
            return string.Format("Failed to retrieve all instance ids of class {0}.", Stringify(searchedClass.Name));
        }
        public static string UnknownSelectCardinality(string cardinality)
        {
            return string
                    .Format
                    (
                        "Unknown select cardinality: '{0}'",
                        Stringify(cardinality)
                    );
        }
        public static string NoRelationshipSelectionOnSelectByRelationship()
        {
            return "No relationship selection defined on select by relationship command. Perhaps it's parser fault?";
        }
        public static string InvalidInstanceId(string variableName, string variableValue)
        {
            return string
                    .Format
                    (
                        "{0} holds value '{1}', which is not valid class instance id.",
                        Stringify(variableName),
                        Stringify(variableValue)
                    );
        }
        public static string PrintValueMustBePrimitive()
        {
            return "Can only print primitive values.";
        }
        public static string ConditionInIfStatementNotSet()
        {
            return "The condition in IF statement is not set. Perhaps a parser issue?";
        }
        public static string IsNotIterable(string type)
        {
            return string
                    .Format
                    (
                        "Type '{0}' is not iterable.",
                        Stringify(type)
                    );
        }
        public static string IterableAndIteratorTypeMismatch(string iterableName, string iterableType, string iteratorName, string iteratorType)
        {
            return string
                    .Format
                    (
                        "Iterable '{0}' of type '{1}' cannot be iterated by iterator '{2}' of type '{3}'.",
                        Stringify(iterableName),
                        Stringify(iterableType),
                        Stringify(iteratorName),
                        Stringify(iteratorType)
                    );
        }
        public static string IterationLoopThresholdCrossed(int iterationThreshold)
        {
            return string.Format("Loop iteration thresholded reached. Maximum of {0} iterations are allowed per loop.", iterationThreshold);
        }

        private static string Stringify(string s)
        {
            return s ?? "NULL";
        }
        private static string VariableNameList(EXEScopeBase currentScope)
        {
            return string.Join
                (
                    ",\n",
                    currentScope
                        .ScopesToTop()
                        .SelectMany
                        (
                            scope => scope
                                        .AllDeclaredVariables()
                                        .Select(pair => pair.Value + " " + pair.Key)
                        )
                );
        }
        private static string ClassNameList(CDClassPool classPool)
        {
            return string.Join(",\n", classPool.Classes.Select(_class => _class.Name));
        }
        private static string AttributeNameList(CDClass owningClass)
        {
            return string.Join(",\n", owningClass.GetAttributes(true).Select(attribute => attribute.Type + " " + attribute.Name));
        }
        private static string AttributeNameList(CDClassInstance owningClassInstance)
        {
            return string.Join(",\n", owningClassInstance.State.Select(vp => vp.Key));
        }
        private static string InstanceIdList(CDClass owningClass)
        {
            return string.Join(",\n", owningClass.Instances.Select(instance => instance.UniqueID.ToString()));
        }
        private static string InstanceIdListRecursive(CDClass owningClass)
        {
            return string.Join(",\n", new List<string> { InstanceIdList(owningClass) }.Concat(owningClass.SubClasses.Select(subClass => InstanceIdListRecursive(subClass))));
        }
        private static string RelationshipInstanceList(CDRelationship relationship)
        {
            return string.Join(",\n", relationship.InstanceRelationships.Select(couple => "(" + couple.Item1 + ", " + couple.Item2 + ")"));
        }
    }
}