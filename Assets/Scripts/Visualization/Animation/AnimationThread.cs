using OALProgramControl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Visualization.Animation
{
    public class AnimationThread
    {
        private readonly EXEExecutionStack CommandStack;
        private readonly OALProgram CurrentProgramInstance;
        private readonly Animation Animation;
        public EXEExecutionResult ExecutionSuccess;
        protected AnimationThread ParentThread;
        protected readonly List<AnimationThread> ChildThreads;
        public readonly int ID;

        protected bool IsOver;

        private bool _animate;
        protected bool Animate
        {
            get
            {
                if (!_animate) { return false; }
                if (ParentThread == null) { return true; }

                return ParentThread.Animate;
            }
        }


        private bool _animateNewObjects;
        protected bool AnimateNewObjects
        {
            get
            {
                if (!_animateNewObjects) { return false; }
                if (ParentThread == null) { return true; }

                return ParentThread.AnimateNewObjects;
            }
        }

        private static int IdSeed = 0;

        public EXEScopeMethod CurrentMethod {
            get {
                return CommandStack.Peek()?.GetCurrentMethodScope();
            }
        }

        public AnimationThread(EXEExecutionStack executionStack, OALProgram currentProgramInstance, Animation animation)
        {
            this.CommandStack = executionStack;
            this.CurrentProgramInstance = currentProgramInstance;
            this.Animation = animation;
            this.ExecutionSuccess = EXEExecutionResult.Success();
            this.IsOver = false;
            this.ParentThread = null;
            this.ChildThreads = new List<AnimationThread>();
            this._animate = true;
            this._animateNewObjects = true;

            IdSeed++;
            this.ID = IdSeed;
        }

        public IEnumerator Start()
        {
            int i = 0;
            while (ExecutionSuccess.IsSuccess && CommandStack.HasNext())
            {
                EXECommand CurrentCommand = CommandStack.Next();

                if (CurrentCommand is EXECommandPragma)
                {
                    (CurrentCommand as EXECommandPragma).CurrentThread = this;
                }

                ExecutionSuccess = CurrentCommand.PerformExecution(CurrentProgramInstance);

                Debug.LogFormat("Thread {2}. Command {0}:{1}", i++, ExecutionSuccess.ToString(), this.ID);

                if (!ExecutionSuccess.IsSuccess)
                {
                    ShowError(ExecutionSuccess);
                    break;
                }

                if (!ExecutionSuccess.IsDone)
                {
                    continue;
                }

                if (Animate)
                {
                    CurrentCommand.ToggleActiveRecursiveBottomUp(true);
                    EXEScopeMethod CurrentMethodScope = CurrentCommand.GetCurrentMethodScope();
                    UI.MenuManager.Instance.RefreshSourceCodePanel(CurrentMethodScope);
                    CurrentCommand.ToggleActiveRecursiveBottomUp(false);
                }

                yield return Animation.AnimateCommand(CurrentCommand, this, Animate, AnimateNewObjects);

                if (!ExecutionSuccess.IsSuccess)
                {
                    ShowError(ExecutionSuccess);
                    break;
                }

                if (CurrentCommand is EXEScopeParallel)
                {
                    Fork(CurrentCommand as EXEScopeParallel);
                    yield return Join();
                }
            }

            Debug.LogFormat("Thread {0} is over.", this.ID);
            IsOver = true;
        }

        private void Fork(EXEScopeParallel forkingCommand)
        {
            foreach (EXEScope threadScope in forkingCommand.Threads)
            {
                EXEExecutionStack ChildThreadExecutionStack = new EXEExecutionStack();
                ChildThreadExecutionStack.Enqueue(threadScope);
                threadScope.CommandStack = ChildThreadExecutionStack;

                AnimationThread ChildThread = new AnimationThread(ChildThreadExecutionStack, CurrentProgramInstance, Animation);
                this.ChildThreads.Add(ChildThread);
                ChildThread.ParentThread = this;

                Animation.StartCoroutine(ChildThread.Start());
            }
        }

        private IEnumerator Join()
        {
            yield return new WaitUntil(() => !ChildThreads.Any(thread => !thread.IsOver));
        }

        private void ShowError(EXEExecutionResult executionSuccess)
        {
            UI.MenuManager.Instance.ShowErrorPanel(executionSuccess);
        }

        public void ToggleAnimate(bool animate)
        {
            this._animate = animate;
        }
        public void ToggleNewObjectAnimate(bool animate)
        {
            this._animateNewObjects = animate;
        }
    }
}