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
            DiagramPool.Instance.ActivityDiagram = new ActivityDiagram();
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

        public void PrintDiagamsInStack()
        {
            Debug.Log("[Karin] ~~~~~ ActivityDiagramManager::PrintStack()");
            Debug.LogFormat("[Karin] ActivityDiagrams.Count {0}", ActivityDiagrams.Count);
            int i=0;
            foreach (var activityDiagram in ActivityDiagrams)
            {
                Debug.LogFormat("[Karin] activityDiagram.PrintDiagram() index = {0}", i);
                activityDiagram.PrintActivitiesInDiagram();
                i++;
            }
            Debug.Log("[Karin] ~~~~~ END ActivityDiagramManager::PrintStack()");
        }

    }
}