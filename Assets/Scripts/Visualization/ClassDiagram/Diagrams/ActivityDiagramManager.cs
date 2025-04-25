using System;
using System.Collections.Generic;
using UnityEngine;
using Visualization.ClassDiagram;
using Visualization.ClassDiagram.Diagrams;
namespace AnimArch.Visualization.Diagrams
{
    public class ActivityDiagramManager
    {
        private static ActivityDiagramManager _instance = null;

        public Stack<ActivityDiagram> ActivityDiagrams { get; set; }

        private ActivityDiagramManager()
        {
            ActivityDiagrams = new Stack<ActivityDiagram>();
        }

        public static ActivityDiagramManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ActivityDiagramManager();
                }
                return _instance;
            }
        }

        public void ClearDiagrams()
        {
            while (ActivityDiagrams.Count > 0)
            {
                ActivityDiagram activityDiagram = ActivityDiagrams.Pop();
                activityDiagram.ClearDiagram();
            }
        }

        public void PrintDiagamsInStack()
        {
            Debug.LogFormat("[Karin] ~~~~~ Print All Activity Diagrams, count: {0}", ActivityDiagrams.Count);
            int i=0;
            foreach (var activityDiagram in ActivityDiagrams)
            {
                Debug.LogFormat("[Karin]      ---- Print Activities In Diagram {0}", i);
                activityDiagram.PrintActivitiesInDiagram();
                Debug.Log("[Karin]      ------------------------");
                i++;
            }
            Debug.Log("[Karin] ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        }

    }
}