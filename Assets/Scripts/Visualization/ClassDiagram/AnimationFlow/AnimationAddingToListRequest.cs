using System.Collections;
using OALProgramControl;
using UnityEngine;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.ComponentsInDiagram;
using Visualization.UI;

namespace Visualization.Animation
{
    public class AnimationAddingToListRequest : AnimationRequest
    {

        public AnimationAddingToListRequest(EXECommand command, AnimationThread thread, bool animate, bool animateNewObjects) : base(command, thread, animate, animateNewObjects)
        {
        }

        public override IEnumerator PerformRequest()
        {
            EXECommandAddingToList addingToList = (EXECommandAddingToList)command;
            CDClassInstance listOwnerInstance = addingToList.GetAssignmentTargetOwner();
            CDClassInstance appendedInstance = addingToList.GetAppendedElementInstance();

            if (listOwnerInstance != null)
            {
                if (appendedInstance != null)
                {
                    animation.objectDiagram.AddRelation(listOwnerInstance, appendedInstance, "ASSOCIATION");
                }

                animation.objectDiagram.UpdateAttributeValues(listOwnerInstance);
            }

            Done = true;
            yield return new WaitForFixedUpdate();
        }
    }
}