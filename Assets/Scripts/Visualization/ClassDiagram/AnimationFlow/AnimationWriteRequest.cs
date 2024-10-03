using System.Collections;
using OALProgramControl;
using UnityEngine;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.ComponentsInDiagram;
using Visualization.UI;

namespace Visualization.Animation
{
    public class AnimationWriteRequest : AnimationRequest
    {

        public AnimationWriteRequest(EXECommand command, AnimationThread thread, bool animate, bool animateNewObjects) : base(command, thread, animate, animateNewObjects)
        {
        }

        public override IEnumerator PerformRequest()
        {
            EXECommandWrite readCommand = command as EXECommandWrite;

            ConsoleRequestWrite consoleRequest = new ConsoleRequestWrite(readCommand.PromptText);
            animation.consoleScheduler.Enqueue(consoleRequest);
            yield return new WaitUntil(() => consoleRequest.Done);

            Done = true;
            yield return new WaitForFixedUpdate();
        }
    }
}