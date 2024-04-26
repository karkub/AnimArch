using System;
using System.Collections;
using OALProgramControl;
using UnityEngine;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.ComponentsInDiagram;
using Visualization.UI;

namespace Visualization.Animation
{
    public class AnimationTerminateRequest : AnimationRequest
    {
        private Func<bool> terminateThread;

        public AnimationTerminateRequest(EXECommand command, AnimationThread thread, bool animate, bool animateObjects, Func<bool> terminateThread) : base(command, thread, animate, animateObjects)
        {
            this.terminateThread = terminateThread;
        }

        public override IEnumerator PerformRequest()
        {
            terminateThread();
            yield return new WaitForFixedUpdate();
        }
    }
}