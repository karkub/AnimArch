using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UMSAGL.Scripts;
using UnityEngine;
using Visualization;
using Visualization.ClassDiagram;
using Visualization.ClassDiagram.ComponentsInDiagram;
using Visualization.ClassDiagram.Diagrams;
using Visualization.ClassDiagram.Relations;

namespace AnimArch.Visualization.Diagrams
{
    public class ActivityDiagram : Diagram
    {
        public Graph graph;
        public List<ActivityInDiagram> Activities { get; private set; }
        public List<ActivityRelation> Relations { get; private set; }
        private int maxIndentationLevelY;

        private int activityOffsetX = 500;
        private int activityOffsetY = -150;

        public ActivityDiagram()
        {
            Activities = new List<ActivityInDiagram>();
            Relations = new List<ActivityRelation>();
            maxIndentationLevelY = 0;
        }

        private void Awake()
        {
            DiagramPool.Instance.ActivityDiagram = this;
        }

        public void ResetDiagram()
        {
            if (DiagramPool.Instance.ClassDiagram && DiagramPool.Instance.ClassDiagram.Classes != null)
            {
                foreach (var classDiagramClass in DiagramPool.Instance.ClassDiagram.Classes)
                {
                    classDiagramClass.ClassInfo.Instances.Clear();
                }
            }

            ClearDiagram();

            if (graph != null)
            {
                Destroy(graph.gameObject);
                graph = null;
            }
        }
        
        public void ClearDiagram()
        {
            // Get rid of already rendered activities in diagram.
            if (Activities != null && Activities.Count > 0)
            {
                foreach (ActivityInDiagram Activity in Activities)
                {
                    Destroy(Activity.VisualObject);
                }
            }
            Activities = new List<ActivityInDiagram>();
            Relations = new List<ActivityRelation>();
            maxIndentationLevelY = 0;
        }

        public void LoadDiagram()
        {
            Generate();
        }

        public void SaveDiagram()
        {
            Debug.Log("[Karin] ActivityDiagram::SaveDiagram() PUSH");
            ActivityDiagramManager.Instance.ActivityDiagrams.Push(this);
            ActivityDiagramManager.Instance.PrintDiagamsInStack();
        }

        public Graph CreateGraph()
        {
            var go = Instantiate(DiagramPool.Instance.graphPrefab);
            graph = go.GetComponent<Graph>();
            graph.nodePrefab = DiagramPool.Instance.activityPrefab;
            graph.transform.position = new Vector3(0, 0, (2 + ActivityDiagramManager.Instance.ActivityDiagrams.Count) * offsetZ);
            return graph;
        }

        public void Generate()
        {
            //Render activities
            if (Activities != null && Activities.Count > 0)
            {
                foreach (ActivityInDiagram activity in Activities)
                {
                    switch (activity.ActivityType)
                    {
                        case ActivityType.Initial:
                            GenerateInitialActivity(activity);
                            break;
                        case ActivityType.Classic:
                            GenerateActivity(activity);
                            break;
                        case ActivityType.Final:
                            GenerateFinalActivity(activity);
                            break;
                        default:
                            GenerateDecisionActivity(activity);
                            break;
                    }
                }

                foreach (ActivityRelation relation in Relations)
                {
                    relation.GenerateVisualObject(graph);
                }
            }
        }

        public ActivityInDiagram AddActivityInDiagram(string variableName, int indentationLevelX, int indentationLevelY)
        {
            ActivityInDiagram activityInDiagram = new ActivityInDiagram
            {
                ActivityText = variableName,
                ActivityType = ActivityType.Classic,
                IndentationLevelX = indentationLevelX,
                IndentationLevelY = indentationLevelY,
                VisualObject = null
            };

            maxIndentationLevelY = Math.Max(maxIndentationLevelY, indentationLevelY);
            Activities.Add(activityInDiagram);
            GenerateActivity(activityInDiagram);

            return activityInDiagram;
        }

