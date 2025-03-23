using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnimArch.Visualization.Diagrams;
using Assets.Scripts.AnimationControl.OAL;
using OALProgramControl;
using TMPro;
using UMSAGL.Scripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI.Extensions;
using Visualisation.Animation;
using Visualization.ClassDiagram;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.ComponentsInDiagram;
using Visualization.ClassDiagram.Diagrams;
using Visualization.ClassDiagram.Relations;
using Visualization.UI;
using AnimArch.Extensions;
using UnityEngine.AI;

namespace Visualization.Animation
{
    //Controls the entire animation process
    public class Animation : Singleton<Animation>
    {
        public ClassDiagram.Diagrams.ClassDiagram classDiagram { get; private set;}
        public ObjectDiagram objectDiagram { get; private set;}
        public ActivityDiagram activityDiagram { get; set; }
        public Color classColor;
        public Color methodColor;
        public Color relationColor;
        public GameObject LineFill;
        public HighlightEdgeState edgeHighlighter;
        [HideInInspector] public bool AnimationIsRunning = false;
        [HideInInspector] public bool isPaused = false;
        [HideInInspector] public bool standardPlayMode = true;
        public bool nextStep = false;
        private bool prevStep = false;

        private List<GameObject> Fillers;
        public ConsoleScheduler consoleScheduler;
        private AnimationScheduler highlightScheduler;

        private ActivityInDiagram lastDecisionNode;
        private ActivityInDiagram lastMergeNode;
        private ActivityInDiagram finalActivity;
        private String activityRelationLabel = "";
        private EXECommand lastCommand;

        public string startClassName;
        public string startMethodName;
        public Dictionary<string, List<EXEVariable>> startMethodParameters = new Dictionary<string, List<EXEVariable>>();

        public const float AnimationSpeedCoefficient = 0.2f;

        [HideInInspector] private OALProgram currentProgramInstance = new OALProgram();
        [HideInInspector] public OALProgram CurrentProgramInstance { get { return currentProgramInstance; } }

        public bool SuperSpeed = false;

        private void Awake()
        {
            classDiagram = GameObject.Find("ClassDiagram").GetComponent<ClassDiagram.Diagrams.ClassDiagram>();
            objectDiagram = GameObject.Find("ObjectDiagram").GetComponent<ObjectDiagram>();
            activityDiagram = GameObject.Find("ActivityDiagram").GetComponent<ActivityDiagram>();
            standardPlayMode = true;
            edgeHighlighter = HighlightImmediateState.GetInstance();
        }

        private void ParseAnimationMethods()
        {
            Anim selectedAnimation = AnimationData.Instance.selectedAnim;

            List<AnimClass> MethodsCodes = selectedAnimation.GetMethodsCodesList(); //Filip

            foreach (AnimClass classItem in MethodsCodes) //Filip
            {
                CDClass Class = CurrentProgramInstance.ExecutionSpace.getClassByName(classItem.Name);

                foreach (AnimMethod methodItem in classItem.Methods)
                {
                    CDMethod Method = Class.GetMethodByName(methodItem.Name);
                    if (Method == null)
                    {
                        continue;
                    }

                    EXEScopeMethod MethodBody = OALParserBridge.Parse(methodItem.Code);
                    Method.ExecutableCode = MethodBody;
                }
            }
        }

        private CDMethod FindInitialMethod()
        {
            CDClass startClass = CurrentProgramInstance.ExecutionSpace.getClassByName(startClassName);
            if (startClass == null)
            {
                AnimationIsRunning = false;
                isPaused = true;
                UI.MenuManager.Instance.ShowNotSelectedPanel("class");
                return null;
            }

            CDMethod startMethod = startClass.GetMethodByName(startMethodName);
            if (startMethod == null)
            {
                AnimationIsRunning = false;
                isPaused = true;
                UI.MenuManager.Instance.ShowNotSelectedPanel("method");
                return null;
            }

            return startMethod;
        }

        private EXEScopeMethod FindInitialMethodCode()
        {
            //najdeme startMethod z daneho class stringu a method stringu, ak startMethod.ExecutableCode je null tak return null alebo yield break
            EXEScopeMethod MethodExecutableCode = CurrentProgramInstance.ExecutionSpace.getClassByName(startClassName)
                .GetMethodByName(startMethodName).ExecutableCode;
            if (MethodExecutableCode == null)
            {
                Debug.Log("Warning, EXEScopeMethod of selected Method is null");
                return null;
            }

            CurrentProgramInstance.SuperScope = MethodExecutableCode; //StartMethod.ExecutableCode
            //OALProgram.Instance.SuperScope = OALParserBridge.Parse(Code); //Method.ExecutableCode dame namiesto OALParserBridge.Parse(Code) pre metodu ktora bude zacinat

            return MethodExecutableCode;
        }

        private CDClassInstance CreateInitialInstance(EXEScopeMethod MethodExecutableCode)
        {
            CDClassInstance startingInstance = MethodExecutableCode.MethodDefinition.OwningClass.CreateClassInstance();
            MethodExecutableCode.OwningObject = new EXEValueReference(startingInstance);

            return startingInstance;
        }

        private void AssignInitialVariables(EXEScopeMethod MethodExecutableCode)
        {
            MethodExecutableCode.InitializeVariables(startMethodParameters.ContainsKey(startMethodName) ?
                startMethodParameters[startMethodName] :
                new List<EXEVariable>());
        }

        private void HighlightInitialMethod(CDMethod startMethod, CDClassInstance startingInstance)
        {
            Fillers = new List<GameObject>();
            objectDiagram.ShowObject(AddObjectToDiagram(startingInstance));

            Class caller = classDiagram.FindClassByName(startClassName).ParsedClass;
            Method callerMethod = classDiagram.FindMethodByName(startClassName, startMethodName);

            MethodInvocationInfo CallerCall = MethodInvocationInfo.CreateCallerOnlyInstance(startMethod, startingInstance);
            MethodInvocationInfo CalledCall = MethodInvocationInfo.CreateCalledOnlyInstance(startMethod, startingInstance);
            assignCallInfoToAllHighlightSubjects(caller, callerMethod, null, CallerCall, CallerCall.CallerMethod);
            callerMethod.HighlightObjectSubject.InvocationInfo = CalledCall;

            caller.HighlightSubject.IncrementHighlightLevel();
            callerMethod.HighlightSubject.IncrementHighlightLevel();
            callerMethod.HighlightObjectSubject.IncrementHighlightLevel();
        }

