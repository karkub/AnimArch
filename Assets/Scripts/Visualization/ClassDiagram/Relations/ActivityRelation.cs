using System;
using UMSAGL.Scripts;
using UnityEngine;
using Visualization.ClassDiagram.ComponentsInDiagram;

namespace Visualization.ClassDiagram.Relations
{
    public class ActivityRelation
    {
        private readonly Graph _graph;        
        private readonly String _type;
        private readonly ActivityInDiagram _start;
        private readonly ActivityInDiagram _end;
        public GameObject GameObject;
        public ActivityRelation(Graph graph, ActivityInDiagram start, ActivityInDiagram end)
        {
            _graph = graph;
            _type = "ASSOCIATION";
            _start = start;
            _end = end;
        }

        public void Generate()
        {
            Debug.Log("[Karin] ActivityRelation::Generate()");
            GameObject = InitEdge();
            if (GameObject == null)
            {
                Debug.Log("[Karin] InitEdge() returned null");
                return;
            }

            var uEdge = GameObject.GetComponent<UEdge>();
            if (uEdge == null)
            {
                Debug.Log("[Karin] UEdge component not found on GameObject");
                return;
            }

            uEdge.Points = new Vector2[]
            {
                _start.VisualObject.transform.position,
                _end.VisualObject.transform.position
            };

            Debug.Log("[Karin] ActivityRelation::Generate() - Edge created successfully");
        }

        private GameObject InitEdge()
        {
            if (_start.VisualObject == null || _end.VisualObject == null)
            {
                Debug.Log("[Karin] Start or End VisualObject is null");
                return null;
            }

            if (DiagramPool.Instance.associationNonePrefab == null)
            {
                Debug.Log("[Karin] associationNonePrefab is null");
                return null;
            }

            return _graph.AddEdge(_start.VisualObject, _end.VisualObject,
                DiagramPool.Instance.associationNonePrefab);
        }

    }
}