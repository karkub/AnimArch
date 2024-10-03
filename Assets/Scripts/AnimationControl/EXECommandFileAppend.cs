using UnityEditor;
using UnityEngine;

namespace OALProgramControl
{
    public class EXECommandFileAppend : EXECommandFileModify
    {
        public override bool Append => true;

        public EXECommandFileAppend(EXEASTNodeBase stringToWrite, EXEASTNodeBase fileToWriteTo) : base(stringToWrite, fileToWriteTo) { }

        public override EXECommand CreateClone()
        {
            return new EXECommandFileAppend(StringToWrite.Clone(), FileToWriteTo.Clone());
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeCommandFileAppend(this);
        }
    }
}