        private void InitializeSchedulers()
        {
            consoleScheduler = new ConsoleScheduler();
            highlightScheduler = new AnimationScheduler();
            StartCoroutine(consoleScheduler.Start(this));
        }

        private void TerminateSchedulers()
        {
            consoleScheduler.Terminate();
            highlightScheduler.Terminate();
        }

        private void SetupAnimation(CDMethod startMethod, EXEScopeMethod MethodExecutableCode)
        {
            UI.MenuManager.Instance.HideErrorPanelOnStopButton();
            UI.MenuManager.Instance.RefreshSourceCodePanel(MethodExecutableCode);

            Debug.Log("Abt to execute program");

            CDClassInstance startingInstance = CreateInitialInstance(MethodExecutableCode);
            AssignInitialVariables(MethodExecutableCode);

            HighlightInitialMethod(startMethod, startingInstance);

            InitializeSchedulers();
        }

        private IEnumerator TeardownAnimation()
        {
            TerminateSchedulers();
            yield return new WaitUntil(() => highlightScheduler.IsOver());
            Debug.Log("Over");
        }

        // Main Couroutine for compiling the OAL of Animation script and then starting the visualisation of Animation
        public IEnumerator Animate()
        {
            if (AnimationIsRunning)
            {
                yield break;
            }

            AnimationIsRunning = true;

            EXEScopeMethod.CommandIDSeed = 1;
            ParseAnimationMethods();

            CDMethod startMethod = FindInitialMethod();
            if (startMethod == null)
            {
                yield break;
            }

            EXEScopeMethod MethodExecutableCode = FindInitialMethodCode();
            if (MethodExecutableCode == null)
            {
                yield break;
            }

            SetupAnimation(startMethod, MethodExecutableCode);

            AnimationThread SuperThread = new AnimationThread(currentProgramInstance.CommandStack, currentProgramInstance, this);
            yield return StartCoroutine(SuperThread.Start());

            yield return TeardownAnimation();
            AnimationIsRunning = false;
        }

        public IEnumerator AnimateCommand(EXECommand CurrentCommand, AnimationThread AnimationThread, bool Animate = true, bool AnimateNewObjects = true)
        {
            //VisitorCommandToString v = new VisitorCommandToString();
            //CurrentCommand.Accept(v);
            //Debug.Log($"[LR] CommandID: {CurrentCommand.CommandID}, {CurrentCommand.GetType().Name}, {v.GetCommandString()}");

            AnimationRequest request = AnimationRequestFactory.Create(CurrentCommand, AnimationThread, Animate, AnimateNewObjects);
            highlightScheduler.Enqueue(request);
            yield return new WaitUntil(() => request.IsDone());

            // Karin - Activity Diagram =>
            VisitorCommandToString visitor = new VisitorCommandToString();
            CurrentCommand.Accept(visitor);
            string commandCode = visitor.GetCommandString();
            Debug.LogErrorFormat("[Karin] ZACIATOK commandCode: {0}, type: {1}, id: {2}", commandCode, CurrentCommand.GetType(), CurrentCommand.CommandID);
            if (CurrentCommand.GetType() == typeof(EXEScopeMethod))
            {
                if (ActivityDiagramManager.Instance.ActivityDiagrams.Count() > 0)
                {
                    activityDiagram = new ActivityDiagram();
                    activityDiagram.CreateGraph();
                }
                int indentationLevelX = 0;
                int indentationLevelY = 0;
                ActivityInDiagram initialActivity = activityDiagram.AddInitialActivityInDiagram(CurrentCommand);
                ActivityInDiagram lastActivity = animateActivityInDiagram(CurrentCommand, indentationLevelX, indentationLevelY, initialActivity);
                finalActivity =  activityDiagram.AddFinalActivityInDiagram(lastActivity.IndentationLevelX, lastActivity.IndentationLevelY + 1);
                activityDiagram.AddRelation(lastActivity, finalActivity);
                activityDiagram.SaveDiagram();
            }
            else if (CurrentCommand.GetType() == typeof(EXECommandReturn))
            {
                finalActivity.Command = CurrentCommand;
                if (ActivityDiagramManager.Instance.ActivityDiagrams.Count() > 1)
                {
                    activityDiagram.ResetDiagram();
                    ActivityDiagramManager.Instance.ActivityDiagrams.Pop();
                }
                ActivityDiagramManager.Instance.PrintDiagamsInStack(); //TODOa
            }

            if (CurrentCommand.GetType() != typeof(EXEScope)) // takto sa zafarbi aj initial node 
            {
                if (CurrentCommand.GetType() == typeof(EXEScopeForEach) || CurrentCommand.GetType() == typeof(EXEScopeLoopWhile))
                {
                    Debug.LogFormat("[Karin] EXEScopeForEach || EXEScopeLoopWhile");
                    unhighlightActivitiesForCommand(CurrentCommand);
                    // unhiglightRelations(CurrentCommand); //TODOa asi spravit unhighlight all relations
                }
                if (lastCommand != null && lastCommand.GetType() == typeof(EXEScopeCondition) && CurrentCommand.SuperScope != lastCommand.SuperScope)
                {
                    Debug.LogFormat($"[Karin] predosly bol EXEScopeCondition a ine SuperScope, current scope= {CurrentCommand.SuperScope}, last scope= {lastCommand.SuperScope}");
                    if (((EXEScopeCondition)lastCommand).ElifScopes.Count > 0 && ((EXEScopeCondition)lastCommand).ElifScopes.Contains(CurrentCommand) || ((EXEScopeCondition)lastCommand).ElseScope.Commands.Contains(CurrentCommand))
                    {
                        Debug.LogFormat("[Karin] ElifScopes contains current command");
                        foreach (EXECommand elifCommand in ((EXEScopeCondition)lastCommand).ElifScopes)
                        {
                            List<ActivityInDiagram> elifActivities = activityDiagram.GetActivitiesInDiagram(elifCommand);
                            if (elifActivities == null || elifActivities.Count == 0)
                            {
                                continue;
                            }
                            Debug.LogFormat("[Karin] highlight elif decision");
                            ActivityInDiagram activity = elifActivities.Find(a => a.ActivityType == ActivityType.Decision);
                            highlightActivity(activity);
                        }   
                    }

                    List<ActivityInDiagram> activities = activityDiagram.GetActivitiesInDiagram(lastCommand);
                    Debug.LogFormat("[Karin] highlight posledneho mergu");
                    if (activities != null && activities.Count > 0)
                    {
                        ActivityInDiagram activity = activities.Find(a => a.ActivityType == ActivityType.Merge);
                        highlightActivity(activity);
                    }

                }
                highlightActivitiesForCommand(CurrentCommand);
                // highlightRelations(CurrentCommand);
            }

            lastCommand = CurrentCommand;
            // dalsia vetva ak exe command return a existuje dalsi command 
            // <= Karin - Activity Diagram

            yield return new WaitUntil(() => !isPaused);
        }