        public ActivityInDiagram AddInitialActivityInDiagram()
        {
            ActivityInDiagram initialActivityInDiagram = new ActivityInDiagram
            {
                ActivityText = "Initial Activity",
                ActivityType = ActivityType.Initial,
                IndentationLevelX = 0,
                IndentationLevelY = 0,
                VisualObject = null,
            };
            Activities.Add(initialActivityInDiagram);
            GenerateInitialActivity(initialActivityInDiagram);

            return initialActivityInDiagram;
        }

        public ActivityInDiagram AddFinalActivityInDiagram()
        {
            ActivityInDiagram finalActivityInDiagram = new ActivityInDiagram
            {
                ActivityText = "Final Activity",
                ActivityType = ActivityType.Final,
                IndentationLevelX = 0,
                IndentationLevelY = maxIndentationLevelY + 1,
                VisualObject = null
            };
            Activities.Add(finalActivityInDiagram);
            GenerateFinalActivity(finalActivityInDiagram);

            return finalActivityInDiagram;
        }

        public ActivityInDiagram AddDecisionActivityInDiagram(int indentationLevelX, int indentationLevelY, ActivityType activityType, string labelText = "")
        {
            ActivityInDiagram decisionActivityInDiagram = new ActivityInDiagram
            {
                ActivityText = "Decision Node",
                ActivityType = activityType,
                IndentationLevelX = indentationLevelX,
                IndentationLevelY = indentationLevelY,
                LabelText = labelText,
                VisualObject = null
            };
            maxIndentationLevelY = Math.Max(maxIndentationLevelY, indentationLevelY);
            Activities.Add(decisionActivityInDiagram);
            GenerateDecisionActivity(decisionActivityInDiagram);

            return decisionActivityInDiagram;
        }


        private void GenerateActivity(ActivityInDiagram Activity)
        {
            graph.nodePrefab = DiagramPool.Instance.activityPrefab;
            var node = graph.AddNode();
            node.GetComponent<Clickable>().IsObject = true;
            node.SetActive(true);
            node.name = Activity.ActivityText.Trim();
            var header = node.transform.Find("Background/Header");
            header.GetComponent<TextMeshProUGUI>().text = node.name;

            Activity.VisualObject = node;
            RepositionActivity(Activity);
        }

        private void GenerateInitialActivity(ActivityInDiagram Activity)
        {
            graph.nodePrefab = DiagramPool.Instance.activityInitialPrefab;
            var node = graph.AddNode();

            Activity.VisualObject = node;
            RepositionActivity(Activity);
        }

        private void GenerateFinalActivity(ActivityInDiagram Activity)
        {
            graph.nodePrefab = DiagramPool.Instance.activityFinalPrefab;
            var node = graph.AddNode();

            Activity.VisualObject = node;
            RepositionActivity(Activity);
        }

        private void GenerateDecisionActivity(ActivityInDiagram Activity)
        {
            graph.nodePrefab = DiagramPool.Instance.activityDecisionPrefab;
            var node = graph.AddNode();

            Activity.VisualObject = node;
            RepositionActivity(Activity);
        }
        
        public void RepositionActivity(ActivityInDiagram Activity)
        {
            Activity.VisualObject.transform.localPosition = new Vector3(
                Activity.IndentationLevelX * activityOffsetX, 
                Activity.IndentationLevelY * activityOffsetY, 
                0);
        }

