using System;
using System.Collections.Generic;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEngine;
using Visualisation.Animation;

namespace Visualization.ClassDiagram
{

public class EdgeHighlightObserver : HighlightObserver
{
        public EdgeHighlightObserver(HighlightSubject subject) : base(subject)
        {
        }

        public override void Update()
        {
            EdgeHighlightSubject s = Subject as EdgeHighlightSubject;
            Animation.Animation a = Animation.Animation.Instance;

            if (s.HighlightInt == 1)
            {
                a.edgeHighlighter.Highligt(s.InvocationInfo);
            }
            else if (s.HighlightInt == 0)
            {
                a.HighlightEdge(s.InvocationInfo.Relation?.RelationshipName, false, s.InvocationInfo);
            }
            
        }

}



}
