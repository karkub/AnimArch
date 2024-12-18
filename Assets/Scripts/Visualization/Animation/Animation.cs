﻿using System;
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
        public bool isEXECommandReturn = false;

        private List<GameObject> Fillers;
        public ConsoleScheduler consoleScheduler;
        private AnimationScheduler highlightScheduler;

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
            AnimationRequest request = AnimationRequestFactory.Create(CurrentCommand, AnimationThread, Animate, AnimateNewObjects);
            highlightScheduler.Enqueue(request);
            yield return new WaitUntil(() => request.IsDone());

            // Karin - Activity Diagram =>
            VisitorCommandToString visitor = new VisitorCommandToString();
            CurrentCommand.Accept(visitor);
            string commandCode = visitor.GetCommandString();
            
            if (CurrentCommand.GetType() == typeof(EXEScopeMethod))
            {
                if (activityDiagram.Activities.Count > 0)
                {
                    activityDiagram.SaveDiagram();
                    activityDiagram.ClearDiagram();
                }
                Debug.Log("[Karin] EXEScopeMethod vo vonkajsej");
                int indentationLevelX = 0;
                int indentationLevelY = 0;
                animateActivityInDiagram(CurrentCommand, indentationLevelX, indentationLevelY);
                activityDiagram.AddFinalActivityInDiagram();
                activityDiagram.AddRelations();
            }
            // <= Karin - Activity Diagram

            yield return new WaitUntil(() => !isPaused);
        }

        private void animateActivityInDiagram(EXECommand originalCommand, int indentationLevelX, int indentationLevelY)
        {
            Debug.LogFormat("[Karin] animateActivityInDiagram originalCommand type: {0}", originalCommand.GetType());

            if (originalCommand.GetType() != typeof(EXEScopeMethod) && originalCommand.IsDirectlyInCode)
            {
                if (isEXECommandReturn)
                {
                    Debug.Log("[Karin] isEXECommandReturn v rekurzii");
                    activityDiagram.ResetDiagram();
                    activityDiagram = ActivityDiagramManager.Instance.ActivityDiagrams.Pop();
                    activityDiagram.LoadDiagram();
                    isEXECommandReturn = false;
                }
                if (originalCommand.GetType() == typeof(EXEScopeForEach))
                {
                    animateActivityInDiagram((EXEScopeForEach)originalCommand, indentationLevelX, indentationLevelY);
                    
                }
                else if (originalCommand.GetType() == typeof(EXEScopeCondition))
                {
                    animateActivityInDiagram((EXEScopeCondition)originalCommand, indentationLevelX, indentationLevelY);
                }
                else
                {
                    VisitorCommandToString visitor = new VisitorCommandToString();
                    originalCommand.Accept(visitor);
                    string commandCode = visitor.GetCommandString();
                    Debug.LogFormat("[Karin] AddActivityInDiagram commandCode : {0}", commandCode);
                    activityDiagram.AddActivityInDiagram(commandCode, indentationLevelX, indentationLevelY);
                }
            }
            else if (originalCommand.GetType() == typeof(EXEScopeMethod)) 
            {
                EXEScopeMethod methodScope = (EXEScopeMethod)originalCommand;
                foreach (EXECommand command in methodScope.Commands)
                {                    
                    if (command.GetType() == typeof(EXEScopeMethod))
                    {
                        Debug.Log("[Karin] command.GetType() == typeof(EXEScopeMethod)");
                        animateActivityInDiagram(command, indentationLevelX, indentationLevelY);
                    }
                    else if (command.GetType() == typeof(EXEScopeForEach))
                    {
                        Debug.Log("[Karin] command.GetType() == typeof(EXEScopeForEach)");
                        animateActivityInDiagram((EXEScopeForEach)command, indentationLevelX, indentationLevelY + 1);
                        indentationLevelY += 2;
                    }
                    else if (command.GetType() == typeof(EXEScopeCondition))
                    {
                        Debug.Log("[Karin] command.GetType() == typeof(EXEScopeCondition)");
                        animateActivityInDiagram((EXEScopeCondition)command, indentationLevelX, ++indentationLevelY);
                    } 
                    else {
                        animateActivityInDiagram(command, indentationLevelX, ++indentationLevelY);
                    }
                }
            }
        }

        private void animateActivityInDiagram(EXEScopeForEach forEachScope, int indentationLevelX, int indentationLevelY)
        {
            activityDiagram.AddDecisionActivityInDiagram(indentationLevelX, indentationLevelY, ActivityType.Loop, forEachScope.IteratorName);
            activityDiagram.AddDecisionActivityInDiagram(indentationLevelX, indentationLevelY + 1, ActivityType.LoopDecision);
            indentationLevelY += 1;
            int indentForEach = 0;
            foreach (EXECommand command1 in forEachScope.Commands)
            {
                // indentForEach += 1;
                animateActivityInDiagram(command1, indentationLevelX + 1, indentationLevelY + indentForEach);
                indentForEach += 1;
            }
        }

        private void animateActivityInDiagram(EXEScopeCondition scopeCondition, int indentationLevelX, int indentationLevelY)
        {
            VisitorCommandToString visitor = new VisitorCommandToString();
            scopeCondition.Condition.Accept(visitor);
            string condition = visitor.GetCommandString();

            activityDiagram.AddDecisionActivityInDiagram(indentationLevelX, indentationLevelY, ActivityType.Decision, condition);
            int indentIf = 0;
            foreach (EXECommand ifScope in scopeCondition.Commands)
            {
                Debug.Log("[Karin] Animate command code: if");
                indentIf += 1;
                animateActivityInDiagram(ifScope, indentationLevelX, indentationLevelY + indentIf);
            }
            int indentElif = 0;
            foreach (EXEScopeCondition elifScope in scopeCondition.ElifScopes)
            {
                Debug.Log("[Karin] Animate command code: elif");
                indentElif += 1;
                animateActivityInDiagram(elifScope, indentationLevelX + 1, indentationLevelY + indentElif);
            }
            int indentElse = 0;
            if (scopeCondition.ElseScope != null)
            {
                foreach (EXECommand elseScope in scopeCondition.ElseScope.Commands)
                {
                    Debug.Log("[Karin] Animate command code: elseScope");
                    indentElse += 1;
                    animateActivityInDiagram(elseScope, indentationLevelX + 1, indentationLevelY + indentElse); //TODOa asi nebudu dobre tie indentations
                }
            }
            activityDiagram.AddDecisionActivityInDiagram(indentationLevelX, indentationLevelY + Math.Max(indentIf, indentElse) + 1, ActivityType.Merge);
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
