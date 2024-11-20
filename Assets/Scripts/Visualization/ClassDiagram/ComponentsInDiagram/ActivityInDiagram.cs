using OALProgramControl;
using UnityEngine;

namespace Visualization.ClassDiagram.ComponentsInDiagram
{
    public class ActivityInDiagram
    {
        public string ActivityText;
        public string ActivityType = "";
        public int IndentationLevelX = 0;
        public int IndentationLevelY = 0;
        public string ConditionText = "";
        public GameObject VisualObject;

        public ActivityInDiagram() { }
    }
}