        private ActivityInDiagram animateActivityInDiagram(EXECommand originalCommand, int indentationLevelX, int indentationLevelY, ActivityInDiagram lastActivity)
        {
            VisitorCommandToString v = new VisitorCommandToString();
            originalCommand.Accept(v);
            string cc = v.GetCommandString();
            Debug.LogErrorFormat("[Karin] originalCommand type: {0}, command text: {1}", originalCommand.GetType(), cc);
            if (originalCommand.GetType() != typeof(EXEScopeMethod) && originalCommand.IsDirectlyInCode)
            {
                // Debug.Log("[Karin] branch 1");
                if (originalCommand.GetType() == typeof(EXEScopeForEach))
                {
                    // Debug.Log("[Karin] EXEScopeForEach branch 1");
                    lastActivity = animateActivityInDiagram((EXEScopeForEach)originalCommand, indentationLevelX, indentationLevelY, lastActivity);
                }
                else if (originalCommand.GetType() == typeof(EXEScopeCondition))
                {
                    // Debug.Log("[Karin] EXEScopeCondition branch 1");
                    lastActivity = animateActivityInDiagram((EXEScopeCondition)originalCommand, indentationLevelX, indentationLevelY, lastActivity);
                }
                else if (originalCommand.GetType() == typeof(EXEScopeLoopWhile))
                {
                    // Debug.Log("[Karin] EXEScopeLoopWhile branch 1");
                    lastActivity = animateActivityInDiagram((EXEScopeLoopWhile)originalCommand, indentationLevelX, indentationLevelY, lastActivity);
                }
                else
                {
                    // Debug.Log("[Karin] else branch 1"); 
                    VisitorCommandToString visitor = new VisitorCommandToString();
                    originalCommand.Accept(visitor);
                    string commandCode = visitor.GetCommandString();
                    ActivityInDiagram activity = activityDiagram.AddActivityInDiagram(commandCode, indentationLevelX, indentationLevelY, originalCommand);
                    activityDiagram.AddRelation(lastActivity, activity, activityRelationLabel);
                    activityRelationLabel = "";
                    lastActivity = activity;
                }
            }
            else if (originalCommand.GetType() == typeof(EXEScopeMethod)) 
            {
                // Debug.Log("[Karin] branch 2");
                EXEScopeMethod methodScope = (EXEScopeMethod)originalCommand;
                foreach (EXECommand command in methodScope.Commands)
                {
                    lastDecisionNode = null;
                    lastMergeNode = null;
                    if (command.GetType() == typeof(EXEScopeMethod))
                    {
                        // Debug.Log("[Karin] EXEScopeMethod branch 2");
                        lastActivity = animateActivityInDiagram(command, indentationLevelX, indentationLevelY, lastActivity);
                    }
                    else if (command.GetType() == typeof(EXEScopeForEach))
                    {
                        // Debug.Log("[Karin] EXEScopeForEach branch 2");
                        lastActivity = animateActivityInDiagram((EXEScopeForEach)command, indentationLevelX, lastActivity.IndentationLevelY + 1, lastActivity);
                    }
                    else if (command.GetType() == typeof(EXEScopeLoopWhile))
                    {
                        // Debug.Log("[Karin] EXEScopeLoopWhile branch 2");
                        lastActivity =  animateActivityInDiagram((EXEScopeLoopWhile)command, indentationLevelX, lastActivity.IndentationLevelY + 1, lastActivity);
                    }
                    else if (command.GetType() == typeof(EXEScopeCondition))
                    {
                        // Debug.Log("[Karin] EXEScopeCondition branch 2");
                        lastActivity = animateActivityInDiagram((EXEScopeCondition)command, indentationLevelX, lastActivity.IndentationLevelY + 1, lastActivity);
                    }
                    else {
                        // Debug.Log("[Karin] else branch 2");
                        lastActivity = animateActivityInDiagram(command, indentationLevelX, lastActivity.IndentationLevelY + 1, lastActivity);
                    }
                }
            }
            return lastActivity;
        }

        private ActivityInDiagram animateActivityInDiagram(EXEScopeForEach forEachCommand, int indentationLevelX, int indentationLevelY, ActivityInDiagram lastActivity)
        {
            // Debug.Log("[Karin] Animate command code: forEach");
            ActivityInDiagram mergeActivity = activityDiagram.AddMergeActivityInDiagram(indentationLevelX, indentationLevelY, forEachCommand);
            ActivityInDiagram decisionActivity = activityDiagram.AddDecisionActivityInDiagram(indentationLevelX, indentationLevelY + 1, forEachCommand);
            activityDiagram.AddRelation(lastActivity, mergeActivity);
            activityDiagram.AddRelation(mergeActivity, decisionActivity, "another " + forEachCommand.IteratorName);
            lastActivity = decisionActivity;

            this.activityRelationLabel = "[yes]";
            foreach (EXECommand command1 in forEachCommand.Commands)
            {
                lastActivity = animateActivityInDiagram(command1, indentationLevelX + 1, lastActivity.IndentationLevelY + 1, lastActivity);
            }
            this.activityRelationLabel = "[no]";
            activityDiagram.AddRelation(lastActivity, mergeActivity);
            return decisionActivity;
        }

