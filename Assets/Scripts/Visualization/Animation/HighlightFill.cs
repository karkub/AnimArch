using System;
using System.Collections.Generic;
using OALProgramControl;
using UnityEngine;
using Visualization.ClassDiagram.ComponentsInDiagram;

namespace Visualization.ClassDiagram
{

public class HighlightFill : HighlightEdgeState
{
    protected static HighlightFill instance = null;
    public static HighlightFill GetInstance()
    {
        if (instance == null)
        {
            instance = new HighlightFill();
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
