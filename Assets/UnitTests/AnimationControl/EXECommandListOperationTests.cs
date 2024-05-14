using System.Linq;
using NUnit.Framework;
using OALProgramControl;
using Assets.Scripts.AnimationControl.OAL;
using System;
using System.Collections.Generic;

namespace Assets.UnitTests.AnimationControl
{
    public class EXEListOperationTests : StandardTest
    {
        [Test]
        public void HappyDay_01_AddToList_ListOfIntegers()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "create list x of integer {1, 2}; add 3 to x;";

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
            array.AppendElement(new EXEValueInt(1), programInstance.ExecutionSpace);
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
        public void HappyDay_01_AddToList_ListOfBooleans()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "create list x of boolean {FALSE, TRUE}; add FALSE to x;";

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
            array.AppendElement(new EXEValueBool(false), programInstance.ExecutionSpace);

            // Assert
            Test.Declare(methodScope, _executionResult);

            Test.Variables
                .ExpectVariable("x", array)
                .ExpectVariable("self", methodScope.OwningObject);

            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_01_AddToList_ListOfChars()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "create list x of char {\"pes\"}; add \"macka\" to x; add \"jasterica\" to x;";

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
            array.AppendElement(new EXEValueString("\"pes\""), programInstance.ExecutionSpace);
            array.AppendElement(new EXEValueString("\"macka\""), programInstance.ExecutionSpace);
            array.AppendElement(new EXEValueString("\"jasterica\""), programInstance.ExecutionSpace);

            // Assert
            Test.Declare(methodScope, _executionResult);

            Test.Variables
                .ExpectVariable("x", array)
                .ExpectVariable("self", methodScope.OwningObject);

            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_01_RemoveFromList_ListOfIntegers()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "create list x of integer {1, 2, 3}; remove 3 from x;";

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
            array.AppendElement(new EXEValueInt(1), programInstance.ExecutionSpace);
            array.AppendElement(new EXEValueInt(2), programInstance.ExecutionSpace);
            array.AppendElement(new EXEValueInt(3), programInstance.ExecutionSpace);
            array.RemoveElement(new EXEValueInt(3), programInstance.ExecutionSpace);

            // Assert
            Test.Declare(methodScope, _executionResult);

            Test.Variables
                .ExpectVariable("x", array)
                .ExpectVariable("self", methodScope.OwningObject);

            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_01_RemoveFromList_ListOfBooleans()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "create list x of boolean {FALSE, TRUE}; remove FALSE from x;";

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
            array.RemoveElement(new EXEValueBool(false), programInstance.ExecutionSpace);

            // Assert
            Test.Declare(methodScope, _executionResult);

            Test.Variables
                .ExpectVariable("x", array)
                .ExpectVariable("self", methodScope.OwningObject);

            Test.PerformAssertion();
        }

    }
}