        private ActivityInDiagram animateActivityInDiagram(EXEScopeLoopWhile whileCommand, int indentationLevelX, int indentationLevelY, ActivityInDiagram lastActivity)
        {
            // Debug.Log("[Karin] Animate command code: while");
            VisitorCommandToString visitor = new VisitorCommandToString();
            whileCommand.Condition.Accept(visitor);
            string condition = visitor.GetCommandString();

            ActivityInDiagram mergeActivity = activityDiagram.AddMergeActivityInDiagram(indentationLevelX, indentationLevelY, whileCommand);
            ActivityInDiagram decisionActivity = activityDiagram.AddDecisionActivityInDiagram(indentationLevelX, indentationLevelY + 1, whileCommand);
            activityDiagram.AddRelation(lastActivity, mergeActivity);
            activityDiagram.AddRelation(mergeActivity, decisionActivity);
            lastActivity = decisionActivity;
            this.activityRelationLabel = "while " + condition;
            foreach (EXECommand command1 in whileCommand.Commands)
            {
                lastActivity = animateActivityInDiagram(command1, indentationLevelX + 1, lastActivity.IndentationLevelY + 1, lastActivity);
            }
            activityDiagram.AddRelation(lastActivity, mergeActivity);
            this.activityRelationLabel = "else";
            return decisionActivity; 
        }

        private ActivityInDiagram animateActivityInDiagram(EXEScopeCondition conditionCommand, int indentationLevelX, int indentationLevelY, ActivityInDiagram lastActivity)
        {
            Debug.LogError("[Karin] Animate command code: condition");

            VisitorCommandToString v = new VisitorCommandToString();
            conditionCommand.Accept(v);
            string cc = v.GetCommandString();
            Debug.LogErrorFormat("[Karin] originalCommand type: {0}, command text: {1}", conditionCommand.GetType(), cc);

            ActivityInDiagram decisionNode = activityDiagram.AddDecisionActivityInDiagram(indentationLevelX, indentationLevelY, conditionCommand);
            activityDiagram.AddRelation(lastActivity, decisionNode, this.activityRelationLabel);
            this.lastDecisionNode = decisionNode;
            lastActivity = decisionNode;

            VisitorCommandToString visitor = new VisitorCommandToString();
            conditionCommand.Condition.Accept(visitor);
            this.activityRelationLabel = "[" + visitor.GetCommandString() + "]";

            List<ActivityInDiagram> elifMergeNodes = new List<ActivityInDiagram>();

            foreach (EXECommand ifBranch in conditionCommand.Commands)
            {
                Debug.Log("[Karin] Animate command code: if");
                lastActivity = animateActivityInDiagram(ifBranch, indentationLevelX, lastActivity.IndentationLevelY + 1, lastActivity);
            }
            ActivityInDiagram lastActivityIf = lastActivity;
            ActivityInDiagram lastActivityElse = null;
            if (conditionCommand.ElifScopes.Count > 0)
            {
                foreach (EXEScopeCondition elifBranch in conditionCommand.ElifScopes)
                {
                    this.activityRelationLabel = "else";
                    Debug.Log($"[Karin] {cc} Animate command code: elif");
                    lastActivity = animateActivityInDiagram(elifBranch, ++indentationLevelX, this.lastDecisionNode.IndentationLevelY + 1, this.lastDecisionNode);
                    elifMergeNodes.Add(lastActivity);
                }
            }
            if (conditionCommand.ElseScope != null)
            {
                this.activityRelationLabel = "else";
                lastActivity = this.lastDecisionNode;
                foreach (EXECommand elseBranch in conditionCommand.ElseScope.Commands)
                {
                    Debug.Log("[Karin] Animate command code: elseScope");
                    lastActivity = animateActivityInDiagram(elseBranch, this.lastDecisionNode.IndentationLevelX + 1, lastActivity.IndentationLevelY + 1, lastActivity);
                }
                lastActivityElse = lastActivity;
            }    
            ActivityInDiagram mergeNode = activityDiagram.AddMergeActivityInDiagram(decisionNode.IndentationLevelX, lastActivity.IndentationLevelY + 1, conditionCommand);
            if (lastActivityElse != null && lastActivityElse != this.lastMergeNode)
            {
                if (this.lastMergeNode != null)
                {
                    Debug.Log("[Karin] Add relation from last activity in else branch to main merge node");
                    activityDiagram.AddRelation(lastActivityElse, this.lastMergeNode); // add relation from last activity in else branch to main merge node
                }
                else 
                {
                    Debug.Log("[Karin] Add relation from last activity in else branch to main merge node");
                    activityDiagram.AddRelation(lastActivityElse, mergeNode); // add relation from last activity in else branch to merge node
                }
            }
            this.lastMergeNode = mergeNode;
            if (lastActivity.ActivityType == ActivityType.Merge)
            {
                Debug.Log("[Karin] Add relation from previous merge node to merge node");
                activityDiagram.AddRelation(lastActivity, this.lastMergeNode); // add relation from previous merge node to merge node
            }
            Debug.Log("[Karin] Add relation from last activity in else branch to main merge node");
            activityDiagram.AddRelation(lastActivityIf, this.lastMergeNode); // add relation from last activity in if branch to merge node
            
            if (lastActivityElse != null)
            {
                Debug.Log("[Karin] Add relation from last activity in else branch to main merge node");
                foreach (ActivityInDiagram elifMergeNode in elifMergeNodes) // add relations from merge nodes in elif branches to main merge node
                {
                    activityDiagram.AddRelation(elifMergeNode, this.lastMergeNode);
                }
            }

            return mergeNode;
        }

        private void highlightActivitiesForCommand(EXECommand command)
        {
            Debug.LogFormat("[Karin] som v metode highlightActivityForCommand; command type = {0}, id = {1} ", command.GetType(), command.CommandID);
            List<ActivityInDiagram> activities = activityDiagram.GetActivitiesInDiagram(command);
            if (activities == null || activities.Count == 0)
            {
                Debug.LogError("[Karin] No activities found to highlight.");
                return;
            }

            if (command.GetType() == typeof(EXEScopeCondition))
            {
                highlightActivity(activities[0]);
            }
            else
            {
                foreach (ActivityInDiagram activity in activities)
                {
                    // Debug.LogFormat("[Karin] Nasla sa activity pre command: {0}, A.Text: {1}, A.Type: {2}, A.CommandId: {3}", command.GetType(), activity.ActivityText, activity.ActivityType, activity.Command.CommandID);
                    highlightActivity(activity);
                }
            }
        }

