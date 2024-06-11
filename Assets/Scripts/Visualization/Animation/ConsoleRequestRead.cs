using System.Collections;
using UnityEditor;
using UnityEngine;
using Visualization.UI;
using Visualization.Animation;

namespace Visualization.Animation
{
    public class ConsoleRequestRead : ConsoleRequest
    {
        public string ReadValue { get; set; }

        public ConsoleRequestReadWithInput Request;

        public ConsoleRequestRead(string textToWrite, ConsoleRequestReadWithInput request) : base(textToWrite)
        {
            this.ReadValue = null;
            this.Request = request;
        }

        public override void PerformRequest()
        {
            ConsolePanel.Instance.ActivateInputField(this);
        }
    }
}