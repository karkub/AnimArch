using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.AnimationControl.OAL;
using NUnit.Framework;
using OALProgramControl;
using UnityEngine;
using UnityEngine.TestTools;

public class EXECommandToCodeTests
{
    [Test]
    public void EXECommandAddingToList_ToCodeConversionTest() {
        // Arrange
        EXEASTNodeAccessChain _assignmentTarget = new EXEASTNodeAccessChain();
        _assignmentTarget.AddElement(new EXEASTNodeLeaf("list"));
        EXEASTNodeBase _assignedValue = new EXEASTNodeLeaf("element");

        EXECommand _command = new EXECommandAddingToList(_assignmentTarget, _assignedValue) { IsActive = true };

        // Act
        VisitorCommandToString visitor = new VisitorCommandToString();
        visitor.DeactivateSimpleFormatting();
        _command.Accept(visitor);
        string _actualUnformattedOutput = visitor.GetCommandString();

        visitor = new VisitorCommandToString();
        _command.Accept(visitor);
        string _actualFormattedOutput = visitor.GetCommandString();

        visitor = new VisitorCommandToString();
        visitor.DeactivateSimpleFormatting();
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualHighlightedOutput = visitor.GetCommandString();

        visitor = new VisitorCommandToString();
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualHighlightedAndFormattedOutput = visitor.GetCommandString();
    
        // Assert
        string _expectedUnformattedOutput             = "add element to list";
        string _expectedFormattedOutput               = "add element to list;\n";
        string _expectedHighlightedOutput             = "<b><color=green>add element to list</color></b>";
        string _expectedHighlightedAndFormattedOutput = "<b><color=green>add element to list;\n</color></b>";
    
        Assert.AreEqual(_expectedUnformattedOutput, _actualUnformattedOutput);
        Assert.AreEqual(_expectedFormattedOutput, _actualFormattedOutput);
        Assert.AreEqual(_expectedHighlightedOutput, _actualHighlightedOutput);
        Assert.AreEqual(_expectedHighlightedAndFormattedOutput, _actualHighlightedAndFormattedOutput);
    }

    [Test]
    public void EXECommandAssignment_ToCodeConversionTest() {
        // Arrange
        EXEASTNodeAccessChain _assignmentTarget = new EXEASTNodeAccessChain();
        _assignmentTarget.AddElement(new EXEASTNodeLeaf("x"));
        EXEASTNodeBase _assignedValue = new EXEASTNodeLeaf("5");

        EXECommand _command = new EXECommandAssignment(_assignmentTarget, _assignedValue) { IsActive = true };

        // Act
        VisitorCommandToString visitor = new VisitorCommandToString();
        visitor.DeactivateSimpleFormatting();
        _command.Accept(visitor);
        string _actualUnformattedOutput = visitor.GetCommandString();

        visitor = new VisitorCommandToString();
        _command.Accept(visitor);
        string _actualFormattedOutput = visitor.GetCommandString();

        visitor = new VisitorCommandToString();
        visitor.DeactivateSimpleFormatting();
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualHighlightedOutput = visitor.GetCommandString();

        visitor = new VisitorCommandToString();
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualHighlightedAndFormattedOutput = visitor.GetCommandString();
    
        // Assert
        string _expectedUnformattedOutput             = "x = 5";
        string _expectedFormattedOutput               = "x = 5;\n";
        string _expectedHighlightedOutput             = "<b><color=green>x = 5</color></b>";
        string _expectedHighlightedAndFormattedOutput = "<b><color=green>x = 5;\n</color></b>";
    
        Assert.AreEqual(_expectedUnformattedOutput, _actualUnformattedOutput);
        Assert.AreEqual(_expectedFormattedOutput, _actualFormattedOutput);
        Assert.AreEqual(_expectedHighlightedOutput, _actualHighlightedOutput);
        Assert.AreEqual(_expectedHighlightedAndFormattedOutput, _actualHighlightedAndFormattedOutput);
    }

    [Test]
    public void EXECommandBreak_ToCodeConversionTest() {
        // Arrange
        EXECommand _command = new EXECommandBreak() { IsActive = true };

        // Act
        VisitorCommandToString visitor = new VisitorCommandToString();
        visitor.DeactivateSimpleFormatting();
        _command.Accept(visitor);
        string _actualUnformattedOutput = visitor.GetCommandString();

        visitor = new VisitorCommandToString();
        _command.Accept(visitor);
        string _actualFormattedOutput = visitor.GetCommandString();

        visitor = new VisitorCommandToString();
        visitor.DeactivateSimpleFormatting();
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualHighlightedOutput = visitor.GetCommandString();

        visitor = new VisitorCommandToString();
        visitor.ActivateHighlighting();
        _command.Accept(visitor);
        string _actualHighlightedAndFormattedOutput = visitor.GetCommandString();
    
        // Assert
        string _expectedUnformattedOutput             = "break";
        string _expectedFormattedOutput               = "break;\n";
        string _expectedHighlightedOutput             = "<b><color=green>break</color></b>";
        string _expectedHighlightedAndFormattedOutput = "<b><color=green>break;\n</color></b>";
    
        Assert.AreEqual(_expectedUnformattedOutput, _actualUnformattedOutput);
        Assert.AreEqual(_expectedFormattedOutput, _actualFormattedOutput);
        Assert.AreEqual(_expectedHighlightedOutput, _actualHighlightedOutput);
        Assert.AreEqual(_expectedHighlightedAndFormattedOutput, _actualHighlightedAndFormattedOutput);
    }
}
