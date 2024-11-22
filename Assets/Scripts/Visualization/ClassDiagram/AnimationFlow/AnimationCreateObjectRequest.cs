using System.Collections;
using System.Collections.Generic;
using OALProgramControl;
using UnityEngine;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.ComponentsInDiagram;
using Visualization.UI;

namespace Visualization.Animation
{
    public class AnimationCreateObjectRequest : AnimationRequest
    {
        public AnimationCreateObjectRequest(EXECommand command, AnimationThread thread, bool animate, bool animateNewObjects) : base(command, thread, animate, animateNewObjects)
        {
        }

        public override IEnumerator PerformRequest()
        {

            EXECommandQueryCreate createCommand = (EXECommandQueryCreate)command;

            CDClassInstance callerObject = (command.GetCurrentMethodScope().OwningObject as EXEValueReference).ClassInstance;
            CDClassInstance createdObject = createCommand.GetCreatedInstance();

            string targetVariableName = null;
            if (createCommand.AssignmentTarget != null)
            {
                VisitorCommandToString visitor = new VisitorCommandToString();
                createCommand.AssignmentTarget.Accept(visitor);
                targetVariableName = visitor.GetCommandString();
            }

            if (animateNewObjects)
            {
                var objectInDiagram = animation.AddObjectToDiagram(createdObject, targetVariableName);
                var relation = Animation.FindInterGraphRelation(createdObject.UniqueID);

                if (!animate)
                {
                    animation.objectDiagram.ShowObject(objectInDiagram);
                    animation.objectDiagram.AddRelation(callerObject, createdObject, "ASSOCIATION");
                }
                else
                {
                    float speedPerAnim = AnimationData.Instance.AnimSpeed;
                    float timeModifier = 1.25f;
                    IEnumerable<RelationInDiagram> relationsOfClass = animation.classDiagram.FindRelationsByClass(createdObject.OwningClass.Name);

                    foreach (RelationInDiagram rel in relationsOfClass)
                    {
                        yield return new WaitUntil(() => rel.HighlightSubject.finishedFlag.IsDrawingFinished());
                    }
                    Class highlightedClass = animation.classDiagram.FindClassByName(createdObject.OwningClass.Name).ParsedClass;
                    highlightedClass.HighlightSubject.ClassName = highlightedClass.Name;

                    highlightedClass.HighlightSubject.IncrementHighlightLevel();
                    animation.objectDiagram.ShowObject(objectInDiagram);
                    yield return new WaitForSeconds(AnimationData.Instance.AnimSpeed * timeModifier);
                    highlightedClass.HighlightSubject.DecrementHighlightLevel();

                    animation.objectDiagram.AddRelation(callerObject, createdObject, "ASSOCIATION");
                }
            }
            else
            {
                animation.AddObjectToDiagram(createdObject, targetVariableName, false);
            }
            Done = true;
        }
    }
}