using System;
using TMPro;
using UMSAGL.Scripts;
using UnityEngine;
using Visualization.ClassDiagram.ComponentsInDiagram;

namespace Visualization.ClassDiagram.Relations
{
    public class ActivityRelation
    {
        public readonly ActivityInDiagram From;
        public readonly ActivityInDiagram To;
        public string Label;
        public bool IsHighlighted = false;

        public GameObject VisualObject;
        public ActivityRelation(ActivityInDiagram start, ActivityInDiagram end, string label = "")
        {
            From = start;
            To = end;
            Label = label;
        }

        public void GenerateVisualObject(Graph graph)
        {
            if (From.VisualObject == null || To.VisualObject == null)
            {
                Debug.LogError("[Karin] From or To VisualObject is null");
                return;
            }

            VisualObject = graph.AddEdge(From.VisualObject, To.VisualObject, DiagramPool.Instance.activityFlowPrefab);
            if (VisualObject == null)
            {
                Debug.LogError("[Karin] Failed to create visual object for relation");
                return;
            } 
            var label = VisualObject.transform.Find("Label/Text");
            if (Label != "")
            {
                label.GetComponent<TextMeshProUGUI>().text = Label;
            }
            
        }
    }
}