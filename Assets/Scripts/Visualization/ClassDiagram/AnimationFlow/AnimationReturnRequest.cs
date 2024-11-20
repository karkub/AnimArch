using System.Collections;
using OALProgramControl;
using UnityEngine;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.ComponentsInDiagram;

namespace Visualization.Animation
{
    public class AnimationReturnRequest : AnimationRequest
    {
        public AnimationReturnRequest(EXECommand command, AnimationThread thread, bool animate, bool animateNewObjects) : base(command, thread, animate, animateNewObjects)
        {
            EXECommandReturn exeCommandReturn = (EXECommandReturn)command;

            if (animate)
            {
                EXEScopeMethod exeScopeMethod = exeCommandReturn.GetCurrentMethodScope();

                if (exeScopeMethod != null)
                {
                    CDMethod calledMethod = exeScopeMethod.MethodDefinition;
                    EXEScopeMethod exeScopeCaller = thread.CurrentMethod;
                    CDMethod callerMethod = exeScopeCaller?.MethodDefinition;

                    if
                    (
                        exeScopeCaller != null && callerMethod != null &&
                        exeScopeCaller.OwningObject != null && exeScopeCaller.OwningObject is EXEValueReference &&
                        exeScopeMethod.OwningObject != null && exeScopeMethod.OwningObject is EXEValueReference
                    )
                    {
                        CDClass caller = callerMethod.OwningClass;
                        CDClass called = calledMethod.OwningClass;
                        CDRelationship relation = animation.CurrentProgramInstance.RelationshipSpace.GetRelationshipByClasses(caller.Name, called.Name);

                        CDClassInstance callerInstance = (exeScopeCaller.OwningObject as EXEValueReference).ClassInstance;
                        CDClassInstance calledInstance = (exeScopeMethod.OwningObject as EXEValueReference).ClassInstance;

                        callInfo = new MethodInvocationInfo(callerMethod, calledMethod, relation, callerInstance, calledInstance);
                    }
                    else if
                    (
                        exeScopeCaller == null &&
                        exeScopeMethod.OwningObject != null && exeScopeMethod.OwningObject is EXEValueReference
                    )
                    {
                        CDClassInstance calledInstance = (exeScopeMethod.OwningObject as EXEValueReference).ClassInstance;
                        callInfo = MethodInvocationInfo.CreateCalledOnlyInstance(calledMethod, calledInstance);
                    }
                }
            }
        }

        public override IEnumerator PerformRequest()
        {
            yield return UnhighlightClassAndObject();
            AnimateActivityDiagram();

            Done = true;
        }

        private IEnumerator UnhighlightClassAndObject()
        {
            float timeModifier = 1f;
            Animation a = Animation.Instance;

            Class called = a.classDiagram.FindClassByName(callInfo.CalledMethod.OwningClass.Name).ParsedClass;
            Method calledMethod = a.classDiagram.FindMethodByName(callInfo.CalledMethod.OwningClass.Name, callInfo.CalledMethod.Name);
            RelationInDiagram relation = a.classDiagram.FindEdgeInfo(callInfo.Relation?.RelationshipName);
            Animation.assignCallInfoToAllHighlightSubjects(called, calledMethod, relation, callInfo, callInfo.CalledMethod);

            if (relation != null)
            {
                yield return new WaitUntil(() => relation.HighlightSubject.finishedFlag.IsDrawingFinished());
            }

            calledMethod.HighlightSubject.DecrementHighlightLevel();
            calledMethod.HighlightObjectSubject.DecrementHighlightLevel();
            called.HighlightSubject.DecrementHighlightLevel();
            relation?.HighlightSubject.DecrementHighlightLevel();

            if (a.standardPlayMode)
            {
                yield return new WaitForSeconds(AnimationData.Instance.AnimSpeed * timeModifier);
            }
        }
        private void AnimateActivityDiagram()
        {
            animation.activityDiagram.AddFinalActivityInDiagram();
            animation.isEXECommandReturn = true;
        }
    }
}