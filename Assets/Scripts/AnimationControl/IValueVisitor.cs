using OALProgramControl;
using UnityEditor;
using UnityEngine;


public interface IValueVisitor
{
    void VisitExeValueArray(EXEValueArray value);
    void VisitExeValueBool(EXEValueBool value);
    void VisitExeValueInt(EXEValueInt value);
    void VisitExeValueReal(EXEValueReal value);
    void VisitExeValueReference(EXEValueReference value);
    void VisitExeValueString(EXEValueString value);
}