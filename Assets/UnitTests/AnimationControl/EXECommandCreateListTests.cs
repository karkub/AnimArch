using System.Linq;
using NUnit.Framework;
using OALProgramControl;
using Assets.Scripts.AnimationControl.OAL;
using System;
using System.Collections.Generic;

namespace Assets.UnitTests.AnimationControl
{
    public class EXECommandCreateListTests : StandardTest
    {
        //TODO create tests for different types of lists (int, integer, long, long int, long integer)

        // Test int, double, bool, string 
        [Test]
        public void HappyDay_01_AssignToLocalVariable_01_Literal_01_ListOfIntegers()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "create list x of integer {5, 2, 3};";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);

            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);
            EXEValueArray array = new EXEValueArray("integer[]");
            array.InitializeEmptyArray();
            array.AppendElement(new EXEValueInt(5), programInstance.ExecutionSpace);
            array.AppendElement(new EXEValueInt(2), programInstance.ExecutionSpace);
            array.AppendElement(new EXEValueInt(3), programInstance.ExecutionSpace);

            // Assert
            Test.Declare(methodScope, _executionResult);

            Test.Variables
                .ExpectVariable("x", array)
                .ExpectVariable("self", methodScope.OwningObject);

            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_01_AssignToLocalVariable_01_Literal_01_ListOfReals()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "create list x of real {5.5, 2.2, 3.9};";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);

            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);
            EXEValueArray array = new EXEValueArray("real[]");
            array.InitializeEmptyArray();
            array.AppendElement(new EXEValueReal("5.5"), programInstance.ExecutionSpace);
            array.AppendElement(new EXEValueReal("2.2"), programInstance.ExecutionSpace);
            array.AppendElement(new EXEValueReal("3.9"), programInstance.ExecutionSpace);

            // Assert
            Test.Declare(methodScope, _executionResult);

            Test.Variables
                .ExpectVariable("x", array)
                .ExpectVariable("self", methodScope.OwningObject);
            // test console history
            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_01_AssignToLocalVariable_01_Literal_01_ListOfStrings()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "create list x of string {\"ananas\", \"b\"};";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);

            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);
            EXEValueArray array = new EXEValueArray("string[]");
            array.InitializeEmptyArray();
            array.AppendElement(new EXEValueString("\"ananas\""), programInstance.ExecutionSpace);
            array.AppendElement(new EXEValueString("\"b\""), programInstance.ExecutionSpace);

            // Assert
            Test.Declare(methodScope, _executionResult);

            Test.Variables
                .ExpectVariable("x", array)
                .ExpectVariable("self", methodScope.OwningObject);

            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_01_AssignToLocalVariable_01_Literal_01_ListOfBooleans()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "create list x of boolean {FALSE, TRUE};";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);

            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);
            EXEValueArray array = new EXEValueArray("boolean[]");
            array.InitializeEmptyArray();
            array.AppendElement(new EXEValueBool(false), programInstance.ExecutionSpace);
            array.AppendElement(new EXEValueBool(true), programInstance.ExecutionSpace);

            // Assert
            Test.Declare(methodScope, _executionResult);

            Test.Variables
                .ExpectVariable("x", array)
                .ExpectVariable("self", methodScope.OwningObject);

            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_01_AssignToLocalVariable_01_Literal_01_ListOfInts()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "create list x of int {5, 2, 3};";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);

            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);
            EXEValueArray array = new EXEValueArray("integer[]");
            array.InitializeEmptyArray();
            array.AppendElement(new EXEValueInt(5), programInstance.ExecutionSpace);
            array.AppendElement(new EXEValueInt(2), programInstance.ExecutionSpace);
            array.AppendElement(new EXEValueInt(3), programInstance.ExecutionSpace);

            // Assert
            Test.Declare(methodScope, _executionResult);

            Test.Variables
                .ExpectVariable("x", array)
                .ExpectVariable("self", methodScope.OwningObject);

            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_01_AssignToLocalVariable_01_Literal_01_ListOfFloats()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "create list x of float {5.5, 2.2, 3.9};";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);

            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);
            EXEValueArray array = new EXEValueArray("real[]");
            array.InitializeEmptyArray();
            array.AppendElement(new EXEValueReal("5.5"), programInstance.ExecutionSpace);
            array.AppendElement(new EXEValueReal("2.2"), programInstance.ExecutionSpace);
            array.AppendElement(new EXEValueReal("3.9"), programInstance.ExecutionSpace);

            // Assert
            Test.Declare(methodScope, _executionResult);

            Test.Variables
                .ExpectVariable("x", array)
                .ExpectVariable("self", methodScope.OwningObject);

            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_01_AssignToLocalVariable_01_Literal_01_ListOfChars()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "create list x of char {\"ananas\", \"b\"};";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);

            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);
            EXEValueArray array = new EXEValueArray("string[]");
            array.InitializeEmptyArray();
            array.AppendElement(new EXEValueString("\"ananas\""), programInstance.ExecutionSpace);
            array.AppendElement(new EXEValueString("\"b\""), programInstance.ExecutionSpace);

            // Assert
            Test.Declare(methodScope, _executionResult);

            Test.Variables
                .ExpectVariable("x", array)
                .ExpectVariable("self", methodScope.OwningObject);

            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_01_AssignToLocalVariable_01_Literal_01_ListOfBools()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "create list x of bool {FALSE, TRUE};";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);

            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);
            EXEValueArray array = new EXEValueArray("bool[]");
            array.InitializeEmptyArray();
            array.AppendElement(new EXEValueBool(false), programInstance.ExecutionSpace);
            array.AppendElement(new EXEValueBool(true), programInstance.ExecutionSpace);

            // Assert
            Test.Declare(methodScope, _executionResult);

            Test.Variables
                .ExpectVariable("x", array)
                .ExpectVariable("self", methodScope.OwningObject);

            Test.PerformAssertion();
        }
    }
}