using NUnit.Framework;
using OALProgramControl;
using Assets.Scripts.AnimationControl.OAL;
using System.Collections.Generic;
using UnityEngine;
using Visualization.UI;
using System;

namespace Assets.UnitTests.AnimationControl
{
    public class EXECommandReadWriteTests : StandardTest
    {
        [Test]
        public void HappyDay_01_Read_01_String()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "placeholder=\"Zadaj string...\";\nx=string(read(placeholder));";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);

            List<String> _mockedInputs = new List<String> { "\"Ahoj\"" };
            MenuManager.Instance.Strategy.MockedInputs = _mockedInputs;

            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);

            // Assert
            Test.Declare(methodScope, _executionResult);
            Test.Variables
                    .ExpectVariable("self", methodScope.OwningObject)
                    .ExpectVariable("placeholder", new EXEValueString("\"Zadaj string...\""))
                    .ExpectVariable("x", new EXEValueString("\"Ahoj\""));
            Test.ConsoleHistory
                    .ExpectText("\"Zadaj string...\"")
                    .ExpectText("\"Ahoj\"");

            Test.PerformAssertion();
        }
        
        [Test]
        public void HappyDay_01_Read_02_Int()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "placeholder=\"Zadaj int...\";\nx=int(read(placeholder));";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);

            List<String> _mockedInputs = new List<String> { "12345" };
            MenuManager.Instance.Strategy.MockedInputs = _mockedInputs;

            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);

            // Assert
            Test.Declare(methodScope, _executionResult);
            Test.Variables
                    .ExpectVariable("self", methodScope.OwningObject)
                    .ExpectVariable("placeholder", new EXEValueString("\"Zadaj int...\""))
                    .ExpectVariable("x", new EXEValueInt("12345"));
            Test.ConsoleHistory
                    .ExpectText("\"Zadaj int...\"")
                    .ExpectText("12345");

            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_01_Read_03_Bool()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "placeholder=\"Zadaj bool...\";\nx=bool(read(placeholder));";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);

            List<String> _mockedInputs = new List<String> { "TRUE" };
            MenuManager.Instance.Strategy.MockedInputs = _mockedInputs;

            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);

            // Assert
            Test.Declare(methodScope, _executionResult);
            Test.Variables
                    .ExpectVariable("self", methodScope.OwningObject)
                    .ExpectVariable("placeholder", new EXEValueString("\"Zadaj bool...\""))
                    .ExpectVariable("x", new EXEValueBool("TRUE"));
            Test.ConsoleHistory
                    .ExpectText("\"Zadaj bool...\"")
                    .ExpectText("TRUE");

            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_01_Read_04_Real()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "placeholder=\"Zadaj real...\";\nx=real(read(placeholder));";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);

            List<String> _mockedInputs = new List<String> { "0.123" };
            MenuManager.Instance.Strategy.MockedInputs = _mockedInputs;

            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);

            // Assert
            Test.Declare(methodScope, _executionResult);
            Test.Variables
                    .ExpectVariable("self", methodScope.OwningObject)
                    .ExpectVariable("placeholder", new EXEValueString("\"Zadaj real...\""))
                    .ExpectVariable("x", new EXEValueReal("0.123"));
            Test.ConsoleHistory
                    .ExpectText("\"Zadaj real...\"")
                    .ExpectText("0.123");

            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_01_Read_05_Multi()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "p1=\"Zadaj string...\";\nx1=string(read(p1));\np2=\"Zadaj int...\";\nx2=int(read(p2));\np3=\"Zadaj real...\";\nx3=real(read(p3));\np4=\"Zadaj bool...\";\nx4=bool(read(p4));";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);

            List<String> _mockedInputs = new List<String> { "\"Hello\"", "-123", "-0.123", "False" };
            MenuManager.Instance.Strategy.MockedInputs = _mockedInputs;

            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);

            // Assert
            Test.Declare(methodScope, _executionResult);
            Test.Variables
                    .ExpectVariable("self", methodScope.OwningObject)
                    .ExpectVariable("p1", new EXEValueString("\"Zadaj string...\""))
                    .ExpectVariable("x1", new EXEValueString("\"Hello\""))
                    .ExpectVariable("p2", new EXEValueString("\"Zadaj int...\""))
                    .ExpectVariable("x2", new EXEValueInt("-123"))
                    .ExpectVariable("p3", new EXEValueString("\"Zadaj real...\""))
                    .ExpectVariable("x3", new EXEValueReal("-0.123"))
                    .ExpectVariable("p4", new EXEValueString("\"Zadaj bool...\""))
                    .ExpectVariable("x4", new EXEValueBool("False"));
            Test.ConsoleHistory
                    .ExpectText("\"Zadaj string...\"")
                    .ExpectText("\"Hello\"")
                    .ExpectText("\"Zadaj int...\"")
                    .ExpectText("-123")
                    .ExpectText("\"Zadaj real...\"")
                    .ExpectText("-0.123")
                    .ExpectText("\"Zadaj bool...\"")
                    .ExpectText("False");

            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_02_Write_01_String() 
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "write(\"Ahoj\");";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);
           
            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);

            // Assert
            Test.Declare(methodScope, _executionResult);
            Test.Variables
                    .ExpectVariable("self", methodScope.OwningObject);
            Test.ConsoleHistory
                    .ExpectText("\"Ahoj\"");

            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_02_Write_02_Int() 
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "write(123456789);";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);
           
            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);

            // Assert
            Test.Declare(methodScope, _executionResult);
            Test.Variables
                    .ExpectVariable("self", methodScope.OwningObject);
            Test.ConsoleHistory
                    .ExpectText("123456789");
                
            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_02_Write_03_Bool() 
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "write(true);";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);
           
            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);

            // Assert
            Test.Declare(methodScope, _executionResult);
            Test.Variables
                    .ExpectVariable("self", methodScope.OwningObject);
            Test.ConsoleHistory
                    .ExpectText("TRUE");
                
            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_02_Write_04_Real() 
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "write(0.123);";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);
           
            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);

            // Assert
            Test.Declare(methodScope, _executionResult);
            Test.Variables
                    .ExpectVariable("self", methodScope.OwningObject);
            Test.ConsoleHistory
                    .ExpectText("0,123");
                
            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_02_Write_05_Multi() 
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "write(\"Čo je wrist?\");\nwrite(\"Zápästie,\nnie fist - päsť.\");\nwrite(-123);\nwrite(0.987);\nwrite(False);";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);
           
            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);

            // Assert
            Test.Declare(methodScope, _executionResult);
            Test.Variables
                    .ExpectVariable("self", methodScope.OwningObject);
            Test.ConsoleHistory
                    .ExpectText("\"Čo je wrist?\"")
                    .ExpectText("\"Zápästie,\nnie fist - päsť.\"")
                    .ExpectText("-123")
                    .ExpectText("0,987")
                    .ExpectText("FALSE");          

            Test.PerformAssertion();
        }

        [Test]
        public void HappyDay_03_ReadWrite_01()
        {
            CommandTest Test = new CommandTest();

            // Arrange
            string _methodSourceCode = "write(\"Ahoj.\");\nwrite(\"Ako sa máš?\");\nx=string(read(\"\"));\ny=int(read(\"1+2=\"));";

            OALProgram programInstance = new OALProgram();
            CDClass owningClass = programInstance.ExecutionSpace.SpawnClass("Class1");

            CDMethod owningMethod = new CDMethod(owningClass, "Method1", "");
            owningClass.AddMethod(owningMethod);
            List<String> _mockedInputs = new List<String> { "\"Dobre.\"", "3" };
            MenuManager.Instance.Strategy.MockedInputs = _mockedInputs;

            // Act
            EXEScopeMethod methodScope = OALParserBridge.Parse(_methodSourceCode);
            owningMethod.ExecutableCode = methodScope;
            programInstance.SuperScope = methodScope;

            EXEExecutionResult _executionResult = PerformExecution(programInstance);

            // Assert
            Test.Declare(methodScope, _executionResult);
            Test.Variables
                    .ExpectVariable("self", methodScope.OwningObject)
                    .ExpectVariable("x", new EXEValueString("\"Dobre.\""))
                    .ExpectVariable("y", new EXEValueInt("3"));
            Test.ConsoleHistory
                    .ExpectText("\"Ahoj.\"")
                    .ExpectText("\"Ako sa máš?\"")
                    .ExpectText("\"\"")
                    .ExpectText("\"Dobre.\"")
                    .ExpectText("\"1+2=\"")
                    .ExpectText("3");

            Test.PerformAssertion();
        }

    }
}