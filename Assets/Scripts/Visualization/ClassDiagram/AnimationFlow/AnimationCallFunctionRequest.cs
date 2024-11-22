using System.Collections;
using OALProgramControl;
using UnityEngine;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.ComponentsInDiagram;
using Visualization.UI;

namespace Visualization.Animation
{
    public class AnimationCallFunctionRequest : AnimationRequest
    {
        private const float ANIM_SPEED_QUANTIFIER = 1.25f;

        public AnimationCallFunctionRequest(EXECommand command, AnimationThread thread, bool animate, bool animateNewObjects) : base(command, thread, animate, animateNewObjects)
        {
            EXECommandCall exeCommandCall = (EXECommandCall)command;

            if (animate)
            {
                MethodInvocationInfo methodCallInfo = exeCommandCall.CallInfo;
                callInfo = methodCallInfo;

                if (methodCallInfo != null)
                {
                    animation.objectDiagram.AddRelation(methodCallInfo.CallerObject, methodCallInfo.CalledObject, "ASSOCIATION");
                }
            }
        }

        public override IEnumerator PerformRequest()
        {
            ClassDiagram.Diagrams.ClassDiagram classDiagram = Animation.Instance.classDiagram;
            Class called = classDiagram.FindClassByName(callInfo.CalledMethod.OwningClass.Name).ParsedClass;
            Method calledMethod = classDiagram.FindMethodByName(callInfo.CalledMethod.OwningClass.Name, callInfo.CalledMethod.Name);
            RelationInDiagram relation = classDiagram.FindEdgeInfo(callInfo.Relation?.RelationshipName);

            Animation.assignCallInfoToAllHighlightSubjects(called, calledMethod, relation, callInfo, callInfo.CalledMethod);

            if (relation != null)
            {
                yield return new WaitUntil(() => relation.HighlightSubject.finishedFlag.IsDrawingFinished());
                relation?.HighlightSubject.IncrementHighlightLevel();
                yield return new WaitUntil(() => relation.HighlightSubject.finishedFlag.IsDrawingFinished());
            }
            calledMethod.HighlightObjectSubject.IncrementHighlightLevel();
            called.HighlightSubject.IncrementHighlightLevel();
            calledMethod.HighlightSubject.IncrementHighlightLevel();
            yield return new WaitForSeconds(AnimationData.Instance.AnimSpeed * ANIM_SPEED_QUANTIFIER);

            Done = true;
        }
    }
}