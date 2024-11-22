using System;
using System.Collections;
using OALProgramControl;
using UnityEngine;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.ComponentsInDiagram;
using Visualization.UI;

namespace Visualization.Animation
{
    public class AnimationWaitRequest : AnimationRequest
    {

        public AnimationWaitRequest(EXECommand command, AnimationThread thread, bool animate, bool animateNewObjects) : base(command, thread, animate, animateNewObjects)
        {
        }

        public override IEnumerator PerformRequest()
        {
           
            if (animate)
            {
                EXECommandWait waitCommand = command as EXECommandWait;

                float secondsToWait;
                if (waitCommand.WaitTime.EvaluationResult.ReturnedOutput is EXEValueReal)
                {
                    EXEValueReal secondsToWaitValue = waitCommand.WaitTime.EvaluationResult.ReturnedOutput as EXEValueReal;
                    secondsToWait = (float)secondsToWaitValue.Value;
                }
                else if (waitCommand.WaitTime.EvaluationResult.ReturnedOutput is EXEValueInt)
                {
                    EXEValueInt secondsToWaitValue = waitCommand.WaitTime.EvaluationResult.ReturnedOutput as EXEValueInt;
                    secondsToWait = (float)secondsToWaitValue.Value;
                }
                else
                {
                    throw new Exception(string.Format("Tried to wait for some seconds. The value type is {0}", waitCommand.WaitTime.EvaluationResult.ReturnedOutput.TypeName));
                }

                yield return new WaitForSeconds(secondsToWait);
            }

            Done = true;
            yield return new WaitForFixedUpdate();
    
        }
    }
}