        private void highlightActivity(ActivityInDiagram activity)
        {
            GameObject activityGo = activity.VisualObject;
            if (activity.ActivityType == ActivityType.Classic)
            {
                Debug.Log("[Karin] Highlight activity classic");
                activityGo.GetComponent<BackgroundHighlighter>().HighlightBackground();
            }
            else
            {
                Debug.Log("[Karin] Highlight activity decision / merge / initial / final");
                activityGo.GetComponent<BackgroundHighlighter>().HighlightBackground(relationColor);
            }
            activity.IsHighlighted = true;
        }

        // private void unhighlightActivities(EXECommand command)
        // {
        //     Debug.Log("[Karin] Unhighlight activity");
        //     List<ActivityInDiagram> activities = activityDiagram.GetActivitiesInDiagram(command);
        //     if (activities == null || activities.Count == 0)
        //     {
        //         Debug.LogError("[Karin] No activities found to unhighlight.");
        //         return;
        //     }
        //     foreach (ActivityInDiagram activity in activities)
        //     {
        //         if (activity.IsHighlighted)
        //         {
        //             GameObject activityGo = activity.VisualObject;
        //             if (activity.ActivityType == ActivityType.Classic)
        //             {
        //                 Debug.Log("[Karin] Unhighlight activity: " + activity.ActivityText);
        //                 activityGo.GetComponent<BackgroundHighlighter>().UnhighlightBackground();
        //             }
        //             else
        //             {
        //                 Debug.Log("[Karin] Unhighlight activity decision / merge / initial / final");
        //                 activityGo.GetComponent<BackgroundHighlighter>().UnhighlightBackground(Color.black);
        //             }
        //             activity.IsHighlighted = false;
        //         }
        //     }
        // }

        private void unhighlightActivitiesForCommand(EXECommand command)
        {
            Debug.Log("[Karin] unhighlight all activities for command: " + command);
            if (command is EXEScope scope) 
            {
                if (command is EXEScopeCondition condition)
                {
                    foreach (EXECommand subCommand in condition.ElifScopes)
                    {
                        unhighlightActivitiesForCommand(subCommand);
                    }
                    if (condition.ElseScope != null)
                    {
                        unhighlightActivitiesForCommand(condition.ElseScope);
                    }
                }
                foreach (EXECommand subCommand in scope.Commands)
                {
                    unhighlightActivitiesForCommand(subCommand);
                }
            }
            List<ActivityInDiagram> activities = activityDiagram.GetActivitiesInDiagram(command);
            if (activities == null || activities.Count == 0)
            {
                Debug.LogError("[Karin] No activities found to unhighlight.");
                return;
            }
            foreach (ActivityInDiagram activity in activities)
            {
                if (activity.IsHighlighted)
                {
                    unhighlightActivity(activity);
                }
            }
        }

        private void unhighlightActivity(ActivityInDiagram activity)
        {
            GameObject activityGo = activity.VisualObject;
            if (activity.ActivityType == ActivityType.Classic)
            {
                Debug.Log("[Karin] Unhighlight activity classic " + activity.ActivityText);
                activityGo.GetComponent<BackgroundHighlighter>().UnhighlightBackground();
            }
            else
            {
                Debug.Log("[Karin] Unhighlight activity decision / merge / initial / final");
                activityGo.GetComponent<BackgroundHighlighter>().UnhighlightBackground(Color.black);
            }
            activity.IsHighlighted = false;
        }

        private void highlightRelations(EXECommand command)
        {
            Debug.Log("[Karin] Highlight relations from command: " + command);
            List<ActivityRelation> relations = activityDiagram.GetActivityRelations(command);
            foreach (ActivityRelation relation in relations)
            {
                Debug.Log("[Karin] Highlight relation: " + relation);
                GameObject edge = relation.VisualObject;
                if (edge != null)
                {
                    edge.GetComponent<UEdge>().ChangeColor(relationColor);
                    edge.GetComponent<UILineRenderer>().LineThickness = 8;
                }
            }
        }

        private void unhiglightRelations(EXECommand command)
        {
            Debug.Log("[Karin] Unhighlight relations from command: " + command);
            List<ActivityRelation> relations = activityDiagram.GetActivityRelations(command);
            foreach (ActivityRelation relation in relations)
            {
                Debug.Log("[Karin] Unhighlight relation: " + relation);
                GameObject edge = relation.VisualObject;
                if (edge != null)
                {
                    edge.GetComponent<UEdge>().ChangeColor(Color.black);
                    edge.GetComponent<UILineRenderer>().LineThickness = 4;
                }
            }
        }

        public ObjectInDiagram AddObjectToDiagram(CDClassInstance newObject, string name = null, bool showNewObject = true)
        {
            ObjectInDiagram objectInDiagram = objectDiagram.AddObjectInDiagram(name, newObject, showNewObject);
            return objectInDiagram;
        }
        
