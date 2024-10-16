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

        private float initialActivityPositionX;
        private float initialActivityPositionZ;
        private float activityOffsetY = 70;
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
            //Generate UI objects displaying the diagram
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
                for (int i = 0; i < Activities.Count; i++)
                {
                    if (i == 0)
                    {
                        GenerateInitialActivity(Activities[i]);
                    }
                    //else if (i == Activities.Count - 1)
                    //{
                     //   GenerateFinalActivity(Activities[i]);
                    //}
                    else
                    {
                        GenerateActivity(Activities[i]);
                    }
                }
                RepositionActivities();

                foreach (ActivityRelation relation in Relations)
                {
                    relation.Generate();
                }
            }
        }

        private void GenerateActivity(ActivityInDiagram Activity)
        {
            //Setting up
            graph.nodePrefab = DiagramPool.Instance.activityPrefab;
            var node = graph.AddNode();
            node.GetComponent<Clickable>().IsObject = true;
            node.SetActive(true);
            node.name = Activity.ActivityText;
            var header = node.transform.Find("Background/Header");

            // Printing the values into diagram
            header.GetComponent<TextMeshProUGUI>().text = node.name;

            //Add Class to Dictionary
            Activity.VisualObject = node;
        }

        private void GenerateInitialActivity(ActivityInDiagram Activity)
        {
            graph.nodePrefab = DiagramPool.Instance.activityInitialPrefab;
            var node = graph.AddNode();
            Activity.VisualObject = node;
            initialActivityPositionX = node.transform.position.x;
            initialActivityPositionZ = node.transform.position.z;
        }

        private void GenerateFinalActivity(ActivityInDiagram Activity)
        {
            graph.nodePrefab = DiagramPool.Instance.activityFinalPrefab;
            var node = graph.AddNode();
            Activity.VisualObject = node;
        }

        public void AddActivityInDiagram(string variableName)
        {
            if (Activities.Count == 0)
            {
                AddInitialActivityInDiagram();
            }

            ActivityInDiagram activityInDiagram = new ActivityInDiagram
            {
                ActivityText = variableName,
                VisualObject = null
            };
            AddVisualPartOfActivity(activityInDiagram);
            RepositionActivities();
        }

        private void AddInitialActivityInDiagram()
        {
            ActivityInDiagram initialActivityInDiagram = new ActivityInDiagram
            {
                ActivityText = "Initial Activity",
                VisualObject = null
            };
            AddVisualPartOfActivity(initialActivityInDiagram, "initial");
        }

        public void AddFinalActivityInDiagram()
        {
            ActivityInDiagram finalActivityInDiagram = new ActivityInDiagram
            {
                ActivityText = "Final Activity",
                VisualObject = null
            };
            AddVisualPartOfActivity(finalActivityInDiagram, "final");
            RepositionActivities();
        }

        private void AddVisualPartOfActivity(ActivityInDiagram Activity, string typeOfActivity = "classic")
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
                default:
                    GenerateActivity(Activity);
                    break;
            }
            // graph.Layout(); // TODOa
        }

        public void RepositionActivities()
        {
            Debug.Log("[Karin] ActivityDiagram::RepositionActivities()");
            int i = 0;
            foreach (ActivityInDiagram activityInDiagram in Activities)
            {
                activityInDiagram.VisualObject.transform.SetPositionAndRotation(
                    new Vector3(initialActivityPositionX, -i * activityOffsetY, initialActivityPositionZ),
                    Quaternion.identity);
                i++;
            }
        }

        public void AddRelation()
        {
            ActivityInDiagram start = Activities[Activities.Count - 2];
            ActivityInDiagram end = Activities[Activities.Count - 1];
            Debug.LogFormat("[Karin] Adding relation from {0} to {1}", start.ActivityText, end.ActivityText);
            ActivityRelation relation = new ActivityRelation(graph, start, end);
            Relations.Add(relation);
            relation.Generate();
        }


        public void PrintActivitiesinDiagram()
        {
            Debug.Log("[Karin] -------- ActivityDiagram::PrintDiagram()");
            foreach (ActivityInDiagram Activity in Activities)
            {
                Debug.LogFormat("[Karin] {0}", Activity.ActivityText);
            }
            Debug.Log("[Karin] -------- END ActivityDiagram::PrintDiagram()");
        }
    }
}