using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using OALProgramControl;

namespace Assets.UnitTests.AnimationControl
{
    public class ObjectInstanceAsserter
    {
        private List<Tuple<CDClassInstance, CDClassInstance>> ExpectedToActualInstances;

        public ObjectInstanceAsserter()
        {
            this.ExpectedToActualInstances = new List<Tuple<CDClassInstance, CDClassInstance>>();
        }

        public ObjectInstanceAsserter ExpectObjectInstance(CDClassInstance expectedClassInstance, CDClassInstance actualClassInstance)
        {
            if (expectedClassInstance == null)
            {
                throw new ArgumentNullException("classInstance");
            }

            if (actualClassInstance == null)
            {
                throw new ArgumentNullException("actualClassInstance");
            }

            this.ExpectedToActualInstances
                .Add(new Tuple<CDClassInstance, CDClassInstance>(expectedClassInstance, actualClassInstance));

            return this;
        }

        public void Declare()
        {
        }

        public void PerformAssertion()
        {
            foreach(Tuple<CDClassInstance, CDClassInstance> instancePair in this.ExpectedToActualInstances)
            {
                Assert.AreEqual(instancePair.Item1.OwningClass.Name, instancePair.Item2.OwningClass.Name, "Type mismatch of object instance pair.");

                foreach (string attributeName in instancePair.Item1.OwningClass.GetAttributes(true).Select(attribute => attribute.Name))
                {
                    if (instancePair.Item1.State[attributeName] is not EXEValueReference)
                    {
                        VisitorCommandToString visitor = new VisitorCommandToString();
                        instancePair.Item1.State[attributeName].Accept(visitor);
                        VisitorCommandToString visitor2 = new VisitorCommandToString();
                        instancePair.Item2.State[attributeName].Accept(visitor2);
                        Assert.AreEqual(visitor.GetCommandString(), visitor2.GetCommandString(), "Invalid value of attribute.");
                    }
                    else
                    {

                    }
                }
            }
        }
    }
}