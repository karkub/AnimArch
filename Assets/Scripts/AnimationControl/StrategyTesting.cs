using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace OALProgramControl
{
    public class StrategyTesting : IStrategy
    {
        public List<string> ConsoleHistory { get; }
        public List<String> MockedInputs { get; set; }
        public StrategyTesting()
        {   
            this.ConsoleHistory = new List<string>();
            this.MockedInputs = new List<String>();
        }

        public void Read(EXECommandRead CurrentCommand, OALProgram CurrentProgramInstance)
        {
            if(MockedInputs.Count <= 0)
            {
                throw new Exception("No more inputs to read.");
            }
            String MockedInput = MockedInputs[0];
            MockedInputs.RemoveAt(0);
            CurrentCommand.AssignReadValue(MockedInput, CurrentProgramInstance);
            ConsoleHistory.Add(CurrentCommand.PromptText);
            ConsoleHistory.Add(MockedInput);
        }   

        public void Write(string text)
        {
            ConsoleHistory.Add(text);
        }

    }
}