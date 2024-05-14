using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using OALProgramControl;
using UnityEngine;
using UnityEngine.TestTools;

public class EXECommandToCodeHighlightedAndFormattedTests
{
    [Test]
    public void EXECommandAddingToList_ToCodeHighlightedAndFormattedConversionTest() {
        // Arrange
        EXEASTNodeAccessChain _assignmentTarget = new EXEASTNodeAccessChain();
        _assignmentTarget.AddElement(new EXEASTNodeLeaf("list"));
        EXEASTNodeBase _assignedValue = new EXEASTNodeLeaf("element");

        EXECommand _command = new EXECommandAddingToList(_assignmentTarget, _assignedValue);
        _command.IsActive = true;
        VisitorCommandToString visitor = new VisitorCommandToString();

        // Act
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualOutput = visitor.GetCommandString();

        // Assert
        string _expectedOutput = "<b><color=green>add element to list;\n</color></b>";

        Assert.AreEqual(_expectedOutput, _actualOutput);
    }

    [Test]
    public void EXECommandAssignment_ToCodeHighlightedAndFormattedConversionTest() {
        // Arrange
        EXEASTNodeAccessChain _assignmentTarget = new EXEASTNodeAccessChain();
        _assignmentTarget.AddElement(new EXEASTNodeLeaf("variable"));
        EXEASTNodeBase _assignedValue = new EXEASTNodeLeaf("5");

        EXECommand _command = new EXECommandAssignment(_assignmentTarget, _assignedValue);
        _command.IsActive = true;
        VisitorCommandToString visitor = new VisitorCommandToString();

        // Act
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualOutput = visitor.GetCommandString();

        // Assert
        string _expectedOutput = "<b><color=green>variable = 5;\n</color></b>";

        Assert.AreEqual(_expectedOutput, _actualOutput);
    }

    [Test]
    public void EXECommandBreak_ToCodeHighlightedAndFormattedConversionTest() {
        // Arrange
        EXECommand _command = new EXECommandBreak();
        _command.IsActive = true;
        VisitorCommandToString visitor = new VisitorCommandToString();

        // Act
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualOutput = visitor.GetCommandString();

        // Assert
        string _expectedOutput = "<b><color=green>break;\n</color></b>";

        Assert.AreEqual(_expectedOutput, _actualOutput);
    }

    [Test]
    public void EXECommandCall_ToCodeHighlightedAndFormattedConversionTest() {
        // Arrange
        EXEASTNodeAccessChain _callSource = new EXEASTNodeAccessChain();
        _callSource.AddElement(new EXEASTNodeLeaf("firstVariable"));
        _callSource.AddElement(new EXEASTNodeLeaf("secondVariable"));
        EXEASTNodeMethodCall _callTargett = new EXEASTNodeMethodCall("function");

        EXECommand _command = new EXECommandCall(_callSource, _callTargett);
        _command.IsActive = true;
        VisitorCommandToString visitor = new VisitorCommandToString();

        // Act
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualOutput = visitor.GetCommandString();

        // Assert
        string _expectedOutput = "<b><color=green>firstVariable.secondVariable.function();\n</color></b>";

        Assert.AreEqual(_expectedOutput, _actualOutput);
    }

    [Test]
    public void EXECommandContinue_ToCodeHighlightedAndFormattedConversionTest() {
        // Arrange
        EXECommand _command = new EXECommandContinue();
        _command.IsActive = true;
        VisitorCommandToString visitor = new VisitorCommandToString();

        // Act
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualOutput = visitor.GetCommandString();

        // Assert
        string _expectedOutput = "<b><color=green>continue;\n</color></b>";

        Assert.AreEqual(_expectedOutput, _actualOutput);
    }

    [Test]
    public void EXECommandCreateList_ToCodeHighlightedAndFormattedConversionTest() {
        // Arrange
        EXEASTNodeAccessChain _callSource = new EXEASTNodeAccessChain();
        _callSource.AddElement(new EXEASTNodeLeaf("firstVariable"));
        _callSource.AddElement(new EXEASTNodeLeaf("secondVariable"));
        List<EXEASTNodeBase> _elementList = new List<EXEASTNodeBase>
        {
            new EXEASTNodeLeaf("firstElement"),
            new EXEASTNodeLeaf("secondElement"),
            new EXEASTNodeLeaf("thirdElement")
        };

        EXECommand _command = new EXECommandCreateList("ListType", _callSource, _elementList);
        _command.IsActive = true;
        VisitorCommandToString visitor = new VisitorCommandToString();

        // Act
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualOutput = visitor.GetCommandString();

        // Assert
        string _expectedOutput = "<b><color=green>create list firstVariable.secondVariable of ListType { firstElement, secondElement, thirdElement };\n</color></b>";

        Assert.AreEqual(_expectedOutput, _actualOutput);
    }

