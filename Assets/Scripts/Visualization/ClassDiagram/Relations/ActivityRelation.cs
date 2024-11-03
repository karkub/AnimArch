using System;
using UMSAGL.Scripts;
using UnityEngine;
using Visualization.ClassDiagram.ComponentsInDiagram;

namespace Visualization.ClassDiagram.Relations
{
    public class ActivityRelation
    {
        public readonly ActivityInDiagram From;
        public readonly ActivityInDiagram To;
        public GameObject VisualObject;
        public ActivityRelation(ActivityInDiagram start, ActivityInDiagram end)
        {
            From = start;
            To = end;
        }

        public void GenerateVisualObject(Graph graph)
        {
            Debug.Log("[Karin] ActivityRelation::GenerateVisualObject()");
            if (From.VisualObject == null || To.VisualObject == null)
            {
                Debug.LogError("[Karin] From or To VisualObject is null");
                return;
            }

            Debug.LogFormat("[Karin] From: {0}, To: {1}", From.ActivityText, To.ActivityText);
            VisualObject = graph.AddEdge(From.VisualObject, To.VisualObject, DiagramPool.Instance.associationSDPrefab);
            if (VisualObject == null)
            {
                Debug.LogError("[Karin] Failed to create visual object for relation");
            }
        }
    }
}