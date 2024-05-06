using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace Assets.UnitTests.AnimationControl
{
    public class ConsoleHistoryAsserter
    {
        private List<string> ExpectedHistory;
        public List<string> ActualHistory;

        public ConsoleHistoryAsserter()
        {
            this.ExpectedHistory = new List<string>();
            this.ActualHistory = new List<string>();
        }

        public ConsoleHistoryAsserter ExpectText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException("text");
            }

            this.ExpectedHistory.Add(text);

            return this;
        }

        public void Declare(List<string> actualHistory)
        {
            if (actualHistory == null)
            {
                throw new ArgumentNullException("actualHistory");
            }

            this.ActualHistory = actualHistory;
        }

        public void PerformAssertion()
        {
            string consoleHistoryDumpMessage = ConsoleHistoryDumpMessage();

            Assert.AreEqual(this.ExpectedHistory.Count, this.ActualHistory.Count, string.Format("The number of strings in console is not as expected.\n{0}", consoleHistoryDumpMessage));

            for (int index=0; index < this.ExpectedHistory.Count; index++)
            {
                Assert.AreEqual(this.ExpectedHistory[index], this.ActualHistory[index], string.Format("The expected string '{0}' is not written in console.\n{1}", this.ExpectedHistory[index], consoleHistoryDumpMessage));
            }
        }

        private string ConsoleHistoryDumpMessage()
        {
            StringBuilder messageBuilder = new StringBuilder();

            messageBuilder.Append("Strings currently in console are: ");
            ConsoleHistoryMessage(messageBuilder, this.ActualHistory);

            messageBuilder.Append("Strings expected to be in console are: ");
            ConsoleHistoryMessage(messageBuilder, this.ExpectedHistory);

            return messageBuilder.ToString();
        }

        private void ConsoleHistoryMessage(StringBuilder messageBuilder, List<string> consoleStrings)
        {
            messageBuilder
                .AppendJoin(", ", consoleStrings)
                .AppendLine();
        }
    }
}