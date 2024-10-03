using OALProgramControl;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Visualization.UI;

namespace Assets.UnitTests.AnimationControl
{
    public class CommandTest
    {
        public readonly VariableAsserter Variables;
        public readonly ObjectInstanceAsserter ObjectInstances;
        public readonly ConsoleHistoryAsserter ConsoleHistory;
        private EXEExecutionResult ActualExecutionResult;

        public CommandTest()
        {
            this.Variables = new VariableAsserter();
            this.ObjectInstances = new ObjectInstanceAsserter();
            this.ConsoleHistory = new ConsoleHistoryAsserter();
        }

        public void Declare(EXEScope actualScope, EXEExecutionResult executionResult)
        {
            this.Variables.Declare(actualScope);
            this.ObjectInstances.Declare();
            this.ConsoleHistory.Declare(MenuManager.Instance.Strategy.ConsoleHistory);
            this.ActualExecutionResult = executionResult;
        }

        public void PerformAssertion()
        {
            Assert.IsTrue(this.ActualExecutionResult.IsSuccess, "There was an execution error.\n" + this.ActualExecutionResult.ToString());
            Assert.IsTrue(this.ActualExecutionResult.IsDone, "The execution did not finish.");

            this.Variables.PerformAssertion();
            this.ObjectInstances.PerformAssertion();
            this.ConsoleHistory.PerformAssertion();
        }
    }
}
