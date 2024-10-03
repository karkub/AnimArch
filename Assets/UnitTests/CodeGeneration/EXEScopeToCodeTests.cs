using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.AnimationControl.OAL;
using NUnit.Framework;
using OALProgramControl;
using UnityEngine;
using UnityEngine.TestTools;

public class EXEScopeToCodeTests
{
    [Test]
    public void EXEScopeForEach_ToCodeConversionTest()
    {//.Perform()element
        // Arrange
        string _methodSourceCode = "for each element in list\r\n\telement.Perform();\r\nend for;";
        EXEScopeMethod _scope = OALParserBridge.Parse(_methodSourceCode);
        _scope.IsActive = true;
        foreach (EXECommand _command in _scope.Commands) {
            _command.IsActive = true;
        }

        // Act
        VisitorCommandToString visitor = new VisitorCommandToString();
        visitor.DeactivateSimpleFormatting();
        _scope.Accept(visitor);
        string _actualUnformattedOutput = visitor.GetCommandString();

        visitor = new VisitorCommandToString();
        _scope.Accept(visitor);
        string _actualFormattedOutput = visitor.GetCommandString();

        visitor = new VisitorCommandToString();
        visitor.DeactivateSimpleFormatting();
        visitor.ActivateHighlighting();
        _scope.Accept(visitor);
        string _actualHighlightedOutput = visitor.GetCommandString();

        visitor = new VisitorCommandToString();
        visitor.ActivateHighlighting();
        _scope.Accept(visitor);
        string _actualHighlightedAndFormattedOutput = visitor.GetCommandString();

        // Assert
        string _expectedUnformattedOutput             = "for each element in list\nelement.Perform()end for";
        string _expectedFormattedOutput               = "for each element in list\n\telement.Perform();\nend for;\n";
        string _expectedHighlightedOutput             = "<b><color=green>for each element in list\n</color></b>element.Perform()<b><color=green>end for</color></b>";
        string _expectedHighlightedAndFormattedOutput = "<b><color=green>for each element in list\n</color></b>\telement.Perform();\n<b><color=green>end for;\n</color></b>";
    
        Assert.AreEqual(_expectedUnformattedOutput, _actualUnformattedOutput);
        Assert.AreEqual(_expectedFormattedOutput, _actualFormattedOutput);
        Assert.AreEqual(_expectedHighlightedOutput, _actualHighlightedOutput);
        Assert.AreEqual(_expectedHighlightedAndFormattedOutput, _actualHighlightedAndFormattedOutput);
    }

    [Test]
    public void EXEScopeEXEASTNodeIndexation_OAl_ToCodeConversionTest()
    {
        // Arrange
        string _methodSourceCode = 
        @"create list Numbers of PrettyIntegers { 1, 2, 3 };
        x = Numbers[1];
        ";
        EXEScopeMethod _scope = OALParserBridge.Parse(_methodSourceCode);

        VisitorCommandToString visitor = new VisitorCommandToString();

        // Act
        _scope.Accept(visitor);
        string _actualOutput = visitor.GetCommandString();

        // Assert
        string _expectedOutput = "create list Numbers of PrettyIntegers { 1, 2, 3 };\nx = Numbers[1];\n";

        Assert.AreEqual(_expectedOutput, _actualOutput);
    }

    [Test]
    public void EXEScopeEXEASTNodeIndexation_Python_ToCodeConversionTest()
    {
        // Arrange
        string _methodSourceCode = 
        @"create list Numbers of PrettyIntegers { 1, 2, 3 };
        x = Numbers[1];
        ";
        EXEScopeMethod _scope = OALParserBridge.Parse(_methodSourceCode);

        VisitorPythonCode visitor = new VisitorPythonCode();

        // Act
        _scope.Accept(visitor);
        string _actualOutput = visitor.GetCommandString();

        // Assert
        string _expectedOutput = "Numbers = [1, 2, 3]\nx = Numbers[1]\n";

        Assert.AreEqual(_expectedOutput, _actualOutput);
    }
}
