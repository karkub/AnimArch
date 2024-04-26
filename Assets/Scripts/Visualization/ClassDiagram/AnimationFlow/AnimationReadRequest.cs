using System.Collections;
using OALProgramControl;
using UnityEngine;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.ComponentsInDiagram;
using Visualization.UI;

namespace Visualization.Animation
{
    public class AnimationReadRequest : AnimationRequest
    {

        public AnimationReadRequest(EXECommand command, AnimationThread thread, bool animate, bool animateNewObjects) : base(command, thread, animate, animateNewObjects)
        {
        }

        public override IEnumerator PerformRequest()
        {
           
             EXECommandRead readCommand = command as EXECommandRead;

            ConsoleRequestRead consoleRequest = new ConsoleRequestRead(readCommand.PromptText);
            animation.consoleScheduler.Enqueue(consoleRequest);
            yield return new WaitUntil(() => consoleRequest.Done);

            thread.ExecutionSuccess
                = ((EXECommandRead)command).AssignReadValue(consoleRequest.ReadValue, animation.CurrentProgramInstance);

            Done = true;
            yield return new WaitForFixedUpdate();
    
        }
    }
}