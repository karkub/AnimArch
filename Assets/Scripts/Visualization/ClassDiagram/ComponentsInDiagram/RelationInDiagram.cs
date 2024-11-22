using OALProgramControl;
using UnityEngine;
using Visualization.ClassDiagram.Relations;

namespace Visualization.ClassDiagram.ComponentsInDiagram
{
    public class RelationInDiagram
    {
        public Relation ParsedRelation;
        public CDRelationship RelationInfo;
        public GameObject VisualObject;
        public EdgeHighlightSubject HighlightSubject { get; set;}
    }
}
