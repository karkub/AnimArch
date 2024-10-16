namespace OALProgramControl
{
    public class EXECommandContinue : EXECommand
    {
        protected override EXEExecutionResult Execute(OALProgram OALProgram)
        {
            EXEScopeLoop currentLoop = this.SuperScope.GetCurrentLoopScope();

            if (currentLoop == null)
            {
                return Error("XEC2021", "Tried to continue in something that is not a loop.");
            }

            bool success = false;

            while (this.CommandStack.HasNext())
            {
                EXECommand peek = this.CommandStack.Next();

                if (peek == currentLoop)
                {
                    this.CommandStack.Enqueue(peek);
                    success = true;
                    break;
                }
            }

            if (!success)
            {
                return Error("XEC2022", "Failed to find the current loop to continue.");
            }

            return Success();
        }
        public override void Accept(Visitor v) {
            v.VisitExeCommandContinue(this);
        }

        protected override EXECommand CreateCloneCustom()
        {
            return new EXECommandContinue();
        }
    }
}
