using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using OALProgramControl;
using UnityEngine;

namespace Visualization.ClassDiagram
{

public class EdgeHighlightSubject : HighlightSubject
{
    public class EdgesDrawingFinishedFlag {
        private enum FlagStatus {
            waiting  = 0,
            drawingFinished = 2,
            highlightingFinished = 3,
            unhighlightingFinished = 4
        }

        private FlagStatus flag;

        public void InitUnhighlightingFinished() {flag = FlagStatus.unhighlightingFinished;}
        public void InitDrawingFinishedFlag() {flag = FlagStatus.drawingFinished;}
        public void InitWaitingFlag() {flag = FlagStatus.waiting;}
        public void IncrementFlag() {
            flag++;
            if (flag > FlagStatus.unhighlightingFinished)
            {
                Debug.LogErrorFormat("Flag value higher than max! {0}", flag);
            }
        }
        public bool IsDrawingFinished() {return flag == FlagStatus.drawingFinished;}
        public bool IsHighlightingFinished() {return flag == FlagStatus.highlightingFinished;}
        public bool IsUnhighlightingFinished() {return flag == FlagStatus.unhighlightingFinished;}
    }

    public MethodInvocationInfo InvocationInfo {get; set;}
    public EdgesDrawingFinishedFlag finishedFlag;

    public EdgeHighlightSubject()
    {
        EdgeHighlightObserver o = new EdgeHighlightObserver(this);
        this.Attach(o);
        finishedFlag = new EdgesDrawingFinishedFlag();
        finishedFlag.InitUnhighlightingFinished();
    }

}

}
