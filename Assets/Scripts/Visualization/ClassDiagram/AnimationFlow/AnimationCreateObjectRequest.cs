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
            animation.BarrierSize = 1;
            animation.CurrentBarrierFill = 0;

            if (animate)
            {
                animation.StartCoroutine(animation.BarrierFillCheck());
            }
        }

        public override IEnumerator PerformRequest()
        {

            EXECommandQueryCreate createCommand = (EXECommandQueryCreate)command;

            CDClassInstance callerObject = (command.GetCurrentMethodScope().OwningObject as EXEValueReference).ClassInstance;
            CDClassInstance createdObject = createCommand.GetCreatedInstance();

            string targetVariableName = null;
            if (createCommand.AssignmentTarget != null)
            {
                VisitorCommandToString visitor = VisitorCommandToString.BorrowAVisitor();
                createCommand.AssignmentTarget.Accept(visitor);
                targetVariableName = visitor.GetCommandStringAndResetStateNow();
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
                    #region Object creation animation

                    int step = 0;
                    float speedPerAnim = AnimationData.Instance.AnimSpeed;
                    float timeModifier = 1f;
                    IEnumerable<RelationInDiagram> relationsOfClass = animation.classDiagram.FindRelationsByClass(createdObject.OwningClass.Name);

                    foreach (RelationInDiagram rel in relationsOfClass)
                    {
                        yield return new WaitUntil(() => rel.HighlightSubject.finishedFlag.IsDrawingFinished());
                    }
                    Class highlightedClass = animation.classDiagram.FindClassByName(createdObject.OwningClass.Name).ParsedClass;
                    highlightedClass.HighlightSubject.ClassName = highlightedClass.Name;
                    while (step < 7)
                    {
                        switch (step)
                        {
                            case 0:
                                highlightedClass.HighlightSubject.IncrementHighlightLevel();
                                break;
                            case 1:
                                // yield return StartCoroutine(AnimateFillInterGraph(relation));
                                timeModifier = 0f;
                                break;
                            case 3:
                                // relation.Show(); // TODO
                                // relation.Highlight();
                                timeModifier = 1f;
                                break;
                            case 2:
                                animation.objectDiagram.ShowObject(objectInDiagram);
                                timeModifier = 0.5f;
                                break;
                            case 6:
                                highlightedClass.HighlightSubject.DecrementHighlightLevel();
                                // relation.UnHighlight();
                                timeModifier = 1f;
                                break;
                        }

                        step++;
                        yield return new WaitForSeconds(AnimationData.Instance.AnimSpeed * timeModifier);
                    }

                    animation.objectDiagram.AddRelation(callerObject, createdObject, "ASSOCIATION");

                    step = 0;
                    while (step < 7)
                    {
                        step++;
                        if (!animation.standardPlayMode)
                        {
                            if (step == 1) step = 2;
                            animation.nextStep = false;
                            animation.prevStep = false;
                            yield return new WaitUntil(() => animation.nextStep);
                            if (animation.prevStep)
                            {
                                if (step > 0) step--;
                                step = animation.UnhighlightObjectCreationStepAnimation(step, createdObject.OwningClass.Name, objectInDiagram);

                                if (step > -1) step--;
                                step = animation.UnhighlightObjectCreationStepAnimation(step, createdObject.OwningClass.Name, objectInDiagram);
                            }

                            yield return new WaitForFixedUpdate();
                            animation.nextStep = false;
                            animation.prevStep = false;
                        }
                    }

                    #endregion
                }
            }
            else
            {
                animation.AddObjectToDiagram(createdObject, targetVariableName, false);
            }

            animation.IncrementBarrier();
            Done = true;
        }
    }
}