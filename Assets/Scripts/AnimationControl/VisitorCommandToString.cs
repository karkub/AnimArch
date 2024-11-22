using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Msagl.Drawing;
using OALProgramControl;
using UnityEngine;

public class VisitorCommandToString : Visitor
{
    private readonly StringBuilder commandString;
    private bool simpleFormatting;
    private bool highlighting;
    private bool originalHighlighting;

    private int indentationLevel;

    public VisitorCommandToString()
    {
        commandString = new StringBuilder();
        ResetState();
    }

    public string GetCommandString() {
        string result = commandString.ToString();
        return result;
    }

    private void ResetState() {
        simpleFormatting = true;
        highlighting = false;
        originalHighlighting = true;
        indentationLevel = 0;
        commandString.Clear();
    }

    public void ActivateSimpleFormatting() {
        simpleFormatting = true;
        indentationLevel = 0;
    }

    public void DeactivateSimpleFormatting() {
        simpleFormatting = false;
    }

    public void ActivateHighlighting() {
        originalHighlighting = highlighting;
        highlighting = true;

    }

    public void DeactivateHighlighting() {
        highlighting = false;
        originalHighlighting = false;
    }

    private void HighlightingToOriginalState() {
        highlighting = originalHighlighting;
    }

    private void IncreaseIndentation() {
        indentationLevel++;
    }

    private void DecreaseIndentation() {
        indentationLevel--;
    }

    private void HighlightBegin(EXECommand command) {
        if (highlighting && command.IsActive) {
            commandString.Append("<b><color=green>");
        }

    }

    private void HighlightEnd(EXECommand command) {
        if (highlighting && command.IsActive) {
            commandString.Append("</color></b>");
        }
    }

    private void WriteIndentation() {
        if (simpleFormatting) {
            commandString.Append(string.Concat(Enumerable.Repeat("\t", indentationLevel)));
        }
    }

    private void AddEOL() {
        if (simpleFormatting) {
            commandString.Append(";\n");
        }
    }

    private void HandleBasicEXECommand(EXECommand command, Func<VisitorCommandToString, bool> addCommandSimpleString) {
        HighlightBegin(command);
        WriteIndentation();
        addCommandSimpleString(this);
        AddEOL();
        HighlightEnd(command);
    }

