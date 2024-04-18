using UMSAGL.Scripts;
using UnityEngine;
using Visualization.ClassDiagram.ComponentsInDiagram;

namespace Visualization.ClassDiagram.Relations
{
    public class ObjectRelation
    {
        private readonly Graph _graph;
        public readonly long startUniqueId;
        public readonly long endUniqueId;
        private readonly string _type;
        private readonly ObjectInDiagram _start;
        private readonly ObjectInDiagram _end;
        private readonly string _relationName;
        public GameObject GameObject;
        public ObjectRelation(Graph graph, long start, long end, string type, string relationName)
        {
            _graph = graph;
            startUniqueId = start;
            endUniqueId = end;
            _type = type;
            _relationName = relationName;
            
            _start = DiagramPool.Instance.ObjectDiagram.FindByID(startUniqueId);
            _end = DiagramPool.Instance.ObjectDiagram.FindByID(endUniqueId);
        }

        public void Generate()
        {
            GameObject = InitEdge();
        }

        private GameObject InitEdge()
        {
            return _graph.AddEdge(_start.VisualObject, _end.VisualObject,
                DiagramPool.Instance.associationNonePrefab);
        }

        public bool Equals(ObjectRelation other)
        {
            if (startUniqueId == other.startUniqueId &&
                endUniqueId == other.endUniqueId)
            {
                return true;
            }

            return false;
        }
    }
}