    [Test]
    public void EXECommandQueryCreate_ToCodeHighlightedAndFormattedConversionTest() {
        // Arrange
        EXEASTNodeAccessChain _callSource = new EXEASTNodeAccessChain();
        _callSource.AddElement(new EXEASTNodeLeaf("InstanceName"));

        EXECommand _command = new EXECommandQueryCreate("ClassName", _callSource);
        _command.IsActive = true;
        VisitorCommandToString visitor = new VisitorCommandToString();

        // Act
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualOutput = visitor.GetCommandString();

        // Assert
        string _expectedOutput = "<b><color=green>create object instance InstanceName of ClassName;\n</color></b>";

        Assert.AreEqual(_expectedOutput, _actualOutput);
    }

    [Test]
    public void EXECommandQueryDelete_ToCodeHighlightedAndFormattedConversionTest() {
        // Arrange
        EXEASTNodeBase _deletedValue = new EXEASTNodeLeaf("variable");
        EXECommand _command = new EXECommandQueryDelete(_deletedValue);
        _command.IsActive = true;
        VisitorCommandToString visitor = new VisitorCommandToString();

        // Act
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualOutput = visitor.GetCommandString();

        // Assert
        string _expectedOutput = "<b><color=green>delete object instance variable;\n</color></b>";

        Assert.AreEqual(_expectedOutput, _actualOutput);
    }

    [Test]
    public void EXECommandRead_ToCodeHighlightedAndFormattedConversionTest() {
        // Arrange
        EXEASTNodeAccessChain _callSource = new EXEASTNodeAccessChain();
        _callSource.AddElement(new EXEASTNodeLeaf("firstVariable"));
        _callSource.AddElement(new EXEASTNodeLeaf("secondVariable"));
        EXEASTNodeBase _prompt = new EXEASTNodeLeaf("prompt");

        EXECommand _command = new EXECommandRead("assignmentType", _callSource, _prompt);
        _command.IsActive = true;
        VisitorCommandToString visitor = new VisitorCommandToString();

        // Act
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualOutput = visitor.GetCommandString();

        // Assert
        string _expectedOutput = "<b><color=green>firstVariable.secondVariable = assignmentType(read(prompt));\n</color></b>"; // not sure if this is how the result should look, bud originally it was `firstVariable.secondVariable = assignmentTypeprompt))` and this seems better according to grammar, but feel free to change it

        Assert.AreEqual(_expectedOutput, _actualOutput);
    }

    [Test]
    public void EXECommandRemovingFromList_ToCodeHighlightedAndFormattedConversionTest() {
        // Arrange
        EXEASTNodeBase _element = new EXEASTNodeLeaf("element");
        EXEASTNodeBase _list = new EXEASTNodeLeaf("list");

        EXECommand _command = new EXECommandRemovingFromList(_list, _element);
        _command.IsActive = true;
        VisitorCommandToString visitor = new VisitorCommandToString();

        // Act
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualOutput = visitor.GetCommandString();

        // Assert
        string _expectedOutput = "<b><color=green>remove element from list;\n</color></b>";

        Assert.AreEqual(_expectedOutput, _actualOutput);
    }

    [Test]
    public void EXECommandReturn_ToCodeHighlightedAndFormattedConversionTest() {
        // Arrange
        EXEASTNodeBase _returnValue = new EXEASTNodeLeaf("value");

        EXECommand _command = new EXECommandReturn(_returnValue);
        _command.IsActive = true;
        VisitorCommandToString visitor = new VisitorCommandToString();

        // Act
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualOutput = visitor.GetCommandString();

        // Assert
        string _expectedOutput = "<b><color=green>return value;\n</color></b>";

        Assert.AreEqual(_expectedOutput, _actualOutput);
    }

    [Test]
    public void EXECommandWrite_ToCodeHighlightedAndFormattedConversionTest() {
        // Arrange
        List<EXEASTNodeBase> _args = new List<EXEASTNodeBase>() {
            new EXEASTNodeLeaf("arg1"),
            new EXEASTNodeLeaf("arg2")
        };

        EXECommand _command = new EXECommandWrite(_args);
        _command.IsActive = true;
        VisitorCommandToString visitor = new VisitorCommandToString();

        // Act
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualOutput = visitor.GetCommandString();

        // Assert
        string _expectedOutput = "<b><color=green>write(arg1, arg2);\n</color></b>";

        Assert.AreEqual(_expectedOutput, _actualOutput);
    }
}
