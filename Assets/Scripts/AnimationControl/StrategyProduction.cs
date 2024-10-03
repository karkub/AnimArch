using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace OALProgramControl
{
    public class StrategyProduction : IStrategy
    {
        public List<string> ConsoleHistory { get; }
        public List<String> MockedInputs { get; set; }
        public StrategyProduction()
        {
            this.ConsoleHistory = new List<string>();
            this.MockedInputs = new List<String>();
        }

        public void Read(EXECommandRead CurrentCommand, OALProgram CurrentProgramInstance)
        {
        }      
        
        public void Write(string text)
        {
        }

    }
}