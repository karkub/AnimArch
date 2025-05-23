﻿using System.IO;
using UnityEditor;
using UnityEngine;

namespace OALProgramControl
{
    public class EXECommandFileWrite : EXECommandFileModify
    {
        public override bool Append => false;

        public EXECommandFileWrite(EXEASTNodeBase stringToWrite, EXEASTNodeBase fileToWriteTo) : base(stringToWrite, fileToWriteTo) { }

        protected override EXECommand CreateCloneCustom()
        {
            return new EXECommandFileWrite(StringToWrite.Clone(), FileToWriteTo.Clone());
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeCommandFileWrite(this);
        }
    }
}