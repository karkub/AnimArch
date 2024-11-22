using OALProgramControl;
using UnityEngine;

namespace Visualization.ClassDiagram.ComponentsInDiagram
{
    public class ActivityInDiagram
    {
        public string ActivityText;
        public ActivityType ActivityType = ActivityType.Classic;
        public int IndentationLevelX = 0;
        public int IndentationLevelY = 0;
        public string LabelText = "";
        public GameObject VisualObject;

        public ActivityInDiagram() { }
    }
    
    public enum ActivityType
    {
        Classic,
        Loop,
        LoopDecision,
        Decision,
        Merge,
    }
}
