using System;
using System.Collections;
using OALProgramControl;
using UnityEngine;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.ComponentsInDiagram;
using Visualization.UI;

namespace Visualization.Animation
{
    public class AnimationNullRequest : AnimationRequest
    {

        public AnimationNullRequest(EXECommand command, AnimationThread thread, bool animate, bool animateNewObjects) : base(command, thread, animate, animateNewObjects)
        {
        }

        public override IEnumerator PerformRequest()
        {
           if (animate)
            {
                float speedPerAnim = AnimationData.Instance.AnimSpeed;
                yield return new WaitForSeconds(Animation.AnimationSpeedCoefficient * speedPerAnim);
            }

            Done = true;
            yield return new WaitForFixedUpdate();
    
        }
    }
}