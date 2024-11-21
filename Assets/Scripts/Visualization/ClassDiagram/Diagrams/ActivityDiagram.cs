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

        private int activityOffsetX = 500;
        private int activityOffsetY = -150;
        private int maxIndentationLevelY = 0;

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
        }

        public void LoadDiagram()
        {
            CreateGraph();
            // Generate UI objects displaying the diagram
            graph.transform.position = new Vector3(0, 0, 2 * offsetZ);
            Generate();
        }

        public void SaveDiagram()
        {
            Debug.Log("[Karin] ActivityDiagram::SaveDiagram() PUSH");
            ActivityDiagramManager.Instance.ActivityDiagrams.Push(this.copy());
            ActivityDiagramManager.Instance.PrintDiagamsInStack();
        }

        private ActivityDiagram copy()
        {
            ActivityDiagram newActivityDiagram = new ActivityDiagram();
            newActivityDiagram.Activities = new List<ActivityInDiagram>(Activities);
            newActivityDiagram.Relations = new List<ActivityRelation>(Relations);
            return newActivityDiagram;
        }

        private Graph CreateGraph()
        {
            var go = Instantiate(DiagramPool.Instance.graphPrefab);
            graph = go.GetComponent<Graph>();
            graph.nodePrefab = DiagramPool.Instance.activityPrefab; 
            return graph;
        }

        public void Generate()
        {
            //Render activities
            Debug.Log("[Karin] ActivityDiagram::Generate()");
            Debug.LogFormat("[Karin] Activities.Count = {0}", Activities.Count);
            if (Activities != null && Activities.Count > 0)
            {
                foreach (ActivityInDiagram activity in Activities)
                {
                    switch (activity.ActivityText)
                    {
                        case "Initial Activity":
                            GenerateInitialActivity(activity);
                            break;
                        case "Final Activity":
                            GenerateFinalActivity(activity);
                            break;
                        case "Decision Node":
                            GenerateDecisionActivity(activity);
                            break;
                        default:
                            GenerateActivity(activity);
                            break;
                    }
                }

                foreach (ActivityRelation relation in Relations)
                {
                    relation.GenerateVisualObject(graph);
                }
            }
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
            // graph.Layout();
        }

        private void GenerateInitialActivity(ActivityInDiagram Activity)
        {
            graph.nodePrefab = DiagramPool.Instance.activityInitialPrefab;
            var node = graph.AddNode();

            Activity.VisualObject = node;
            RepositionActivity(Activity);
            // graph.Layout();
        }

        private void GenerateFinalActivity(ActivityInDiagram Activity)
        {
            graph.nodePrefab = DiagramPool.Instance.activityFinalPrefab;
            var node = graph.AddNode();

            Activity.VisualObject = node;
            RepositionActivity(Activity);
            // graph.Layout();
        }

        private void GenerateDecisionActivity(ActivityInDiagram Activity)
        {
            graph.nodePrefab = DiagramPool.Instance.activityDecisionPrefab;
            var node = graph.AddNode();

            Activity.VisualObject = node;
            RepositionActivity(Activity);
            // graph.Layout();
        }

        public void AddActivityInDiagram(string variableName, int indentationLevelX, int indentationLevelY)
        {
            if (Activities.Count == 0)
            {
                AddInitialActivityInDiagram();
            }

            ActivityInDiagram activityInDiagram = new ActivityInDiagram
            {
                ActivityText = variableName,
                IndentationLevelX = indentationLevelX,
                IndentationLevelY = indentationLevelY,
                VisualObject = null
            };

            maxIndentationLevelY = Math.Max(maxIndentationLevelY, indentationLevelY);
            AddVisualPartOfActivity(activityInDiagram, "classic");
            // AddRelation();
        }

        private void AddInitialActivityInDiagram()
        {
            ActivityInDiagram initialActivityInDiagram = new ActivityInDiagram
            {
                ActivityText = "Initial Activity",
                IndentationLevelX = 0,
                IndentationLevelY = 0,
                VisualObject = null,
            };
            AddVisualPartOfActivity(initialActivityInDiagram, "initial");
            // AddRelation();
        }

        public void AddFinalActivityInDiagram()
        {
            ActivityInDiagram finalActivityInDiagram = new ActivityInDiagram
            {
                ActivityText = "Final Activity",
                IndentationLevelX = 0,
                IndentationLevelY = maxIndentationLevelY + 1,
                VisualObject = null
            };
            AddVisualPartOfActivity(finalActivityInDiagram, "final");
            AddRelation();
        }

        public void AddDecisionActivityInDiagram(int indentationLevelX, int indentationLevelY, string decisionType, string conditionText = "")
        {
            if (Activities.Count == 0)
            {
                AddInitialActivityInDiagram();
            }

            ActivityInDiagram decisionActivityInDiagram = new ActivityInDiagram
            {
                ActivityText = "Decision Node",
                ActivityType = decisionType,
                IndentationLevelX = indentationLevelX,
                IndentationLevelY = indentationLevelY,
                ConditionText = conditionText,
                VisualObject = null
            };
            maxIndentationLevelY = Math.Max(maxIndentationLevelY, indentationLevelY);
            AddVisualPartOfActivity(decisionActivityInDiagram, "decision");
            // AddRelation();
        }

        private void AddVisualPartOfActivity(ActivityInDiagram Activity, string typeOfActivity)
        {
            Activities.Add(Activity);
            switch (typeOfActivity)
            {
                case "initial":
                    GenerateInitialActivity(Activity);
                    break;
                case "final":
                    GenerateFinalActivity(Activity);
                    break;
                case "decision":
                    GenerateDecisionActivity(Activity);
                    break;
                default:
                    GenerateActivity(Activity);
                    break;
            }
        }

        public void RepositionActivity(ActivityInDiagram Activity)
        {
            Activity.VisualObject.transform.localPosition = new Vector3(
                Activity.IndentationLevelX * activityOffsetX, 
                Activity.IndentationLevelY * activityOffsetY, 
                0);
        }

        private void AddRelation()
        {
            if (Activities.Count < 2)
            {
                Debug.LogError("[Karin] ActivityDiagram::AddRelation() - Not enough activities in diagram to create a relation.");
                return;
            }

            ActivityInDiagram from = Activities[Activities.Count - 2];
            ActivityInDiagram to = Activities[Activities.Count - 1];
            ActivityRelation relation = new ActivityRelation(from, to);
            relation.GenerateVisualObject(graph);
            Relations.Add(relation);
        }
        public void AddRelations()
        {
            PrintActivitiesinDiagram();
            if (Activities.Count < 2)
            {
                Debug.LogError("[Karin] ActivityDiagram::AddRelation() - Not enough activities in diagram to create a relation.");
                return;
            }
            for (int i = 0; i < Activities.Count - 1; i++)
            {
                ActivityInDiagram from = Activities[i];
                ActivityInDiagram to = Activities[i + 1];

                if (from.ActivityType == "CONDITION_START")
                {
                    try {
                        ActivityInDiagram toIf = Activities.Find(x => from.IndentationLevelX == x.IndentationLevelX && from.IndentationLevelY + 1 == x.IndentationLevelY);
                        ActivityRelation relation1 = new ActivityRelation(from, toIf, from.ConditionText);
                        relation1.GenerateVisualObject(graph);
                        Relations.Add(relation1);

                        ActivityInDiagram toElse = Activities.Find(x => from.IndentationLevelX + 1 == x.IndentationLevelX && from.IndentationLevelY + 1 == x.IndentationLevelY);
                        ActivityRelation relation2 = new ActivityRelation(from, toElse, "else");
                        relation2.GenerateVisualObject(graph);
                        Relations.Add(relation2);
                    } catch (Exception e) {
                        Debug.LogError("[Karin] ActivityDiagram::AddRelations() - CONDITION_START - Exception");
                        Debug.LogError(e);
                    }
                }
                else if (to.ActivityType == "CONDITION_END")
                {
                    try {
                        ActivityInDiagram fromIf = Activities.FindLast(x => x.IndentationLevelX == to.IndentationLevelX && x.IndentationLevelY < to.IndentationLevelY);
                        ActivityRelation relation1 = new ActivityRelation(fromIf, to);
                        relation1.GenerateVisualObject(graph);
                        Relations.Add(relation1);

                        ActivityInDiagram fromElse = Activities.FindLast(x => x.IndentationLevelX - 1 == to.IndentationLevelX && x.IndentationLevelY < to.IndentationLevelY);
                        ActivityRelation relation2 = new ActivityRelation(fromElse, to);
                        relation2.GenerateVisualObject(graph);
                        Relations.Add(relation2);
                    } catch (Exception e) {
                        Debug.LogError("[Karin] ActivityDiagram::AddRelations() - CONDITION_END - Exception");
                        Debug.LogError(e);
                    }
                }
                else if (from.IndentationLevelX == to.IndentationLevelX)
                {
                    ActivityRelation relation = new ActivityRelation(from, to);
                    relation.GenerateVisualObject(graph);
                    Relations.Add(relation);
                }
            }
        }


        public void PrintActivitiesinDiagram()
        {
            Debug.Log("[Karin] -------- ActivityDiagram::PrintDiagram()");
            foreach (ActivityInDiagram Activity in Activities)
            {
                Debug.LogFormat("[Karin] {0}, X = {1}, Y = {2}", Activity.ActivityText, Activity.IndentationLevelX, Activity.IndentationLevelY);
            }
            Debug.Log("[Karin] -------- END ActivityDiagram::PrintDiagram()");
        }

    }
}