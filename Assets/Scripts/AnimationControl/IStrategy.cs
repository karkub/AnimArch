using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace OALProgramControl
{
    public interface IStrategy
    {
        public List<string> ConsoleHistory { get; }
        public List<String> MockedInputs { get; set; }

        void Read(EXECommandRead CurrentCommand, OALProgram CurrentProgramInstance);      
        void Write(string text); 
    }
}