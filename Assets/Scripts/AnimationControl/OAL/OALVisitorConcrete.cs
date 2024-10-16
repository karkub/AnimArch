using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;
using System.Collections;
using OALProgramControl;
using UnityEngine;
using AnimArch.Extensions;
using System.Runtime.Remoting.Messaging;

namespace AnimationControl.OAL
{
    public class OALVisitorConcrete : OALBaseVisitor<object>
    {
        public OALVisitorConcrete() { }

        private void HandleError(string errorMessage, ParserRuleContext context)
        {
            throw new Exception(string.Format("Error while processing '{0}'.\n-------------------\n{1}\nChildren-wise: '{2}'", errorMessage, context?.GetText(), string.Join(", ", context?.children?.Select(child => child?.GetText()) ?? new string[] { } )));
        }

        // AccessChain
        public override object VisitAccessChain([NotNull] OALParser.AccessChainContext context)
        {
            if (context.accessChainElement() == null)
            {
                HandleError("Visiting AccessChain - 'accessChainElement' cannot be null.", context);
            }

            EXEASTNodeAccessChain accessChain = null;

            if (context.accessChainPrefix() != null)
            {
                object prefixResult = Visit(context.accessChainPrefix());

                if (prefixResult == null || prefixResult is not EXEASTNodeAccessChain)
                {
                    HandleError(string.Format("Access chain prefix exists, but the result of visiting is '{0}'.", prefixResult?.GetType().Name ?? "NULL"), context);
                }

                accessChain = prefixResult as EXEASTNodeAccessChain;
            }

            accessChain ??= new EXEASTNodeAccessChain();

            object currentAccessChainElementResult = Visit(context.accessChainElement());

            if (currentAccessChainElementResult == null || currentAccessChainElementResult is not EXEASTNodeBase)
            {
                HandleError(string.Format("Access chain element exists, but the result of visiting is '{0}'.", currentAccessChainElementResult?.GetType().Name ?? "NULL"), context);
            }

            accessChain.AddElement(currentAccessChainElementResult as EXEASTNodeBase);

            return accessChain;
        }
        // AccessChainElement
        public override object VisitAccessChainElement([NotNull] OALParser.AccessChainElementContext context)
        {
            if (context.ChildCount == 4 && "[".Equals(context.GetChild(1).GetText()) && "]".Equals(context.GetChild(3).GetText())) {
                object list = Visit(context.GetChild(0));
                if (list is not EXEASTNodeBase || list == null) {
                    HandleError(string.Format("Malformed expression - list of indexation is not EXEASTNodeBase, instead it is '{0}'.", list?.GetType().Name ?? "NULL"), context);
                }

                object index = Visit(context.GetChild(2));
                if (index is not EXEASTNodeBase || index == null) {
                    HandleError(string.Format("Malformed expression - index of indexation is not EXEASTNodeBase, instead it is '{0}'.", index?.GetType().Name ?? "NULL"), context);
                }
                return new EXEASTNodeIndexation(list as EXEASTNodeBase, index as EXEASTNodeBase);
            }
            else if (context.ChildCount == 1)
            {
                if (context.NAME() == null && context.methodCall() == null) { HandleError("MalformedAccessChainElement", context); }
                if (context.NAME() != null && context.methodCall() != null) { HandleError("MalformedAccessChainElement", context); }

                if (context.NAME() != null)
                {
                    return VisitTerminal(context.NAME());
                }
                else if (context.methodCall() != null)
                {
                    return Visit(context.methodCall());
                }
            }

            HandleError("Unknown parsing error", context);
            return null;
        }
        // AccessChainPrefix
        public override object VisitAccessChainPrefix([NotNull] OALParser.AccessChainPrefixContext context)
        {
            if (context.ChildCount < 1) { HandleError("Malformed AccessChainPrefix.", context); }

            if (context.ChildCount % 2 != 0) { HandleError("Malformed AccessChainPrefix.", context); }

            if (context.children.Where((item, index) => index % 2 == 1).Any(item => !".".Equals(item.GetText()))) { HandleError("Malformed AccessChainPrefix.", context); }

            if (context.accessChainElement() == null) { HandleError("Malformed AccessChainPrefix.", context); }

            if (context.accessChainElement().Length != context.ChildCount / 2) { HandleError("Malformed AccessChainPrefix.", context); }

            IEnumerable<object> accessChainElementResults = context.accessChainElement().Select(expression => Visit(expression));

            if (accessChainElementResults.Any(accessChainElementResult => accessChainElementResult == null || accessChainElementResult is not EXEASTNodeBase))
            {
                HandleError("One of the access chain elements is not EXEASTNodeBase.", context);
            }

            EXEASTNodeAccessChain accessChain = new EXEASTNodeAccessChain();

            foreach (object accessChainElement in accessChainElementResults)
            {
                accessChain.AddElement(accessChainElement as EXEASTNodeBase);
            }

            return accessChain;
        }
        // Attribute - attribute name
        public override object VisitAttribute([NotNull] OALParser.AttributeContext context)
        {
            return VisitTerminal(context.NAME());
        }
        // Bracketed expression
        public override object VisitBracketedExpr([NotNull] OALParser.BracketedExprContext context)
        {
            if
            (
                !(
                    context.ChildCount == 3
                    && "(".Equals(context.GetChild(0).GetText())
                    && context.expr() != null
                    && ")".Equals(context.GetChild(2).GetText())
                )
            )
            {
                HandleError("Malformed bracketed expression.", context);
            }

            object expression = VisitExpr(context.expr());

            if (expression is not EXEASTNodeBase || expression == null)
            {
                HandleError(string.Format("Bracketed expression contains '{0}' instead of EXEASTNodeBase.", expression?.GetType().Name ?? "NULL"), context);
            }

            (expression as EXEASTNodeBase).IncrementBracketLevel();

            return expression;
        }
        // Break command
        public override object VisitBreakCommand([NotNull] OALParser.BreakCommandContext context)
        {
            if (context.ChildCount != 2 && "break".Equals(context.GetChild(0).GetText()))
            {
                HandleError("Malformed break command.", context);
            }

            EXECommandBreak result = new EXECommandBreak();
            CommandCreationRoutine(result);

            return result;
        }
        // Class name
        public override object VisitClassName([NotNull] OALParser.ClassNameContext context)
        {
            string name = context.GetChild(0).GetText();

            if (context.ChildCount != 1 || !EXETypes.IsValidClassName(name))
            {
                HandleError("Malformed class name.", context);
            }

            return name;
        }
        // A sequence of commands, e.g. in IF or WHILE block
        public override object VisitCommands([NotNull] OALParser.CommandsContext context)
        {
            object parsedChild;

            int i = 0;
            List<EXECommand> result = new List<EXECommand>();
            foreach (IParseTree child in context.children)
            {
                parsedChild = Visit(child);

                if (parsedChild is EXEComment && parsedChild != null)
                {
                    continue;
                }

                if (parsedChild is not EXECommand || parsedChild == null)
                {
                    HandleError(string.Format("{0}th child of 'Commands' is not a command, instead it is '{1}'.", i, parsedChild?.GetType().Name ?? "NULL"), context);
                    return null;
                }

                result.Add(parsedChild as EXECommand);

                i++;
            }

            return result;
        }
        // A comment in code
        public override object VisitCommentCommand([NotNull] OALParser.CommentCommandContext context)
        {
            if (!"//".Equals(context.GetChild(0).GetText().Substring(0, 2)))
            {
                HandleError("Malformed comment.", context);
            }

            string commentText = string.Join(string.Empty, context.children.Skip(1).Select(child => child.GetText()));

            return new EXEComment(commentText);
        }
        // Condition (expression and brackets) of IF and WHILE block
        public override object VisitCondition([NotNull] OALParser.ConditionContext context)
        {
            if
            (
                !(
                    context.ChildCount == 3
                    && "(".Equals(context.GetChild(0).GetText())
                    && context.expr() != null
                    && ")".Equals(context.GetChild(2).GetText())
                )
            )
            {
                HandleError("Malformed condition.", context);
            }

            object expression = VisitExpr(context.expr());

            if (expression is not EXEASTNodeBase || expression == null)
            {
                HandleError(string.Format("Condition contains '{0}' instead of EXEASTNodeBase.", expression?.GetType().Name ?? "NULL"), context);
            }

            return expression;
        }
        // Continue command
        public override object VisitContinueCommand([NotNull] OALParser.ContinueCommandContext context)
        {
            if (context.ChildCount != 2 && "continue".Equals(context.GetChild(0).GetText()))
            {
                HandleError("Malformed continue command.", context);
            }

            EXECommandContinue result = new EXECommandContinue();
            CommandCreationRoutine(result);

            return result;
        }
        // Elif block in an IF command
        public override object VisitElif([NotNull] OALParser.ElifContext context)
        {
            if (context.ChildCount != 3 || !"elif".Equals(context.GetChild(0).GetText()))
            {
                HandleError("Malformed 'elif'.", context);
            }

            object condition = Visit(context.GetChild(1));

            if (condition is not EXEASTNodeBase || condition == null)
            {
                HandleError(string.Format("Condition in 'elif' is '{0}' instead of an expression.", condition?.GetType().Name ?? "NULL"), context);
            }

            object commands = Visit(context.GetChild(2));

            if (commands is not List<EXECommand> || commands == null)
            {
                HandleError(string.Format("Commands in 'elif' is '{0}' instead of a list commands.", commands?.GetType().Name ?? "NULL"), context);
            }

            EXEScopeCondition result = new EXEScopeCondition(condition as EXEASTNodeBase);
            CommandCreationRoutine(result);
            (commands as List<EXECommand>).ForEach(command => result.AddCommand(command));

            return result;
        }
        // Else block in an IF command
        public override object VisitElseBlock([NotNull] OALParser.ElseBlockContext context)
        {
            if (context.ChildCount != 2 || !"else".Equals(context.GetChild(0).GetText()))
            {
                HandleError("Malformed 'else'.", context);
            }

            object commands = Visit(context.GetChild(1));

            if (commands is not List<EXECommand> || commands == null)
            {
                HandleError(string.Format("Commands in 'else' is '{0}' instead of a list commands.", commands?.GetType().Name ?? "NULL"), context);
            }

            EXEScope result = new EXEScope();
            CommandCreationRoutine(result);
            (commands as List<EXECommand>).ForEach(command => result.AddCommand(command));

            return result;
        }
        // Add to list
        public override object VisitExeCommandAddingToList([NotNull] OALParser.ExeCommandAddingToListContext context)
        {
            if (context.ChildCount != 5) { HandleError("Malformed adding to list command.", context); }
            if (!"add ".Equals(context.GetChild(0).GetText())) { HandleError("Malformed adding to list command.", context); }
            if (!" to ".Equals(context.GetChild(2).GetText())) { HandleError("Malformed adding to list command.", context); }

            object item = Visit(context.GetChild(1));

            if (item is not EXEASTNodeBase || item == null)
            {
                HandleError(string.Format("Added item in adding to list command is '{0}' instead of an expression.", item?.GetType().Name ?? "NULL"), context);
            }

            object list = Visit(context.GetChild(3));

            if (list is not EXEASTNodeBase || list == null)
            {
                HandleError(string.Format("Array in adding to list command is '{0}' instead of an expression.", list?.GetType().Name ?? "NULL"), context);
            }

            EXECommandAddingToList result = new EXECommandAddingToList(list as EXEASTNodeBase, item as EXEASTNodeBase);
            CommandCreationRoutine(result);

            return result;
        }
        // Assignment command
        public override object VisitExeCommandAssignment([NotNull] OALParser.ExeCommandAssignmentContext context)
        {
            if (context.accessChain() == null || context.expr() == null)
            {
                HandleError("Malformed assignment command.", context);
            }
            
            object assignmentTarget = VisitAccessChain(context.accessChain());
            
            if (assignmentTarget is not EXEASTNodeAccessChain || assignmentTarget == null)
            {
                HandleError(string.Format("Malformed assignment command - assignment target is not supposed to be '{0}'.", assignmentTarget?.GetType().Name ?? "NULL"), context);
            }

            object assignedExpression = VisitExpr(context.expr());

            if (assignedExpression is not EXEASTNodeBase || assignedExpression == null)
            {
                HandleError(string.Format("Malformed assignment command - assigned expression is not supposed to be '{0}'.", assignedExpression?.GetType().Name ?? "NULL"), context);
            }

            EXECommandAssignment result = new EXECommandAssignment(assignmentTarget as EXEASTNodeAccessChain, assignedExpression as EXEASTNodeBase);
            CommandCreationRoutine(result);

            return result;
        }
        // Method invocation command
        public override object VisitExeCommandCall([NotNull] OALParser.ExeCommandCallContext context)
        {
            EXEASTNodeAccessChain accessChain = null;

            if (context.accessChain() != null)
            {
                if (!".".Equals(context.GetChild(1).GetText()))
                {
                    HandleError("Malformed access chain in method invocation command.", context);
                }

                object accessChainResult = Visit(context.accessChain());

                if (accessChainResult is not EXEASTNodeAccessChain || accessChainResult == null)
                {
                    HandleError("Malformed access chain in method invocation command.", context);
                }

                accessChain = accessChainResult as EXEASTNodeAccessChain;
            }

            object methodCall = Visit(context.methodCall());

            if (methodCall is not EXEASTNodeMethodCall || methodCall == null)
            {
                HandleError("Malformed method call in method invocation command.", context);
            }

            EXECommandCall result = new EXECommandCall(accessChain, methodCall as EXEASTNodeMethodCall);
            CommandCreationRoutine(result);

            return result;
        }
        // Array initialization command
        public override object VisitExeCommandCreateList([NotNull] OALParser.ExeCommandCreateListContext context)
        {
            if (!(context.ChildCount == 5 || context.ChildCount == 6) || !"create list ".Equals(context.GetChild(0).GetText()) || !" of ".Equals(context.GetChild(2).GetText()))
            {
                HandleError("Malformed list creation command.", context);
            }

            object assignmentTarget = Visit(context.accessChain());

            if (assignmentTarget is not EXEASTNodeAccessChain || assignmentTarget == null)
            {
                HandleError(string.Format("Malformed list creation command - assignment target is not supposed to be '{0}'.", assignmentTarget?.GetType().Name ?? "NULL"), context);
            }

            object elementType = Visit(context.typeName());

            if (elementType is not string || elementType == null)
            {
                HandleError(string.Format("Malformed list creation command - element type name is not supposed to be '{0}'.", elementType?.GetType().Name ?? "NULL"), context);
            }

            List<EXEASTNodeBase> listElements = new List<EXEASTNodeBase>();
            if (context.listLiteral() != null)
            {
                object listElementsResult = Visit(context.listLiteral());

                if (listElementsResult is not List<EXEASTNodeBase> || listElementsResult == null)
                {
                    HandleError("Malformed list literal in method invocation command.", context);
                }

                listElements = listElementsResult as List<EXEASTNodeBase>;
            }

            EXECommandCreateList result
                = new EXECommandCreateList(elementType as string, assignmentTarget as EXEASTNodeAccessChain, listElements);
            CommandCreationRoutine(result);

            return result;
        }
        // Append to file command
        public override object VisitExeCommandFileAppend([NotNull] OALParser.ExeCommandFileAppendContext context)
        {
            if(context.ChildCount != 5) { HandleError("Malformed append to file command.", context); }
            if (!"append ".Equals(context.GetChild(0).GetText())) { HandleError("Malformed append to file command.", context); }
            if (!" to file ".Equals(context.GetChild(2).GetText())) { HandleError("Malformed append to file command.", context); }
            if(context.expr() == null) { HandleError("Malformed append to file command.", context); }
            if (context.expr().Length != 2) { HandleError("Malformed append to file command.", context); }

            object textToAppend = Visit(context.expr()[0]);

            if (textToAppend == null || textToAppend is not EXEASTNodeBase)
            {
                HandleError(string.Format("Malformed append to file command - text to append is not supposed to be '{0}'.", textToAppend?.GetType().Name ?? "NULL"), context);
            }

            object fileToAppendTo = Visit(context.expr()[1]);

            if (fileToAppendTo == null || fileToAppendTo is not EXEASTNodeBase)
            {
                HandleError(string.Format("Malformed append to file command - file to append to is not supposed to be '{0}'.", fileToAppendTo?.GetType().Name ?? "NULL"), context);
            }

            EXECommandFileAppend result
                = new EXECommandFileAppend(textToAppend as EXEASTNodeBase, fileToAppendTo as EXEASTNodeBase);
            CommandCreationRoutine(result);

            return result;
        }
        // Check from file command
        public override object VisitExeCommandFileCheck([NotNull] OALParser.ExeCommandFileCheckContext context)
        {
            if (context.ChildCount != 6) { HandleError("Malformed check file command.", context); }
            if (!"check ".Equals(context.GetChild(0).GetText())) { HandleError("Malformed check file command.", context); }
            if (!" if file ".Equals(context.GetChild(2).GetText())) { HandleError("Malformed check file command.", context); }
            if (!" exists".Equals(context.GetChild(4).GetText())) { HandleError("Malformed check file command.", context); }
            if (context.expr() == null) { HandleError("Malformed check file command.", context); }
            if (context.accessChain() == null) { HandleError("Malformed check file command.", context); }

            object assignmentTarget = Visit(context.accessChain());

            if (assignmentTarget == null || assignmentTarget is not EXEASTNodeAccessChain)
            {
                HandleError(string.Format("Malformed check file command - assignment target is not supposed to be '{0}'.", assignmentTarget?.GetType().Name ?? "NULL"), context);
            }

            object fileToCheck = Visit(context.expr());

            if (fileToCheck == null || fileToCheck is not EXEASTNodeBase)
            {
                HandleError(string.Format("Malformed check file command - file to check is not supposed to be '{0}'.", fileToCheck?.GetType().Name ?? "NULL"), context);
            }

            EXECommandFileExists result
                = new EXECommandFileExists(assignmentTarget as EXEASTNodeAccessChain, fileToCheck as EXEASTNodeBase);
            CommandCreationRoutine(result);

            return result;
        }
        // Read from file command
        public override object VisitExeCommandFileRead([NotNull] OALParser.ExeCommandFileReadContext context)
        {
            if (context.ChildCount != 5) { HandleError("Malformed read from file command.", context); }
            if (!"read ".Equals(context.GetChild(0).GetText())) { HandleError("Malformed read from file command.", context); }
            if (!" from file ".Equals(context.GetChild(2).GetText())) { HandleError("Malformed read from file command.", context); }
            if (context.expr() == null) { HandleError("Malformed read from file command.", context); }
            if (context.accessChain() == null) { HandleError("Malformed read from file command.", context); }

            object assignmentTarget = Visit(context.accessChain());

            if (assignmentTarget == null || assignmentTarget is not EXEASTNodeAccessChain)
            {
                HandleError(string.Format("Malformed read from file command - assignment target is not supposed to be '{0}'.", assignmentTarget?.GetType().Name ?? "NULL"), context);
            }

            object fileToReadFrom = Visit(context.expr());

            if (fileToReadFrom == null || fileToReadFrom is not EXEASTNodeBase)
            {
                HandleError(string.Format("Malformed read from file command - file to read from is not supposed to be '{0}'.", fileToReadFrom?.GetType().Name ?? "NULL"), context);
            }

            EXECommandFileRead result
                = new EXECommandFileRead(assignmentTarget as EXEASTNodeAccessChain, fileToReadFrom as EXEASTNodeBase);
            CommandCreationRoutine(result);

            return result;
        }
        // Write to file command
        public override object VisitExeCommandFileWrite([NotNull] OALParser.ExeCommandFileWriteContext context)
        {
            if (context.ChildCount != 5) { HandleError("Malformed write to file command.", context); }
            if (!"write ".Equals(context.GetChild(0).GetText())) { HandleError("Malformed write to file command.", context); }
            if (!" to file ".Equals(context.GetChild(2).GetText())) { HandleError("Malformed write to file command.", context); }
            if (context.expr() == null) { HandleError("Malformed write to file command.", context); }
            if (context.expr().Length != 2) { HandleError("Malformed write to file command.", context); }

            object textToWrite = Visit(context.expr()[0]);

            if (textToWrite == null || textToWrite is not EXEASTNodeBase)
            {
                HandleError(string.Format("Malformed write to file command - text to write is not supposed to be '{0}'.", textToWrite?.GetType().Name ?? "NULL"), context);
            }

            object fileToWriteTo = Visit(context.expr()[1]);

            if (fileToWriteTo == null || fileToWriteTo is not EXEASTNodeBase)
            {
                HandleError(string.Format("Malformed write to file command - file to write to is not supposed to be '{0}'.", fileToWriteTo?.GetType().Name ?? "NULL"), context);
            }

            EXECommandFileWrite result
                = new EXECommandFileWrite(textToWrite as EXEASTNodeBase, fileToWriteTo as EXEASTNodeBase);
            CommandCreationRoutine(result);

            return result;
        }
        // Object instantiation command
        public override object VisitExeCommandQueryCreate([NotNull] OALParser.ExeCommandQueryCreateContext context)
        {
            if
            (
                !(context.ChildCount == 3 || context.ChildCount == 5)
            )
            {
                HandleError("Malformed object instantiation command.", context);
            }

            if
            (
                context.ChildCount == 3 && !("create object instance of ".Equals(context.GetChild(0).GetText()) && context.accessChain() == null)
            )
            {
                HandleError("Malformed object instantiation command.", context);
            }

            if
            (
                context.ChildCount == 5 && !("create object instance ".Equals(context.GetChild(0).GetText()) && " of ".Equals(context.GetChild(2).GetText()) && context.accessChain() != null)
            )
            {
                HandleError("Malformed object instantiation command.", context);
            }

            EXEASTNodeAccessChain assignmentTarget = null;
            if (context.accessChain() != null)
            {
                object assignmentTargetResult = Visit(context.accessChain());

                if (assignmentTargetResult is not EXEASTNodeAccessChain || assignmentTargetResult == null)
                {
                    HandleError(string.Format("Malformed object instantiation command - assignment target is not supposed to be '{0}'.", assignmentTargetResult?.GetType().Name ?? "NULL"), context);
                }

                assignmentTarget = assignmentTargetResult as EXEASTNodeAccessChain;
            }

            object className = Visit(context.className());

            if (className is not string || className == null)
            {
                HandleError(string.Format("Malformed object instantiation command - class name is not supposed to be '{0}'.", className?.GetType().Name ?? "NULL"), context);
            }

            EXECommandQueryCreate result = new EXECommandQueryCreate(className as string, assignmentTarget);
            CommandCreationRoutine(result);

            return result;
        }
        // Object deletion command
        public override object VisitExeCommandQueryDelete([NotNull] OALParser.ExeCommandQueryDeleteContext context)
        {
            if (context.ChildCount != 3 || !"delete object instance ".Equals(context.GetChild(0).GetText()))
            {
                HandleError("Malformed list creation command.", context);
            }

            object expression = Visit(context.expr());

            if (expression is not EXEASTNodeBase || expression == null)
            {
                HandleError(string.Format("Malformed object deletion command - expression is not supposed to be '{0}'.", expression?.GetType().Name ?? "NULL"), context);
            }

            EXECommandQueryDelete result = new EXECommandQueryDelete(expression as EXEASTNodeBase);
            CommandCreationRoutine(result);

            return result;
        }
        // Read from console command
        public override object VisitExeCommandRead([NotNull] OALParser.ExeCommandReadContext context)
        {
            if (!(context.ChildCount == 6 || context.ChildCount == 7 || (context.ChildCount == 8 && "assign".Equals(context.GetChild(0).GetText()))))
            {
                HandleError("Malformed console read command.", context);
            }

            int offsetAssignKeyword = context.ChildCount == 8 ? 1 : 0;
            int offsetExpression = context.expr() == null ? 0 : 1;

            if (!"=".Equals(context.GetChild(1 + offsetAssignKeyword).GetText())) { HandleError("Malformed console read command.", context); }
            if (!"(read(".Equals(context.GetChild(3 + offsetAssignKeyword).GetText())) { HandleError("Malformed console read command.", context); }
            if (!"))".Equals(context.GetChild(4 + offsetAssignKeyword + offsetExpression).GetText())) { HandleError("Malformed console read command.", context); }

            object accessChain = Visit(context.accessChain());

            if (accessChain is not EXEASTNodeAccessChain || accessChain == null)
            {
                HandleError(string.Format("Malformed console read command - accessChain is not supposed to be '{0}'.", accessChain?.GetType().Name ?? "NULL"), context);
            }

            object className = Visit(context.className());

            if (className is not string || className == null)
            {
                HandleError(string.Format("Malformed console read command - type is not supposed to be '{0}'.", className?.GetType().Name ?? "NULL"), context);
            }

            EXEASTNodeBase promptExpression = null;
            if (context.expr() != null)
            {
                object promptResult = Visit(context.expr());

                if (promptResult is not EXEASTNodeBase || promptResult == null)
                {
                    HandleError(string.Format("Malformed console read command - prompt is not supposed to be '{0}'.", promptResult?.GetType().Name ?? "NULL"), context);
                }

                promptExpression = promptResult as EXEASTNodeBase;
            }

            EXECommandRead result = new EXECommandRead(className as string, accessChain as EXEASTNodeAccessChain, promptExpression);
            CommandCreationRoutine(result);

            return result;
        }
        // Remove from list command
        public override object VisitExeCommandRemovingFromList([NotNull] OALParser.ExeCommandRemovingFromListContext context)
        {
            if (context.ChildCount != 5 || !"remove ".Equals(context.GetChild(0).GetText()) || !" from ".Equals(context.GetChild(2).GetText()))
            {
                HandleError("Malformed removing from list.", context);
            }

            object item = Visit(context.GetChild(1));

            if (item is not EXEASTNodeBase || item == null)
            {
                HandleError(string.Format("Added item in removing from list command is '{0}' instead of an expression.", item?.GetType().Name ?? "NULL"), context);
            }

            object list = Visit(context.GetChild(3));

            if (list is not EXEASTNodeBase || list == null)
            {
                HandleError(string.Format("Array in removing from list command is '{0}' instead of an expression.", list?.GetType().Name ?? "NULL"), context);
            }

            EXECommandRemovingFromList result = new EXECommandRemovingFromList(list as EXEASTNodeBase, item as EXEASTNodeBase);
            CommandCreationRoutine(result);

            return result;
        }
        // Wait for seconds command
        public override object VisitExeCommandWait([NotNull] OALParser.ExeCommandWaitContext context)
        {
            if (context.ChildCount != 4) { HandleError("Malformed wait command.", context); }
            if (!"wait for ".Equals(context.GetChild(0).GetText())) { HandleError("Malformed wait command.", context); }
            if (!" seconds".Equals(context.GetChild(2).GetText())) { HandleError("Malformed wait command.", context); }
            if (context.expr() == null) { HandleError("Malformed wait command.", context); }

            object waitSecondsExpression = Visit(context.expr());

            if (waitSecondsExpression is not EXEASTNodeBase || waitSecondsExpression == null)
            {
                HandleError(string.Format("Wait time in wait command is '{0}' instead of an expression.", waitSecondsExpression?.GetType().Name ?? "NULL"), context);
            }

            EXECommandWait result = new EXECommandWait(waitSecondsExpression as EXEASTNodeBase);
            CommandCreationRoutine(result);

            return result;
        }
        // Write to console command
        public override object VisitExeCommandWrite([NotNull] OALParser.ExeCommandWriteContext context)
        {
            if (context.ChildCount != 4 || !"write(".Equals(context.GetChild(0).GetText()) || !")".Equals(context.GetChild(2).GetText()))
            {
                HandleError("Malformed console write command.", context);
            }

            object parameterList = Visit(context.parameterList());

            if (parameterList is not List<EXEASTNodeBase> || parameterList == null)
            {
                HandleError(string.Format("Params in console write command is '{0}' instead of an expression.", parameterList?.GetType().Name ?? "NULL"), context);
            }

            EXECommandWrite result = new EXECommandWrite(parameterList as List<EXEASTNodeBase>);
            CommandCreationRoutine(result);

            return result;
        }
        // Expression - something that can be calculated and has a result
        public override object VisitExpr([NotNull] OALParser.ExprContext context)
        {
            object result = null;

            if (context.NUM() != null)
            {
                result = VisitTerminal(context.NUM());
            }
            else if (context.BOOL() != null)
            {
                result = VisitTerminal(context.BOOL());
            }
            else if (context.STRING() != null)
            {
                result = VisitTerminal(context.STRING());
            }
            else if (context.accessChain() != null)
            {
                result = VisitAccessChain(context.accessChain());
            }
            else if (context.bracketedExpr() != null)
            {
                result = VisitBracketedExpr(context.bracketedExpr());
            }
            // Unary operator
            else if (context.ChildCount == 2)
            {
                string _operator = context.GetChild(0).GetText().ReplaceWhitespace();

                if (!EXETypes.IsUnaryOperator(_operator))
                {
                    HandleError(string.Format("Malformed expression - unknown unary operator '{0}'.", _operator), context);
                }

                object operand = Visit(context.GetChild(1));

                if (operand is not EXEASTNodeBase || operand == null)
                {
                    HandleError(string.Format("Malformed expression - operand of unary operator is not EXEASTNodeBase, instead it is '{0}'.", operand?.GetType().Name ?? "NULL"), context);
                }

                result = new EXEASTNodeComposite(_operator, new EXEASTNodeBase[] { operand as EXEASTNodeBase });
            }
            // Binary operator
            else if (context.ChildCount == 3)
            {
                string _operator = context.GetChild(1).GetText().ToString().ReplaceWhitespace();

                if (!EXETypes.IsBinaryOperator(_operator))
                {
                    HandleError("Malformed expression - unknown binary operator.", context);
                }

                object operand1 = Visit(context.GetChild(0));

                if (operand1 is not EXEASTNodeBase || operand1 == null)
                {
                    HandleError(string.Format("Malformed expression - operand of binary operator is not EXEASTNodeBase, instead it is '{0}'.", operand1?.GetType().Name ?? "NULL"), context);
                }

                object operand2 = Visit(context.GetChild(2));

                if (operand2 is not EXEASTNodeBase || operand2 == null)
                {
                    HandleError(string.Format("Malformed expression - operand of binary operator is not EXEASTNodeBase, instead it is '{0}'.", operand2?.GetType().Name ?? "NULL"), context);
                }

                result = new EXEASTNodeComposite(_operator, new EXEASTNodeBase[] { operand1 as EXEASTNodeBase, operand2 as EXEASTNodeBase });
            }
            else 
            {
                HandleError("Malformed expression - generic error.", context);
                result = null;
            }

            if (result is not EXEASTNodeBase || result == null)
            {
                HandleError(string.Format("Malformed expression - it is not supposed to be '{0}'.", result?.GetType().Name ?? "NULL"), context);
            }

            return result;
        }
        // FOREACH loop
        public override object VisitForeachCommand([NotNull] OALParser.ForeachCommandContext context)
        {
            if (context.ChildCount != 7 || !"for each ".Equals(context.GetChild(0).GetText()) || !" in ".Equals(context.GetChild(2).GetText()) || !"end for".Equals(context.GetChild(5).GetText()))
            {
                HandleError("Malformed foreach loop.", context);
            }

            object variableName = Visit(context.variableName());

            if (variableName is not string || variableName == null)
            {
                HandleError(string.Format("Malformed foreach loop - iterator name is not supposed to be '{0}'.", variableName?.GetType().Name ?? "NULL"), context);
            }

            object array = Visit(context.expr());

            if (array is not EXEASTNodeBase || array == null)
            {
                HandleError(string.Format("Iterable in foreach loop is '{0}' instead of an expression.", array?.GetType().Name ?? "NULL"), context);
            }

            object commands = Visit(context.commands());

            if (commands is not List<EXECommand> || commands == null)
            {
                HandleError(string.Format("Commands in foreach loop is '{0}' instead of a list of commands.", commands?.GetType().Name ?? "NULL"), context);
            }

            EXEScopeForEach result = new EXEScopeForEach(variableName as string, array as EXEASTNodeBase);
            CommandCreationRoutine(result);
            (commands as List<EXECommand>).ForEach(command => result.AddCommand(command));

            return result;
        }
        // IF block
        public override object VisitIfCommand([NotNull] OALParser.IfCommandContext context)
        {
            if (context.ChildCount < 5 || !"if".Equals(context.GetChild(0).GetText()) || !"end if".Equals(context.GetChild(context.ChildCount - 2).GetText()))
            {
                HandleError("Malformed if command.", context);
            }

            object condition = Visit(context.condition());

            if (condition is not EXEASTNodeBase || condition == null)
            {
                HandleError(string.Format("Condition in if command is '{0}' instead of an expression.", condition?.GetType().Name ?? "NULL"), context);
            }

            object commands = Visit(context.commands());

            if (commands is not List<EXECommand> || commands == null)
            {
                HandleError(string.Format("Commands in if command is '{0}' instead of a list of commands.", commands?.GetType().Name ?? "NULL"), context);
            }

            List<EXEScopeCondition> elifs = new List<EXEScopeCondition>();
            if (context.elif() != null)
            {
                object elifResult = null;

                foreach (OALParser.ElifContext elif in context.elif())
                {
                    elifResult = Visit(elif);

                    if (elifResult is not EXEScopeCondition || elifResult == null)
                    {
                        HandleError(string.Format("Elif in if command is '{0}' instead of a conditioned scope.", elifResult?.GetType().Name ?? "NULL"), context);
                    }

                    elifs.Add(elifResult as EXEScopeCondition);
                }
            }

            EXEScope elseScope = null;
            if (context.elseBlock() != null)
            {
                object elseScopeResult = Visit(context.elseBlock());

                if (elseScopeResult is not EXEScope || elseScopeResult == null)
                {
                    HandleError(string.Format("Else in if command is '{0}' instead of a list of a scope.", elseScopeResult?.GetType().Name ?? "NULL"), context);
                }

                elseScope = elseScopeResult as EXEScope;
            }

            EXEScopeCondition result = new EXEScopeCondition(condition as EXEASTNodeBase, elifs, elseScope);
            CommandCreationRoutine(result);
            (commands as List<EXECommand>).ForEach(command => result.AddCommand(command));

            return result;
        }
        // Single command
        public override object VisitLine([NotNull] OALParser.LineContext context)
        {
            if (context.ChildCount != 1)
            {
                HandleError("Line contains invalid count of children.", context);
            }

            Debug.Log(string.Format("Visiting '{0}', children-wise |{1}|.", context.GetText(), string.Join(", ", context.children.Select(child => "'" + child.GetText() + "'"))));
            return Visit(context.GetChild(0));
        }
        // A series of commands
        public override object VisitLines([NotNull] OALParser.LinesContext context)
        {
            EXEScopeMethod methodScope = new EXEScopeMethod();

            object parsedChild = null;

            for (int i = 0; i < context.ChildCount - 1; i++)
            {
                parsedChild = Visit(context.children[i]);

                if (parsedChild == null)
                {
                    HandleError("Parsed command is null.", context);
                    return null;
                }

                if (parsedChild is EXEComment)
                {
                    continue;
                }

                if (parsedChild is EXECommand)
                {
                    methodScope.AddCommand(parsedChild as EXECommand);
                    continue;
                }

                HandleError(string.Format("Lines contain something that is not a command - '{0}'.", context.children[i].GetText()), context);
            }

            if (!(string.Empty.Equals(context.GetText()) || "<EOF>".Equals(context.children.Last().GetText())))
            {
                HandleError(string.Format("Last element in lines is something other than EOF - '{0}'.", context.children.Last().GetText()), context);
            }

            return methodScope;
        }
        // List creation literal
        public override object VisitListLiteral([NotNull] OALParser.ListLiteralContext context)
        {
            if (context.ChildCount < 2 || !"{".Equals(context.GetChild(0).GetText()) || !"}".Equals(context.GetChild(2).GetText()))
            {
                HandleError("Malformed list literal.", context);
            }

            List<EXEASTNodeBase> parameterList = new List<EXEASTNodeBase>();
            if (context.parameterList() != null)
            {
                object parameterListResult = Visit(context.parameterList());

                if (parameterListResult is not List<EXEASTNodeBase> || parameterListResult == null)
                {
                    HandleError(string.Format("Parameter list in list literal is '{0}' instead of an expression list.", parameterListResult?.GetType().Name ?? "NULL"), context);
                }

                parameterList = parameterListResult as List<EXEASTNodeBase>;
            }

            return parameterList;
        }
        // Mthod call
        public override object VisitMethodCall([NotNull] OALParser.MethodCallContext context)
        {
            if (!(context.ChildCount == 3 || context.ChildCount == 4) || !"(".Equals(context.GetChild(1).GetText()) || !")".Equals(context.GetChild(context.ChildCount - 1).GetText()))
            {
                HandleError("Malformed method call.", context);
            }

            object methodName = Visit(context.methodName());

            if (methodName is not string || methodName == null)
            {
                HandleError(string.Format("Method name in method call is '{0}' instead of method name.", methodName?.GetType().Name ?? "NULL"), context);
            }

            List<EXEASTNodeBase> parameterList = new List<EXEASTNodeBase>();
            if (context.parameterList() != null)
            {
                object parameterListResult = Visit(context.parameterList());

                if (parameterListResult is not List<EXEASTNodeBase> || parameterListResult == null)
                {
                    HandleError(string.Format("Parameter list in method call is '{0}' instead of an expression list.", parameterListResult?.GetType().Name ?? "NULL"), context);
                }

                parameterList = parameterListResult as List<EXEASTNodeBase>;
            }

            EXEASTNodeMethodCall result = new EXEASTNodeMethodCall(methodName as string, parameterList);

            return result;
        }
        // Method name
        public override object VisitMethodName([NotNull] OALParser.MethodNameContext context)
        {
            string name = context.GetChild(0).GetText();

            if (context.ChildCount != 1 || !EXETypes.IsValidMethodName(name))
            {
                HandleError("Malformed method name.", context);
            }

            return name;
        }
        // Parameter list
        public override object VisitParameterList([NotNull] OALParser.ParameterListContext context)
        {
            if (!(context.ChildCount == 1 || context.ChildCount == 2))
            {
                HandleError("Malformed parameter list.", context);
            }

            if (context.expr() == null)
            {
                HandleError("Malformed parameter list - no expression.", context);
            }

            object parameter = Visit(context.expr());

            if (parameter is not EXEASTNodeBase || parameter == null)
            {
                HandleError(string.Format("Parameter in parameter list is '{0}' instead of an expression.", parameter?.GetType().Name ?? "NULL"), context);
            }

            List<EXEASTNodeBase> parameterList = new List<EXEASTNodeBase>() { parameter as EXEASTNodeBase };

            if (context.parameterListSuffix() != null)
            {
                object suffixResult = Visit(context.parameterListSuffix());

                if (suffixResult is not List<EXEASTNodeBase> || suffixResult == null)
                {
                    HandleError(string.Format("ParameterListSuffix in parameter list is '{0}' instead of List<EXEASTNodeBase>.", suffixResult?.GetType().Name ?? "NULL"), context);
                }

                parameterList.AddRange(suffixResult as List<EXEASTNodeBase>);
            }

            return parameterList;
        }
        // Parameter list suffix
        public override object VisitParameterListSuffix([NotNull] OALParser.ParameterListSuffixContext context)
        {
            if (context.ChildCount % 2 != 0) { HandleError("Malformed ParameterListSuffix.", context); }

            if (context.children.Where((item, index) => index % 2 == 0).Any(item => !",".Equals(item.GetText()))) { HandleError("Malformed ParameterListSuffix.", context); }

            if (context.expr() == null) { HandleError("Malformed ParameterListSuffix.", context); }

            if (context.expr().Length != context.ChildCount/2) { HandleError("Malformed ParameterListSuffix.", context); }

            IEnumerable<object> paramResults = context.expr().Select(expression => Visit(expression));

            if (paramResults.Any(paramResult => paramResult == null || paramResult is not EXEASTNodeBase))
            {
                HandleError("One of the parameters is not EXEASTNodeBase.", context);
            }

            return paramResults.Select(parameter => parameter as EXEASTNodeBase).ToList();
        }
        // Parallel command
        public override object VisitParCommand([NotNull] OALParser.ParCommandContext context)
        {
            if
            (
                context.ChildCount < 4
                || !"par".Equals(context.GetChild(0).GetText())
                || !"end par".Equals(context.GetChild(context.ChildCount - 2).GetText())
                || !context.threadCommand().Any()
            )
            {
                HandleError("Malformed parallel command.", context);
            }

            List<EXEScope> threads = new List<EXEScope>();
            object threadResult;
            foreach (OALParser.ThreadCommandContext thread in context.threadCommand())
            {
                threadResult = Visit(thread);

                if (threadResult is not EXEScope || threadResult == null)
                {
                    HandleError(string.Format("Thread in parallel command is '{0}' instead of a scope.", threadResult?.GetType().Name ?? "NULL"), context);
                }

                threads.Add(threadResult as EXEScope);
            }

            EXEScopeParallel result = new EXEScopeParallel(threads);
            CommandCreationRoutine(result);

            return result;
        }
        // Pragma command
        public override object VisitPragmaCommand([NotNull] OALParser.PragmaCommandContext context)
        {
            if (context.ChildCount != 2) { HandleError("Malformed pragma command.", context); }
            if (!"#".Equals(context.GetChild(0).GetText())) { HandleError("Malformed pragma command.", context); }

            string pragmaOption = context.GetChild(1).GetText();

            EXECommandPragma result = new EXECommandPragma(pragmaOption);
            CommandCreationRoutine(result);

            return result;
        }
        // Return command
        public override object VisitReturnCommand([NotNull] OALParser.ReturnCommandContext context)
        {
            if (!(context.ChildCount == 2 || context.ChildCount == 3) || !"return".Equals(context.GetChild(0).GetText()))
            {
                HandleError("Malformed return command.", context);
            }

            EXEASTNodeBase expression = null;
            if (context.expr() != null)
            {
                object expressionResult = Visit(context.expr());

                if (expressionResult is not EXEASTNodeBase || expressionResult == null)
                {
                    HandleError(string.Format("Returned value in return command is '{0}' instead of an expression.", expressionResult?.GetType().Name ?? "NULL"), context);
                }

                expression = expressionResult as EXEASTNodeBase;
            }

            EXECommandReturn result = new EXECommandReturn(expression);
            CommandCreationRoutine(result);

            return result;
        }
        // Thread command
        public override object VisitThreadCommand([NotNull] OALParser.ThreadCommandContext context)
        {
            if (context.ChildCount != 4 || !"thread".Equals(context.GetChild(0).GetText()) || !"end thread".Equals(context.GetChild(2).GetText()))
            {
                HandleError("Malformed thread.", context);
            }

            object commands = Visit(context.commands());

            if (commands is not List<EXECommand> || commands == null)
            {
                HandleError(string.Format("Commands in thread is '{0}' instead of a  of commands.", commands?.GetType().Name ?? "NULL"), context);
            }

            EXEScope result = new EXEScope();
            CommandCreationRoutine(result);
            (commands as List<EXECommand>).ForEach(command => result.AddCommand(command));

            return result;
        }
        public override object VisitTypeName([NotNull] OALParser.TypeNameContext context)
        {
            if (context.ChildCount < 1) { HandleError("Malformed type name.", context); }
            if (context.className() == null) { HandleError("Malformed type name.", context); }

            object className = Visit(context.className());

            if (className is not string || string.IsNullOrEmpty(className as string))
            {
                HandleError("Malformed type name - className should be non-empty string.", context);
            }

            int arrayDepthCount = context.arrayType() == null ? 0 : context.arrayType().Length;

            string result = className + (arrayDepthCount > 0 ? string.Concat(Enumerable.Repeat("[]", arrayDepthCount)) : string.Empty);

            return result;
        }
        // Variable name
        public override object VisitVariableName([NotNull] OALParser.VariableNameContext context)
        {
            string name = context.GetChild(0).GetText();

            if (context.ChildCount != 1 || !EXETypes.IsValidVariableName(name))
            {
                HandleError("Malformed variable name.", context);
            }

            return name;
        }
        // WHILE loop
        public override object VisitWhileCommand([NotNull] OALParser.WhileCommandContext context)
        {
            if (context.ChildCount != 5 || !"while".Equals(context.GetChild(0).GetText()) || !"end while".Equals(context.GetChild(3).GetText()))
            {
                HandleError("Malformed while.", context);
            }

            object condition = Visit(context.GetChild(1));

            if (condition is not EXEASTNodeBase || condition == null)
            {
                HandleError(string.Format("Condition in whileis '{0}' instead of an expression.", condition?.GetType().Name ?? "NULL"), context);
            }

            object commands = Visit(context.GetChild(2));

            if (commands is not List<EXECommand> || commands == null)
            {
                HandleError(string.Format("Commands in while is '{0}' instead of a list commands.", commands?.GetType().Name ?? "NULL"), context);
            }

            EXEScopeLoopWhile result = new EXEScopeLoopWhile(condition as EXEASTNodeBase);
            CommandCreationRoutine(result);
            (commands as List<EXECommand>).ForEach(command => result.AddCommand(command));

            return result;
        }
        // Visit terminal node - most likely a primitive literal or variable/attribute name
        public override object VisitTerminal(ITerminalNode node)
        {
            return new EXEASTNodeLeaf(node.GetText());
        }

        private void CommandCreationRoutine(EXECommand command)
        {
            command.IsDirectlyInCode = true;
        }
    }
}