        private IEnumerator ResolveCreateObject(EXECommand currentCommand, bool Animate = true, bool AnimateNewObjects = true)
        {
            EXECommandQueryCreate createCommand = (EXECommandQueryCreate)currentCommand;

            CDClassInstance callerObject = (currentCommand.GetCurrentMethodScope().OwningObject as EXEValueReference).ClassInstance;
            CDClassInstance createdObject = createCommand.GetCreatedInstance();

            string targetVariableName = null;
            if (createCommand.AssignmentTarget != null)
            {
                VisitorCommandToString visitor = new VisitorCommandToString();
                createCommand.AssignmentTarget.Accept(visitor);
                targetVariableName = visitor.GetCommandString();
            }

            if (AnimateNewObjects)
            {

                

                var objectInDiagram = AddObjectToDiagram(createdObject, targetVariableName);
                var relation = FindInterGraphRelation(createdObject.UniqueID);

                if (!Animate)
                {
                    objectDiagram.ShowObject(objectInDiagram);
                    objectDiagram.AddRelation(callerObject, createdObject, "ASSOCIATION");
                }
                else
                {
                    #region Object creation animation

                    int step = 0;
                    float speedPerAnim = AnimationData.Instance.AnimSpeed;
                    float timeModifier = 1f;
                    while (step < 7)
                    {
                        switch (step)
                        {
                            case 0:
                                HighlightClass(createdObject.OwningClass.Name, true);
                                break;
                            case 1:
                                // yield return StartCoroutine(AnimateFillInterGraph(relation));
                                timeModifier = 0f;
                                break;
                            case 3:
                                // relation.Show();
                                // relation.Highlight();
                                timeModifier = 1f;
                                break;
                            case 2:
                                objectDiagram.ShowObject(objectInDiagram);
                                timeModifier = 0.5f;
                                break;
                            case 6:
                                HighlightClass(createdObject.OwningClass.Name, false);
                                relation.UnHighlight();
                                timeModifier = 1f;
                                break;
                        }

                        step++;
                        if (standardPlayMode)
                        {
                            yield return new WaitForSeconds(AnimationData.Instance.AnimSpeed * timeModifier);
                        }
                        //Else means we are working with step animation
                        else
                        {
                            if (step == 1) step = 2;
                            nextStep = false;
                            prevStep = false;
                            yield return new WaitUntil(() => nextStep);
                            if (prevStep)
                            {
                                if (step > 0) step--;
                                step = UnhighlightObjectCreationStepAnimation(step, createdObject.OwningClass.Name, objectInDiagram);

                                if (step > -1) step--;
                                step = UnhighlightObjectCreationStepAnimation(step, createdObject.OwningClass.Name, objectInDiagram);
                            }

                            yield return new WaitForFixedUpdate();
                            nextStep = false;
                            prevStep = false;
                        }
                    }

                    #endregion

                    objectDiagram.AddRelation(callerObject, createdObject, "ASSOCIATION");
                }
            }
            else
            {
                AddObjectToDiagram(createdObject, targetVariableName, false);
            }
        }

        private IEnumerator AnimateFillInterGraph(InterGraphRelation relation)
        {
            relation.Animate(AnimationData.Instance.AnimSpeed * 20);
            yield return new WaitForSeconds(AnimationData.Instance.AnimSpeed);
        }

        public static InterGraphRelation FindInterGraphRelation(long instanceId)
        {
            InterGraphRelation relation = null;
            foreach (var interGraphRelation in DiagramPool.Instance.RelationsClassToObject)
            {
                if (interGraphRelation.Object.Instance.UniqueID == instanceId)
                {
                    relation = interGraphRelation;
                }
            }

            return relation;
        }

        public int UnhighlightObjectCreationStepAnimation(int step, string className, ObjectInDiagram od)
        {
            if (step == 1) step = 2;
            switch (step)
            {
                case 0:
                    HighlightClass(className, false);
                    break;
                case 2:
                    //relation.UnHighlight();
                    break;
                case 3:
                    break;
            }

            return step;
        }

        public void StartAnimation()
        {
            isPaused = false;
            StartCoroutine("Animate");
        }

        //Couroutine that can be used to Highlight class for a given duration of time
        public IEnumerator AnimateClass(string className, float animationLength)
        {
            HighlightClass(className, true);
            yield return new WaitForSeconds(animationLength);
            HighlightClass(className, false);
        }

        public IEnumerator AnimateObject(long objectId, float animationLength)
        {
            HighlightObject(objectId, true);
            yield return new WaitForSeconds(animationLength);
            HighlightObject(objectId, false);
        }

        //Couroutine that can be used to Highlight method for a given duration of time
        public IEnumerator AnimateMethod(CDMethod method, float animationLength)
        {
            HighlightMethod(method, true);
            yield return new WaitForSeconds(animationLength);
            HighlightMethod(method, false);
        }

        //Couroutine that can be used to Highlight edge for a given duration of time
        public IEnumerator AnimateEdge(string relationshipName, float animationLength, MethodInvocationInfo call)
        {
            HighlightEdge(relationshipName, true, call);
            yield return new WaitForSeconds(animationLength);
            HighlightEdge(relationshipName, false, call);
        }

        public void RunAnimateFill(MethodInvocationInfo Call)
        {
            StartCoroutine(AnimateFill(Call));
        }

        public IEnumerator AnimateFill(MethodInvocationInfo Call)
        {
            RelationInDiagram relationInDiagram = classDiagram.FindEdgeInfo(Call.Relation?.RelationshipName);
            GameObject edge = relationInDiagram?.VisualObject;

            if (edge != null)
            {
                EdgeHighlightSubject.EdgesDrawingFinishedFlag finishedFlag = classDiagram.FindEdgeInfo(Call.Relation.RelationshipName).HighlightSubject.finishedFlag;
                if (edge.CompareTag("Generalization") || edge.CompareTag("Implements") ||
                    edge.CompareTag("Realisation"))
                {
                    finishedFlag.InitDrawingFinishedFlag();
                    HighlightEdge(Call.Relation.RelationshipName, true, Call);
                    yield return new WaitForSeconds(AnimationData.Instance.AnimSpeed / 2);
                }
                else
                {
                    yield return FillNewFiller(classDiagram.FindOwnerOfRelation(Call.Relation.RelationshipName),
                        Call.CalledMethod.OwningClass.Name, edge, Call, finishedFlag);
                }
            }
        }

        private object FillNewFiller(string ownerOfRelation, string calledClassName, GameObject edge, MethodInvocationInfo Call, EdgeHighlightSubject.EdgesDrawingFinishedFlag finishedEdges)
        {
            GameObject newFiller = Instantiate(LineFill);
            Fillers.Add(newFiller);

            newFiller.transform.position = edge.transform.position;
            newFiller.transform.SetParent(edge.transform);
            newFiller.transform.localScale = new Vector3(1, 1, 1);

            LineFiller lf = newFiller.GetComponent<LineFiller>();
            bool flip = ownerOfRelation.Equals(calledClassName);

            var callerInstance = Call.CallerObject;
            var objectRelation = DiagramPool.Instance.ObjectDiagram.FindRelation(callerInstance.UniqueID, Call.CalledObject.UniqueID);
            GameObject newFiller1 = Instantiate(LineFill);
            Fillers.Add(newFiller1);

