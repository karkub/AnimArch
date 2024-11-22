
using OALProgramControl;
using Visualization.UI;

namespace Visualization.Animation
{
    public static class AnimationRequestFactory
    {
        public static AnimationRequest Create(EXECommand command, AnimationThread thread, bool animate, bool animateNewObjects)
        {
            if (command.GetType() == typeof(EXECommandCall)) {
                return new AnimationCallFunctionRequest(command, thread, animate, animateNewObjects);
            }
            else if (command.GetType() == typeof(EXECommandReturn))
            {
                return new AnimationReturnRequest(command, thread, animate, animateNewObjects);
            }
            else if (command.GetType() == typeof(EXECommandQueryCreate))
            {
                return new AnimationCreateObjectRequest(command, thread, animate, animateNewObjects);
            }
            else if (command.GetType() == typeof(EXECommandAssignment))
            {
                return new AnimationAssignmentRequest(command, thread, animate, animateNewObjects);
            }
            else if (command.GetType() == typeof(EXECommandAddingToList))
            {
                return new AnimationAddingToListRequest(command, thread, animate, animateNewObjects);
            }
            else if (command.GetType() == typeof(EXECommandRead))
            {
                return new AnimationReadRequest(command, thread, animate, animateNewObjects);
            }
            else if (command.GetType() == typeof(EXECommandWrite))
            {
                return new AnimationWriteRequest(command, thread, animate, animateNewObjects);
            }
            else if (command.GetType() == typeof(EXECommandWait))
            {
                return new AnimationWaitRequest(command, thread, animate, animateNewObjects);
            }
            else if (command.GetType() == typeof(EXEScopeMethod))
            {
                return new AnimationMethodScopeRequest(command, thread, animate, animateNewObjects);
            }
            return new AnimationNullRequest(command, thread, animate, animateNewObjects);
        }
    }
}
