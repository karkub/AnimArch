using System;
using System.Collections.Generic;
using OALProgramControl;
using UnityEngine;
using Visualization.ClassDiagram.ComponentsInDiagram;

namespace Visualization.ClassDiagram
{

public class HighlightFillState : HighlightEdgeState
{
    protected static HighlightFillState instance = null;
    public static HighlightFillState GetInstance()
    {
        if (instance == null)
        {
            instance = new HighlightFillState();
        }
        return instance;
    }
    public override void Highligt(MethodInvocationInfo Call)
    {
        Animation.Animation a = Animation.Animation.Instance;
        RelationInDiagram relation = a.classDiagram.FindEdgeInfo(Call.Relation?.RelationshipName);
        relation.HighlightSubject.finishedFlag.InitWaitingFlag();
        a.RunAnimateFill(Call);
    }

}

}