            newFiller1.transform.position = objectRelation.GameObject.transform.position;
            newFiller1.transform.SetParent(objectRelation.GameObject.transform);
            newFiller1.transform.localScale = new Vector3(1, 1, 1);

            LineFiller lf1 = newFiller1.GetComponent<LineFiller>();


            Func<bool> highlightEdgeCallback = () => {
                finishedEdges.IncrementFlag();
                if (finishedEdges.IsDrawingFinished())
                {
                    HighlightEdge(Call.Relation.RelationshipName, true, Call);
                    Destroy(lf1.gameObject);
                    Destroy(lf.gameObject);
                }
                return false;
            };


            lf1.StartCoroutine(lf1.AnimateFlow(objectRelation.GameObject.GetComponent<UILineRenderer>().Points, false, highlightEdgeCallback, true));
            return lf.StartCoroutine(lf.AnimateFlow(edge.GetComponent<UILineRenderer>().Points, flip, highlightEdgeCallback, false));
        }

        private GameObject classGameObject(string className)
        {
            if (!UI.UIEditorManager.Instance.isNetworkDisabledOrIsServer())
            {
                var objects = NetworkManager.Singleton.SpawnManager.SpawnedObjects;
                var values = objects.Values;
                foreach (var value in values)
                {
                    if (value.name == className)
                        return value.gameObject;
                }
            }
            return classDiagram.FindNode(className);
        }

        private void HighlightBackground(BackgroundHighlighter backgroundHighlighter, bool isToBeHighlighted)
        {
            if (isToBeHighlighted)
                backgroundHighlighter.HighlightBackground();
            else
                backgroundHighlighter.UnhighlightBackground();
        }

        //Method used to Highlight/Unhighlight single class by name, depending on bool value of argument 
        public void HighlightClass(string className, bool isToBeHighlighted, long instanceID = -1)
        {
            GameObject node = classGameObject(className);

            BackgroundHighlighter backgroundHighlighter = null;
            if (node != null)
            {
                backgroundHighlighter = node.GetComponent<BackgroundHighlighter>();
            }
            else
            {
                Debug.Log("Node " + className + " not found");
                return;
            }
            HighlightBackground(backgroundHighlighter, isToBeHighlighted);
        }

        public void HighlightObjects(MethodInvocationInfo call, bool isToBeHighlighted)
        {
            ClassInDiagram classByName = classDiagram.FindClassByName(call.CallerMethod.OwningClass.Name);

            if (classByName == null)
            {
                Debug.Log("Node " + call.CallerMethod.OwningClass.Name + " not found");
            }

            if (classByName != null)
            {
                foreach (var classInfoInstance in classByName.ClassInfo.Instances)
                {
                    long id = classInfoInstance.UniqueID;
                    HighlightObject(id, isToBeHighlighted);
                }
            }
            else
            {
                Debug.Log("Highlighter component not found");
            }
        }

        //Method used to Highlight/Unhighlight single class by name, depending on bool value of argument 
        public void HighlightObject(long objectUniqueId, bool isToBeHighlighted)
        {
            if (!DiagramPool.Instance.ObjectDiagram.ObjectExists(objectUniqueId))
            {
                return;
            }

            GameObject node = objectDiagram.FindByID(objectUniqueId).VisualObject;
            BackgroundHighlighter backgroundHighlighter = null;
            if (node != null)
            {
                backgroundHighlighter = node.GetComponent<BackgroundHighlighter>();
            }
            else
            {
                Debug.Log("Node " + objectUniqueId + " not found");
                return;
            }

            HighlightBackground(backgroundHighlighter, isToBeHighlighted);
        }

        //Method used to Highlight/Unhighlight single method by name, depending on bool value of argument 
        public void HighlightMethod(Class _class, Method method, bool isToBeHighlighted)
        {
            HighlightMethod(_class.Name, method.Name, isToBeHighlighted);
        }
        public void HighlightMethod(CDMethod method, bool isToBeHighlighted)
        {
            HighlightMethod(method.OwningClass.Name, method.Name, isToBeHighlighted);
        }
        public void HighlightMethod(string className, string methodName, bool isToBeHighlighted)
        {
            var node = classDiagram.FindNode(className);
            if (node)
            {
                ClassTextHighligter classTextHighligter = node.GetComponent<ClassTextHighligter>();
                if (classTextHighligter)
                {
                    if (isToBeHighlighted)
                    {
                        if (DateTime.Now.IsJune()) {
                            classTextHighligter.HighlightClassNameLine();
                        }
                        classTextHighligter.HighlightClassLine(methodName);
                    }
                    else
                    {
                        if (DateTime.Now.IsJune()) {
                            classTextHighligter.UnhighlightClassNameLine();
                        }
                        classTextHighligter.UnhighlightClassLine(methodName);
                    }
                }
                else
                {
                    Debug.LogError("TextHighlighter component not found");
                }
            }
            else
            {
                Debug.LogError("Node " + className + " not found");
            }
        }

        public void HighlightInstancesMethod(MethodInvocationInfo call, bool isToBeHighlighted)
        {
            foreach (CDClassInstance cdClassInstance in call.CallerMethod.OwningClass.Instances)
            {
                HighlightObjectMethod(call.CallerMethod.Name, cdClassInstance.UniqueID, isToBeHighlighted);
            }
        }

        public void HighlightObjectMethod(string methodName, long cdClassInstanceId, bool isToBeHighlighted)
        {
            if (!DiagramPool.Instance.ObjectDiagram.ObjectExists(cdClassInstanceId))
            {
                return;
            }

            var textHighlighter = objectDiagram.FindByID(cdClassInstanceId).VisualObject
                .GetComponent<ObjectTextHighlighter>();
            if (textHighlighter != null)
            {
                if (isToBeHighlighted)
                    textHighlighter.HighlightObjectLine(methodName);
                else
                    textHighlighter.UnHighlightObjectLine(methodName);
            }
        }