    public override void VisitExeCommand(EXECommand command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            visitor.commandString.Append("Command");
            return false;
        });
    }

    public override void VisitExeCommandBreak(EXECommandBreak command)
    {   
        HandleBasicEXECommand(command, (visitor) => {
            visitor.commandString.Append("break");
            return false;
        });
    }

    public override void VisitExeCommandCall(EXECommandCall command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            visitor.commandString.Append(command.MethodAccessChainS + ".");
            command.MethodCall.Accept(visitor);
            return false;
        });
    }

    public override void VisitExeCommandContinue(EXECommandContinue command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            visitor.commandString.Append("continue");
            return false;
        });
    }

    public override void VisitExeCommandListOperation(EXECommandListOperation command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            visitor.commandString.Append("EXECommandListOperation");
            return false;
        });
    }

    public override void VisitExeCommandAddingToList(EXECommandAddingToList command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            visitor.commandString.Append("add ");
            command.AddedElement.Accept(visitor);
            visitor.commandString.Append(" to ");
            command.Array.Accept(visitor);
            return false;
        });
    }

    public override void VisitExeCommandAssignment(EXECommandAssignment command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            command.AssignmentTarget.Accept(visitor);
            visitor.commandString.Append(" = ");
            command.AssignedExpression.Accept(visitor);
            return false;
        });
    }

    public override void VisitExeCommandCreateList(EXECommandCreateList command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            visitor.commandString.Append("create list ");
            command.AssignmentTarget.Accept(visitor);
            visitor.commandString.Append(" of " + command.ArrayType + " { ");
            bool first = true;
            foreach (var item in command.Items)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    commandString.Append(", ");
                }
                item.Accept(this);
            }
            visitor.commandString.Append(" }");
            return false;
        });
    }

    public override void VisitExeCommandFileAppend(EXECommandFileAppend command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            visitor.commandString.Append("append ");
            command.StringToWrite.Accept(visitor);
            visitor.commandString.Append(" to file ");
            command.FileToWriteTo.Accept(visitor);
            return false;
        });
    }

    public override void VisitExeCommandFileExists(EXECommandFileExists command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            visitor.commandString.Append("check ");
            command.AssignmentTarget.Accept(visitor);
            visitor.commandString.Append(" if file ");
            command.FileToCheck.Accept(visitor);
            visitor.commandString.Append(" exists");
            return false;
        });
    }

    public override void VisitExeCommandFileRead(EXECommandFileRead command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            visitor.commandString.Append("read ");
            command.AssignmentTarget.Accept(visitor);
            visitor.commandString.Append(" from file ");
            command.FileToReadFrom.Accept(visitor);
            return false;
        });
    }

    public override void VisitExeCommandFileWrite(EXECommandFileWrite command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            visitor.commandString.Append("write ");
            command.StringToWrite.Accept(visitor);
            visitor.commandString.Append(" to file ");
            command.FileToWriteTo.Accept(visitor);
            return false;
        });
    }

    public override void VisitExeCommandQueryCreate(EXECommandQueryCreate command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            visitor.commandString.Append("create object instance ");
            if (command.AssignmentTarget != null)
            {
                command.AssignmentTarget.Accept(visitor);
            }
            visitor.commandString.Append(" of " + command.ClassName);
            return false;
        });
    }

    public override void VisitExeCommandQueryDelete(EXECommandQueryDelete command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            visitor.commandString.Append("delete object instance ");
            command.DeletedVariable.Accept(visitor);
            return false;
        });
    }

    public override void VisitExeCommandRead(EXECommandRead command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            command.AssignmentTarget.Accept(visitor);
            visitor.commandString.Append(" = ");

            if (!EXETypes.StringTypeName.Equals(command.AssignmentType))
            {

                visitor.commandString.Append(command.AssignmentType);
                visitor.commandString.Append("(");
            }

            visitor.commandString.Append("read(");

            if (command.Prompt != null)
            {
                command.Prompt.Accept(visitor);
            }

            visitor.commandString.Append(")");

            if (!EXETypes.StringTypeName.Equals(command.AssignmentType))
            {
                visitor.commandString.Append(")");
            }
            return false;
        });
    }

    public override void VisitExeCommandRemovingFromList(EXECommandRemovingFromList command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            visitor.commandString.Append("remove ");
            command.Item.Accept(visitor);
            visitor.commandString.Append(" from ");
            command.Array.Accept(visitor);
            return false;
        });
    }

    public override void VisitExeCommandReturn(EXECommandReturn command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            visitor.commandString.Append("return");
            if (command.Expression != null)
            {
                visitor.commandString.Append(" ");
                command.Expression.Accept(visitor);
            }
            return false;
        });
    }


    public override void VisitExeCommandWait(EXECommandWait command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            visitor.commandString.Append("wait for ");
            command.WaitTime.Accept(visitor);
            visitor.commandString.Append(" seconds");

            return false;
        });
    }

    public override void VisitExeCommandWrite(EXECommandWrite command)
    {
        HandleBasicEXECommand(command, (visitor) => {
            visitor.commandString.Append("write(");
            bool first = true;
            foreach (var arg in command.Arguments)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    commandString.Append(", ");
                }
                arg.Accept(this);
            }
            visitor.commandString.Append(")");
            return false;
        });
    }

    public override void VisitExeScopeNull(EXEScopeNull scope)
    {
        HandleBasicEXECommand(scope, (visitor) => {
            visitor.commandString.Append("EXEScopeNull");

            return false;
        });
    }

    public override void VisitExeScope(EXEScope scope)
    {
        foreach (EXECommand Command in scope.Commands)
        {
            Command.Accept(this);
        }
    }

    public override void VisitExeScopeLoop(EXEScopeLoop scope)
    {
        
        foreach (EXECommand Command in scope.Commands)
        {
            Command.Accept(this);
        }
    }

    public override void VisitExeScopeMethod(EXEScopeMethod scope)
    {
        foreach (EXECommand Command in scope.Commands)
        {
            Command.Accept(this);
        }
    }

    public override void VisitExeScopeForEach(EXEScopeForEach scope)
    { 
        HighlightBegin(scope);
        WriteIndentation();
        commandString.Append("for each " + scope.IteratorName + " in ");
        scope.Iterable.Accept(this);
        commandString.Append("\n");
        HighlightEnd(scope);

        ActivateHighlighting();
        IncreaseIndentation();
        foreach (EXECommand Command in scope.Commands)
        {
            Command.Accept(this);
        }
        HighlightingToOriginalState();
        DecreaseIndentation();

        HighlightBegin(scope);
        WriteIndentation();
        commandString.Append("end for");
        AddEOL();
        HighlightEnd(scope);
    }

    public override void VisitExeScopeParallel(EXEScopeParallel scope)
    {
        HighlightBegin(scope);
        WriteIndentation();
        commandString.Append("par\n");
        HighlightEnd(scope);


        if (scope.Threads != null)
        {
            foreach (EXEScope Thread in scope.Threads)
            {
                
                HighlightBegin(scope);
                WriteIndentation();
                commandString.Append("\tthread\n");
                HighlightEnd(scope);


                ActivateHighlighting();
                IncreaseIndentation();
                IncreaseIndentation();
                foreach (EXECommand Command in Thread.Commands)
                {
                    Command.Accept(this);
                }
                DecreaseIndentation();
                DecreaseIndentation();
                HighlightingToOriginalState();

                HighlightBegin(scope);
                WriteIndentation();
                commandString.Append("\tend thread");
                AddEOL();
                HighlightEnd(scope);
            }
        }

        HighlightBegin(scope);
        WriteIndentation();
        commandString.Append("end par");
        AddEOL();
        HighlightEnd(scope);
    }

    public override void VisitExeScopeCondition(EXEScopeCondition scope)
    {
        HighlightBegin(scope);
        WriteIndentation();
        commandString.Append("if (");
        scope.Condition.Accept(this);
        commandString.Append(")\n");
        HighlightEnd(scope);

        ActivateHighlighting();
        IncreaseIndentation();
        foreach (EXECommand Command in scope.Commands)
        {
            Command.Accept(this);
        }
        DecreaseIndentation();
        HighlightingToOriginalState();

        if (scope.ElifScopes != null)
        {
            foreach (EXEScopeCondition Elif in scope.ElifScopes)
            {
                HighlightBegin(scope);
                WriteIndentation();
                commandString.Append("elif (");
                Elif.Condition.Accept(this);
                commandString.Append(")\n");
                HighlightEnd(scope);

                ActivateHighlighting();
                IncreaseIndentation();
                foreach (EXECommand Command in Elif.Commands)
                {
                    Command.Accept(this);
                }
                DecreaseIndentation();
                HighlightingToOriginalState();
            }
        }
            if (scope.ElseScope != null)
            {
                HighlightBegin(scope);
                WriteIndentation();
                commandString.Append("else\n");
                HighlightEnd(scope);

                ActivateHighlighting();
                IncreaseIndentation();
                foreach (EXECommand Command in scope.ElseScope.Commands)
                {
                    Command.Accept(this);
                }
                DecreaseIndentation();
                HighlightingToOriginalState();
            }

            HighlightBegin(scope);
            WriteIndentation();
            commandString.Append("end if");
            AddEOL();
            HighlightEnd(scope);

    }

    public override void VisitExeScopeLoopWhile(EXEScopeLoopWhile scope)
    {
        HighlightBegin(scope);
        WriteIndentation();
        commandString.Append("while (");
        scope.Condition.Accept(this);
        commandString.Append(")\n");
        HighlightEnd(scope);

        ActivateHighlighting();
        IncreaseIndentation();
        foreach (EXECommand Command in scope.Commands)
        {
            Command.Accept(this);
        }
        DecreaseIndentation();
        HighlightingToOriginalState();

        HighlightBegin(scope);
        WriteIndentation();
        commandString.Append("end while");
        AddEOL();
        HighlightEnd(scope);

    }

    public override void VisitExeASTNodeAccesChain(EXEASTNodeAccessChain node)
    {
        bool first = true;
        foreach (var element in node.GetElements()) {
            if (first)
            {
                first = false;
            }
            else
            {
                commandString.Append(".");
            }
            element.NodeValue.Accept(this);
        }
    }

    public override void VisitExeASTNodeComposite(EXEASTNodeComposite node)
    {
        if (node.Operands.Count == 1)
        {
            commandString.Append(node.Operation + " ");
            node.Operands.First().Accept(this);
        }
        else
        {
            bool first = true;
            foreach (var operand in node.Operands) {
                if (first)
                {
                    first = false;
                }
                else
                {
                    commandString.AppendFormat(" {0} ", node.Operation);
                }
                operand.Accept(this);
            }
        }
    }

    public override void VisitExeASTNodeLeaf(EXEASTNodeLeaf node)
    {
        commandString.Append(node.Value);
    }

    public override void VisitExeASTNodeMethodCall(EXEASTNodeMethodCall node)
    {
        commandString.Append(node.MethodName + "(");
        bool first = true;
        foreach (var arg in node.Arguments) {
            if (first)
            {
                first = false;
            }
            else
            {
                commandString.Append(", ");
            }
            arg.Accept(this);
        }
        commandString.Append(")");
    }

    public override void VisitExeValueArray(EXEValueArray value)
    {
        if (value.Elements == null)
        {
            commandString.Append(EXETypes.UnitializedName);
        }
        else
        {
            commandString.Append("[");
            bool first = true;
            foreach (var element in value.Elements) {
                if (first)
                {
                    first = false;
                }
                else
                {
                    commandString.Append(", ");
                }
                element.Accept(this);
            }
            commandString.Append("]");
        }
    }

    public override void VisitExeValueBool(EXEValueBool value)
    {
        commandString.Append(value.Value ? EXETypes.BooleanTrue : EXETypes.BooleanFalse);
    }

    public override void VisitExeValueInt(EXEValueInt value)
    {
        commandString.Append(value.Value.ToString());
    }

    public override void VisitExeValueReal(EXEValueReal value)
    {
        commandString.Append(value.Value.ToString());
    }

    public override void VisitExeValueReference(EXEValueReference value)
    {
        commandString.Append(value.TypeName + "<" + value.ClassInstance.UniqueID + ">");
    }

    public override void VisitExeValueString(EXEValueString value)
    {
        commandString.Append("\"" + value.Value + "\"");
    }

    public override void VisitExeASTNodeIndexation(EXEASTNodeIndexation node)
    {
        node.List.Accept(this);
        commandString.Append("[");
        node.Index.Accept(this);
        commandString.Append("]");
    }
}
