using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OALProgramControl;

namespace Assets.UnitTests.AnimationControl
{
    public class VariableAsserter 
    {
        private List<EXEVariable> ExpectedVariables;
        private EXEScope ActualScope;

        public VariableAsserter()
        {
            this.ExpectedVariables = new List<EXEVariable>();
            this.ActualScope = null;
        }

        public VariableAsserter ExpectVariable(string name, EXEValueBase value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            ExpectVariable(new EXEVariable(name, value));

            return this;
        }

        public VariableAsserter ExpectVariable(EXEVariable variable)
        {
            if (variable == null)
            {
                throw new ArgumentNullException("variable");
            }

            if (variable.Value == null)
            {
                throw new ArgumentNullException("variable.Value");
            }

            if (string.IsNullOrEmpty(variable.Name))
            {
                throw new ArgumentNullException("variable.Name");
            }

            if (ExpectedVariables.Select(variable => variable.Name).Contains(variable.Name))
            {
                throw new Exception("[INVALID TEST] This test tried to expect several variables with the same name.");
            }

            this.ExpectedVariables.Add(variable);

            return this;
        }

        public void Declare(EXEScope actualScope)
        {
            if (actualScope == null)
            {
                throw new ArgumentNullException("actualScope");
            }

            this.ActualScope = actualScope;
        }

        public void PerformAssertion()
        {
            string variableDumpMessage = VariableDumpMessage();

            Assert.AreEqual(this.ExpectedVariables.Count, this.ActualScope.Variables.Count, "Wrong count of variables set.\n" + variableDumpMessage);

            EXEVariable actualVariable;
            this.ExpectedVariables
                .ForEach
                (
                    expectedVariable =>
                    {
                        Assert.IsTrue(this.ActualScope.VariableExists(expectedVariable.Name), string.Format("The expected variable '{0}' is not set.\n{1}", expectedVariable.Name, variableDumpMessage));

                        actualVariable = this.ActualScope.FindVariable(expectedVariable.Name);

                        if (actualVariable.Value is not EXEValueReference)
                        {
                            VisitorCommandToString visitor = new VisitorCommandToString();
                            expectedVariable.Value.Accept(visitor);
                            VisitorCommandToString visitor2 = new VisitorCommandToString();
                            actualVariable.Value.Accept(visitor2);
                            Assert.AreEqual(visitor.GetCommandString(), visitor2.GetCommandString(), string.Format("The expected variable '{0}' has invalid value.\n{1}", expectedVariable.Name, variableDumpMessage));
                        }
                        else
                        {
                            Assert.AreEqual
                            (
                                (expectedVariable.Value as EXEValueReference).ClassInstance.UniqueID,
                                (actualVariable.Value as EXEValueReference).ClassInstance.UniqueID,
                                string.Format("The expected variable '{0}' references instance with ID '{1}'. It should reference '{2}'.", expectedVariable.Name, (actualVariable.Value as EXEValueReference).ClassInstance.UniqueID, (expectedVariable.Value as EXEValueReference).ClassInstance.UniqueID)
                            );
                        }
                    }
                );
        }

        private string VariableDumpMessage()
        {
            StringBuilder messageBuilder = new StringBuilder();

            messageBuilder.Append("Currently existing variables are: ");
            VariablesMessage(messageBuilder, this.ActualScope.Variables);

            messageBuilder.Append("Variables expected to exist are: ");
            VariablesMessage(messageBuilder, this.ExpectedVariables);

            return messageBuilder.ToString();
        }

        private void VariablesMessage(StringBuilder messageBuilder, List<EXEVariable> variables)
        {
            messageBuilder
                .AppendJoin(", ", variables.Select(variable => variable.Name))
                .AppendLine();
        }
    }
}