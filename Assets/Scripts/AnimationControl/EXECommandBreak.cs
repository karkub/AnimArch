namespace OALProgramControl
{
    public class EXECommandBreak : EXECommand
    {
        protected override EXEExecutionResult Execute(OALProgram OALProgram)
        {
            EXEScopeLoop currentLoop = this.SuperScope.GetCurrentLoopScope();

            if (currentLoop == null)
            {
                return Error("XEC2019", "Tried to break out of something that is not a loop.");
            }

            bool success = false;

            while (this.CommandStack.HasNext())
            {
                EXECommand peek = this.CommandStack.Next();

                if (peek == currentLoop)
                {
                    success = true;
                    break;
                }
            }

            if (!success)
            {
                return Error("XEC2020", "Failed to find the current loop to break out from.");
            }

            return Success();
        }
        public override void Accept(Visitor v) {
            v.VisitExeCommandBreak(this);
        }

        protected override EXECommand CreateCloneCustom()
        {
            return new EXECommandBreak();
        }
    }
}
