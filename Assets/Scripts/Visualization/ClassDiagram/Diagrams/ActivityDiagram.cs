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
using OALProgramControl;

namespace AnimArch.Visualization.Diagrams
{
    public class ActivityDiagram : Diagram
    {
        public Graph graph;
        public List<ActivityInDiagram> Activities { get; private set; }
        public List<ActivityRelation> Relations { get; private set; }

        private int activityOffsetX = 500;
        private int activityOffsetY = -150;

        public ActivityDiagram()
        {
            Activities = new List<ActivityInDiagram>();
            Relations = new List<ActivityRelation>();
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
        }

        public void LoadDiagram()
        {
            Generate();
        }

        public void SaveDiagram()
        {
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
                        case ActivityType.Merge:
                            GenerateMergeActivity(activity);
                            break;
                        case ActivityType.Decision:
                            GenerateDecisionActivity(activity);
                            break;
                        case ActivityType.Final:
                            GenerateFinalActivity(activity);
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

        public ActivityInDiagram AddActivityInDiagram(string variableName, int indentationLevelX, int indentationLevelY, EXECommand command)
        {
            ActivityInDiagram activityInDiagram = new ActivityInDiagram
            {
                ActivityText = variableName,
                ActivityType = ActivityType.Classic,
                IndentationLevelX = indentationLevelX,
                IndentationLevelY = indentationLevelY,
                Command = command,
                VisualObject = null
            };
            Activities.Add(activityInDiagram);
            GenerateActivity(activityInDiagram);

            return activityInDiagram;
        }

        public ActivityInDiagram AddInitialActivityInDiagram(EXECommand command)
        {
            ActivityInDiagram initialActivityInDiagram = new ActivityInDiagram
            {
                ActivityText = "Initial Activity",
                ActivityType = ActivityType.Initial,
                IndentationLevelX = 0,
                IndentationLevelY = 0,
                Command = command,
                VisualObject = null,
            };
            Activities.Add(initialActivityInDiagram);
            GenerateInitialActivity(initialActivityInDiagram);

            return initialActivityInDiagram;
        }

        public ActivityInDiagram AddFinalActivityInDiagram(int indentationLevelX, int indentationLevelY)
        {
            ActivityInDiagram finalActivityInDiagram = new ActivityInDiagram
            {
                ActivityText = "Final Activity",
                ActivityType = ActivityType.Final,
                IndentationLevelX = indentationLevelX,
                IndentationLevelY = indentationLevelY,
                VisualObject = null
            };
            Activities.Add(finalActivityInDiagram);
            GenerateFinalActivity(finalActivityInDiagram);

            return finalActivityInDiagram;
        }

        public ActivityInDiagram AddDecisionActivityInDiagram(int indentationLevelX, int indentationLevelY, EXECommand command) 
        {
            ActivityInDiagram decisionActivityInDiagram = new ActivityInDiagram
            {
                ActivityText = "Decision Node",
                ActivityType = ActivityType.Decision,
                IndentationLevelX = indentationLevelX,
                IndentationLevelY = indentationLevelY,
                Command = command,
                VisualObject = null
            };
            Activities.Add(decisionActivityInDiagram);
            GenerateDecisionActivity(decisionActivityInDiagram);

            return decisionActivityInDiagram;
        }

        public ActivityInDiagram AddMergeActivityInDiagram(int indentationLevelX, int indentationLevelY, EXECommand command) 
        {
            ActivityInDiagram mergeActivityInDiagram = new ActivityInDiagram
            {
                ActivityText = "Merge Node",
                ActivityType = ActivityType.Merge,
                IndentationLevelX = indentationLevelX,
                IndentationLevelY = indentationLevelY,
                Command = command,
                VisualObject = null
            };
            Activities.Add(mergeActivityInDiagram);
            GenerateMergeActivity(mergeActivityInDiagram);

            return mergeActivityInDiagram;
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

        private void GenerateMergeActivity(ActivityInDiagram Activity)
        {
            graph.nodePrefab = DiagramPool.Instance.activityMergePrefab;
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

        public void AddRelation(ActivityInDiagram from, ActivityInDiagram to, string labelText="")
        {
            Debug.LogFormat("[Karin] AddRelation from {0} to {1}; from[{2}, {3}], to[{4}, {5}]", from.ActivityText, to.ActivityText, from.IndentationLevelX, from.IndentationLevelY, to.IndentationLevelX, to.IndentationLevelY);
            ActivityRelation relation = new ActivityRelation(from, to, labelText);
            relation.GenerateVisualObject(graph);
            Relations.Add(relation);
        }

        public List<ActivityInDiagram> GetActivitiesInDiagram(EXECommand command)
        {
            List<ActivityInDiagram> activities = Activities.FindAll(activity => command.CommandID.Equals(activity.Command?.CommandID));
            return activities;
        }

        public List<ActivityRelation> GetActivityRelations(EXECommand command)
        {
            List<ActivityRelation> relations = Relations.FindAll(relation => command.CommandID.Equals(relation.To.Command?.CommandID));
            return relations;
        }

        public void PrintActivitiesInDiagram()
        {
            foreach (ActivityInDiagram Activity in Activities)
            {
                Debug.LogFormat("[Karin] {0}, X = {1}, Y = {2}, Command = {3}, CommandId = {4}, SuperScope = {5}", Activity.ActivityText, Activity.IndentationLevelX, Activity.IndentationLevelY, Activity.Command, Activity.Command?.CommandID, Activity.Command?.SuperScope);
            }
        }

    }
}