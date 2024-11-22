using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Msagl.Core.DataStructures;
using UnityEngine;

namespace Visualization.Animation
{
    public class AnimationScheduler
    {
        private enum ThreadStatus {
            RUNNING = 0,
            TERMINATED = 1
        };

        private struct ThreadQueue {
            public Queue<AnimationRequest> queue;
            public ThreadStatus status;
        };

        private Dictionary<int, ThreadQueue> requestQueues;
        private bool wantsToterminate;

        public AnimationScheduler()
        {
            requestQueues = new Dictionary<int, ThreadQueue>();
            wantsToterminate = false;
        }

        public bool IsOver()
        {
            return wantsToterminate && this.requestQueues.Values.All((q) => q.status == ThreadStatus.TERMINATED);
        }

        private IEnumerator QueueLoop(Queue<AnimationRequest> queue)
        {
            AnimationRequest currentRequest;
            while (true)
            {
                yield return new WaitUntil(() => queue.Any() || IsOver());

                if (IsOver())
                {
                    break;
                }

                currentRequest = queue.Dequeue();
                Animation.Instance.StartCoroutine(currentRequest.PerformRequest());

                yield return new WaitUntil(() => currentRequest.IsDone());
            }
        }

        public void Enqueue(AnimationRequest request)
        {
            if (!this.requestQueues.ContainsKey(request.GetThreadId()))
            {
                Queue<AnimationRequest> newQueue = new Queue<AnimationRequest>();
                this.requestQueues.Add(request.GetThreadId(), new ThreadQueue { 
                    queue = newQueue,
                    status = ThreadStatus.RUNNING
                });

                Animation.Instance.StartCoroutine(QueueLoop(newQueue));
            }
            this.requestQueues[request.GetThreadId()].queue.Enqueue(request);
        }

        public void Terminate()
        {
            wantsToterminate = true;
            foreach(var pair in this.requestQueues) {
                pair.Value.queue.Enqueue(new AnimationTerminateRequest(null, null, false, false, () => {
                    ThreadQueue q = requestQueues[pair.Key];
                    q.status = ThreadStatus.TERMINATED;
                    requestQueues[pair.Key] = q;
                    return true;
                }));
            }
            
        }
    }
}