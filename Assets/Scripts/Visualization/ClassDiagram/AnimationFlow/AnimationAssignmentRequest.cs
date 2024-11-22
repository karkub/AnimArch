using System.Collections;
using OALProgramControl;
using UnityEngine;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.ComponentsInDiagram;
using Visualization.UI;

namespace Visualization.Animation
{
    public class AnimationAssignmentRequest : AnimationRequest
    {

        public AnimationAssignmentRequest(EXECommand command, AnimationThread thread, bool animate, bool animateNewObjects) : base(command, thread, animate, animateNewObjects)
        {
        }

        public override IEnumerator PerformRequest()
        {
            EXECommandAssignment assignment = (EXECommandAssignment)command;
            CDClassInstance classInstance = assignment.GetAssignmentTargetOwner();
            if (classInstance != null)
            {
                animation.objectDiagram.UpdateAttributeValues(classInstance);
            }

            Done = true;
            yield return new WaitForFixedUpdate();
    
        }
    }
}