using System.Collections;
using System.Collections.Generic;
using OALProgramControl;
using UnityEngine;

public abstract class Visitor : IValueVisitor
{
    public abstract void VisitExeCommandCall(EXECommandCall command);
    public abstract void VisitExeCommandBreak(EXECommandBreak command);
    public abstract void VisitExeCommandContinue(EXECommandContinue command);
    public abstract void VisitExeCommandAddingToList(EXECommandAddingToList command);
    public abstract void VisitExeCommandAssignment(EXECommandAssignment command);
    public abstract void VisitExeCommandCreateList(EXECommandCreateList command);
    public abstract void VisitExeCommandFileAppend(EXECommandFileAppend command);
    public abstract void VisitExeCommandFileExists(EXECommandFileExists command);
    public abstract void VisitExeCommandFileRead(EXECommandFileRead command);
    public abstract void VisitExeCommandFileWrite(EXECommandFileWrite command);
    public virtual void VisitExeCommandPragma(EXECommandPragma command) { }
    public abstract void VisitExeCommandQueryCreate(EXECommandQueryCreate command);
    public abstract void VisitExeCommandQueryDelete(EXECommandQueryDelete command);
    public abstract void VisitExeCommandRead(EXECommandRead command);
    public abstract void VisitExeCommandRemovingFromList(EXECommandRemovingFromList command);
    public abstract void VisitExeCommandReturn(EXECommandReturn command);
    public abstract void VisitExeCommandWait(EXECommandWait command);
    public abstract void VisitExeCommandWrite(EXECommandWrite command);
    public abstract void VisitExeCommand(EXECommand command);

    public abstract void VisitExeScope(EXEScope scope);
    public abstract void VisitExeScopeLoop(EXEScopeLoop scope);
    public abstract void VisitExeScopeMethod(EXEScopeMethod scope);
    public abstract void VisitExeScopeForEach(EXEScopeForEach scope);
    public abstract void VisitExeScopeParallel(EXEScopeParallel scope);
    public abstract void VisitExeScopeCondition(EXEScopeCondition scope);
    public abstract void VisitExeScopeLoopWhile(EXEScopeLoopWhile scope);

    public abstract void VisitExeASTNodeAccesChain(EXEASTNodeAccessChain node);

    public abstract void VisitExeASTNodeComposite(EXEASTNodeComposite node);
    public abstract void VisitExeASTNodeLeaf(EXEASTNodeLeaf node);
    public abstract void VisitExeASTNodeMethodCall(EXEASTNodeMethodCall node);
    public abstract void VisitExeASTNodeIndexation(EXEASTNodeIndexation node);

    public abstract void VisitExeValueArray(EXEValueArray value);
    public abstract void VisitExeValueBool(EXEValueBool value);
    public abstract void VisitExeValueInt(EXEValueInt value);
    public abstract void VisitExeValueReal(EXEValueReal value);
    public abstract void VisitExeValueReference(EXEValueReference value);
    public abstract void VisitExeValueString(EXEValueString value);
}
