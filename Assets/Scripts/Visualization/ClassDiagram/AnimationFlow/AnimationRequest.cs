using System.Collections;
using OALProgramControl;
using UnityEngine;
using Visualization.UI;

namespace Visualization.Animation
{
    public abstract class AnimationRequest
    {
        protected bool Done { get; set; }
        protected MethodInvocationInfo callInfo;
        protected static Animation animation = Animation.Instance;
        protected bool animate;
        protected bool animateNewObjects;
        protected AnimationThread thread;
        protected EXECommand command;


        public AnimationRequest(EXECommand command, AnimationThread thread, bool animate, bool animateNewObjects)
        {
            this.command = command;
            this.Done = false;
            this.thread = thread;
            this.animate = animate;
            this.animateNewObjects = animateNewObjects;
        }

        public bool IsDone()
        {
            return Done;
        }

        public int GetThreadId()
        {
            return thread.ID;
        }

        public abstract IEnumerator PerformRequest();
    }
}