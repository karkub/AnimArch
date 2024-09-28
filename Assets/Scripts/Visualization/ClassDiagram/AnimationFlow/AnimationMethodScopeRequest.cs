using OALProgramControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Visualization.Animation
{
    public class AnimationMethodScopeRequest : AnimationRequest
    {
        public AnimationMethodScopeRequest(EXECommand command, AnimationThread thread, bool animate, bool animateNewObjects) : base(command, thread, animate, animateNewObjects)
        {
        }

        public override IEnumerator PerformRequest()
        {
            Debug.LogError("EXEScopeMethod activityDiagram.ClearDiagram()");
            animation.activityDiagram.ClearDiagram();

            Done = true;
            yield return null;
        }
    }
}