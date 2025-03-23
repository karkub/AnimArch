using OALProgramControl;
using UnityEngine;

namespace Visualization.ClassDiagram.ComponentsInDiagram
{
    public class ActivityInDiagram
    {
        public string ActivityText;
        public ActivityType ActivityType;
        public int IndentationLevelX = 0;
        public int IndentationLevelY = 0;
        public EXECommand Command = null;
        public bool IsHighlighted = false;
        public GameObject VisualObject;

        public ActivityInDiagram() { }
    }
    
    public enum ActivityType
    {
        Initial,
        Classic,
        Final,
        Decision,
        Merge,
    }
}