        public void AddRelations()
        {
            if (Activities.Count < 2)
            {
                Debug.LogError("[Karin] Not enough activities in diagram to create a relation.");
                return;
            }
            for (int i = 0; i < Activities.Count - 1; i++)
            {
                ActivityInDiagram from = Activities[i];
                ActivityInDiagram to = Activities[i + 1];
                try {
                    if (from.ActivityType == ActivityType.Decision)
                    {
                        ActivityInDiagram toIf = Activities.Find(x => from.IndentationLevelX == x.IndentationLevelX && from.IndentationLevelY + 1 == x.IndentationLevelY);
                        ActivityRelation relation1 = new ActivityRelation(from, toIf, "[" + from.LabelText + "]");
                        relation1.GenerateVisualObject(graph);
                        Relations.Add(relation1);

                        ActivityInDiagram toElse = Activities.Find(x => from.IndentationLevelX + 1 == x.IndentationLevelX && from.IndentationLevelY + 1 == x.IndentationLevelY);
                        ActivityRelation relation2 = new ActivityRelation(from, toElse, "else");
                        relation2.GenerateVisualObject(graph);
                        Relations.Add(relation2);
                    }
                    else if (to.ActivityType == ActivityType.Merge)
                    {
                        ActivityInDiagram fromIf = Activities.FindLast(x => x.IndentationLevelX == to.IndentationLevelX && x.IndentationLevelY < to.IndentationLevelY);
                        ActivityRelation relation1 = new ActivityRelation(fromIf, to);
                        relation1.GenerateVisualObject(graph);
                        Relations.Add(relation1);

                        ActivityInDiagram fromElse = Activities.FindLast(x => x.IndentationLevelX - 1 == to.IndentationLevelX && x.IndentationLevelY < to.IndentationLevelY);
                        ActivityRelation relation2 = new ActivityRelation(fromElse, to);
                        relation2.GenerateVisualObject(graph);
                        Relations.Add(relation2);
                    }
                    else if (from.ActivityType == ActivityType.Loop)
                    {
                        string labelText = "another " + from.LabelText;
                        ActivityRelation relation = new ActivityRelation(from, to, labelText);
                        relation.GenerateVisualObject(graph);
                        Relations.Add(relation);

                        ActivityInDiagram loopEnd = Activities.FindLast(x => from.IndentationLevelX + 1 == x.IndentationLevelX && from.IndentationLevelY < x.IndentationLevelY);
                        ActivityRelation relation2 = new ActivityRelation(loopEnd, from);
                        relation2.GenerateVisualObject(graph);
                        Relations.Add(relation2);
                    }
                    else if (from.ActivityType == ActivityType.LoopDecision)
                    {
                        // ActivityInDiagram toIf = Activities.Find(x => from.IndentationLevelX + 1 == x.IndentationLevelX && from.IndentationLevelY + 1 == x.IndentationLevelY);
                        ActivityInDiagram toIf = Activities.Find(x => from.IndentationLevelX + 1 == x.IndentationLevelX && from.IndentationLevelY == x.IndentationLevelY);
                        ActivityRelation relation1 = new ActivityRelation(from, toIf, "[yes]");
                        relation1.GenerateVisualObject(graph);
                        Relations.Add(relation1);

                        ActivityInDiagram toElse = Activities.Find(x => from.IndentationLevelX == x.IndentationLevelX && from.IndentationLevelY + 1 == x.IndentationLevelY);
                        ActivityRelation relation2 = new ActivityRelation(from, toElse, "[no]");
                        relation2.GenerateVisualObject(graph);
                        Relations.Add(relation2);
                    }
                    else if (from.IndentationLevelX == to.IndentationLevelX)
                    {
                        ActivityRelation relation = new ActivityRelation(from, to);
                        relation.GenerateVisualObject(graph);
                        Relations.Add(relation);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("[Karin] Error while creating relation: " + e.Message);
                }
            }
        }

        public void PrintActivitiesInDiagram()
        {
            Debug.Log("[Karin] -------- ActivityDiagram::PrintDiagram()");
            foreach (ActivityInDiagram Activity in Activities)
            {
                Debug.LogFormat("[Karin] {0}, X = {1}, Y = {2}, type = {3}", Activity.ActivityText, Activity.IndentationLevelX, Activity.IndentationLevelY, Activity.ActivityType);
            }
            Debug.Log("[Karin] -------- END ActivityDiagram::PrintDiagram()");
        }

    }
}