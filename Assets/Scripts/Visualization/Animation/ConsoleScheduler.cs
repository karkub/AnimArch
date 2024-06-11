using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Visualization.Animation
{
    public class ConsoleScheduler
    {
        private readonly Queue<ConsoleRequestReadWithInput> BlockingRequestQueue;
        private readonly Queue<ConsoleRequestWrite> NonblockingRequestQueue;
        private bool Over;

        public ConsoleScheduler()
        {
            this.BlockingRequestQueue = new Queue<ConsoleRequestReadWithInput>();
            this.NonblockingRequestQueue = new Queue<ConsoleRequestWrite>();
            this.Over = false;
        }

        public IEnumerator Start(MonoBehaviour animation)
        {
            animation.StartCoroutine(QueueLoop(BlockingRequestQueue));
            animation.StartCoroutine(QueueLoop(NonblockingRequestQueue));
            yield return new WaitForFixedUpdate();
        }

        private IEnumerator QueueLoop<T>(Queue<T> queue) where T : ConsoleRequest
        {
            T currentRequest;
            while (true)
            {
                yield return new WaitUntil(() => queue.Any() || Over);

                if (Over) { break; }

                currentRequest = queue.Dequeue();
                currentRequest.PerformRequest();

                yield return new WaitUntil(() => currentRequest.Done);
            }
        }

        public void Enqueue(ConsoleRequestReadWithInput request)
        {
            this.BlockingRequestQueue.Enqueue(request);
        }
        public void Enqueue(ConsoleRequestWrite request)
        {
            this.NonblockingRequestQueue.Enqueue(request);
        }

        public void Terminate()
        {
            this.Over = true;
        }
    }
}