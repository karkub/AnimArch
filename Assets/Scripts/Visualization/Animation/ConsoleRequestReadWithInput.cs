using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Visualization.UI;
using Visualization.Animation;

namespace Visualization.Animation
{
    public class ConsoleRequestReadWithInput : ConsoleRequest
    {
        public string ReadValue { get; set; }

        private List<ConsoleRequest> childrenRequests = new List<ConsoleRequest>();

        public ConsoleRequestReadWithInput(string textToWrite) : base(textToWrite)
        {
            this.ReadValue = null;

            AddRequest(new ConsoleRequestWrite(textToWrite));
            AddRequest(new ConsoleRequestRead(textToWrite, this));
        }

        public override void PerformRequest()
        {
            foreach (var childrenRequest in this.childrenRequests) 
            {
                childrenRequest.PerformRequest();
            }
        }

        public void AddRequest(ConsoleRequest request) {
            childrenRequests.Add(request);
        }

        public void RemoveRequest(ConsoleRequest request) {
            childrenRequests.Remove(request);
        }

        public ConsoleRequest GetRequest() {
            return this;
        }
    }
}