        //Method used to Highlight/Unhighlight single edge by name, depending on bool value of argument 
        public void HighlightEdge(string relationshipName, bool isToBeHighlighted, MethodInvocationInfo Call)
        {
            RelationInDiagram relationInDiagram = classDiagram.FindEdgeInfo(relationshipName);

            GameObject edge = relationInDiagram?.VisualObject;

            var callerClassInDiagram =
                DiagramPool.Instance.ClassDiagram.FindClassByName(relationInDiagram?.RelationInfo.FromClass);
            var calledClassInDiagram =
                DiagramPool.Instance.ClassDiagram.FindClassByName(relationInDiagram?.RelationInfo.ToClass);

            if (edge != null)
            {
                if (isToBeHighlighted)
                {
                    edge.GetComponent<UEdge>().ChangeColor(relationColor);
                    edge.GetComponent<UILineRenderer>().LineThickness = 8;
                    HighlightInstancesRelations(Call, callerClassInDiagram, calledClassInDiagram, true);
                }
                else
                {
                    edge.GetComponent<UEdge>().ChangeColor(Color.white);
                    edge.GetComponent<UILineRenderer>().LineThickness = 5;
                    HighlightInstancesRelations(Call, callerClassInDiagram, calledClassInDiagram, false);
                }
            }
            else
            {
                Debug.Log(relationshipName + " NULL Edge ");
            }
        }

        private void HighlightInstancesRelations(MethodInvocationInfo Call, ClassInDiagram callerClassName,
                    ClassInDiagram calledClassName, bool isToBeHighlighted)
        {
            if (Call == null) // unhighlight all
            {
                foreach (var callerInstance in callerClassName.ClassInfo.Instances)
                {
                    foreach (var calledInstance in calledClassName.ClassInfo.Instances)
                    {
                        HighlightObjectRelation(callerInstance.UniqueID, calledInstance.UniqueID, false);
                    }
                }
            }
            else
            {
                var classInDiagram = DiagramPool.Instance.ClassDiagram.FindClassByName(Call.CallerMethod.OwningClass.Name);
                foreach (var callerInstance in classInDiagram.ClassInfo.Instances)
                {
                    HighlightObjectRelation(callerInstance.UniqueID, Call.CalledObject.UniqueID, isToBeHighlighted);
                }
            }
        }

        private void HighlightObjectRelation(long callerInstanceId, long calledInstanceId, bool isToBeHighlighted)
        {
            if (callerInstanceId == calledInstanceId)
            {
                return;
            }

            if (DiagramPool.Instance.ObjectDiagram == null)
                return;

            var objectRelation = DiagramPool.Instance.ObjectDiagram.FindRelation(callerInstanceId, calledInstanceId);
            
            if (objectRelation == null)
                return;
            
            var objectRelationGo = objectRelation.GameObject;

            if (isToBeHighlighted)
            {
                objectRelationGo.GetComponent<UEdge>().ChangeColor(relationColor);
                objectRelationGo.GetComponent<UILineRenderer>().LineThickness = 8;
            }
            else
            {
                objectRelationGo.GetComponent<UEdge>().ChangeColor(Color.white);
                objectRelationGo.GetComponent<UILineRenderer>().LineThickness = 5;
            }
        }

        public static void assignCallInfoToAllHighlightSubjects(Class c, Method m, RelationInDiagram relation, MethodInvocationInfo Call, CDMethod method) {
            c.HighlightSubject.ClassName = method.OwningClass.Name;
            m.HighlightSubject.MethodName = method.Name;
            m.HighlightSubject.ClassName = method.OwningClass.Name;
            m.HighlightObjectSubject.InvocationInfo = Call;
            if (relation != null)
            {
                relation.HighlightSubject.InvocationInfo = Call;
            }
        }

        private int UnhighlightAllStepAnimation(MethodInvocationInfo Call, int step)
        {
            if (step == 2) step = 1;
            switch (step)
            {
                case 0:
                    HighlightClass(Call.CallerMethod.OwningClass.Name, false);
                    HighlightObjects(Call, false);
                    break;
                case 1:
                    HighlightMethod(Call.CallerMethod, false);
                    HighlightInstancesMethod(Call, false);
                    break;
                case 3:
                    HighlightEdge(Call.Relation.RelationshipName, false, Call);
                    break;
                case 4:
                    HighlightClass(Call.CalledMethod.OwningClass.Name, false, Call.CalledObject.UniqueID);
                    HighlightObject(Call.CalledObject.UniqueID, false);
                    break;
                case 5:
                    HighlightMethod(Call.CalledMethod, false);
                    HighlightObjectMethod(Call.CalledMethod.Name, Call.CalledObject.UniqueID, false);
                    break;
            }

            return step;
        }

        public string GetColorCode(string type)
        {
            if (type == "class")
            {
                return ColorUtility.ToHtmlStringRGB(classColor);
            }

            if (type == "method")
            {
                return ColorUtility.ToHtmlStringRGB(methodColor);
            }

            if (type == "relation")
            {
                return ColorUtility.ToHtmlStringRGB(relationColor);
            }

            return "";
        }

        //Method used to stop all animations and unhighlight all objects
        public void UnhighlightAll()
        {
            isPaused = false;
            StopAllCoroutines();
            if (DiagramPool.Instance.ClassDiagram.GetClassList() != null)
                foreach (Class c in DiagramPool.Instance.ClassDiagram.GetClassList())
                {
                    HighlightClass(c.Name, false);
                    if (c.Methods != null)
                    {
                        foreach (Method m in c.Methods)
                        {
                            HighlightMethod(c, m, false);
                        }
                    }
                }

            if (DiagramPool.Instance.ClassDiagram.GetRelationList() != null)
                foreach (Relation r in DiagramPool.Instance.ClassDiagram.GetRelationList())
                {
                    HighlightEdge(r.OALName, false, null);
                }

            AnimationIsRunning = false;
        }

        public void Pause()
        {
            isPaused = !isPaused;
        }

        public void NextStep()
        {
            if (AnimationIsRunning == false)
                StartAnimation();
            else
                nextStep = true;
        }

        public void PrevStep()
        {
            nextStep = true;
            prevStep = true;
        }

        public void SetEdgeHighlighter(HighlightEdgeState newState)
        {
            edgeHighlighter = newState; 
        }

        public HighlightEdgeState GetEdgeHighlighter()
        {
            return edgeHighlighter;
        }
    }
}
