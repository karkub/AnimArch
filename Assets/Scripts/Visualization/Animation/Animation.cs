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
        public ClassDiagram.Diagrams.ClassDiagram classDiagram  {get; private set;}
        private ObjectDiagram objectDiagram;
        public Color classColor;
        public Color methodColor;
        public Color relationColor;
        public GameObject LineFill;
        private int BarrierSize;
        private int CurrentBarrierFill;
        public HighlightEdgeState edgeHighlighter;
        [HideInInspector] public bool AnimationIsRunning = false;
        [HideInInspector] public bool isPaused = false;
        [HideInInspector] public bool standardPlayMode = true;
        public bool nextStep = false;
        private bool prevStep = false;
        private List<GameObject> Fillers;
        private ConsoleScheduler consoleScheduler;

        public string startClassName;
        public string startMethodName;
        public Dictionary<string, List<EXEVariable>> startMethodParameters = new Dictionary<string, List<EXEVariable>>();

        private const float AnimationSpeedCoefficient = 0.2f;

        [HideInInspector] private OALProgram currentProgramInstance = new OALProgram();
        [HideInInspector] public OALProgram CurrentProgramInstance { get { return currentProgramInstance; } }

        public bool SuperSpeed = false;

        private void Awake()
        {
            classDiagram = GameObject.Find("ClassDiagram").GetComponent<ClassDiagram.Diagrams.ClassDiagram>();
            objectDiagram = GameObject.Find("ObjectDiagram").GetComponent<ObjectDiagram>();
            standardPlayMode = true;
            edgeHighlighter = HighlightImmediate.GetInstance();
        }


        // Main Couroutine for compiling the OAL of Animation script and then starting the visualisation of Animation
        public IEnumerator Animate()
        {
            Fillers = new List<GameObject>();

            if (AnimationIsRunning)
            {
                yield break;
            }

            AnimationIsRunning = true;

            UI.MenuManager.Instance.HideErrorPanelOnStopButton();

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

            CDClass startClass = CurrentProgramInstance.ExecutionSpace.getClassByName(startClassName);
            if (startClass == null)
            {
                AnimationIsRunning = false;
                isPaused = true;
                UI.MenuManager.Instance.ShowNotSelectedPanel("class");
                yield break;
            }

            CDMethod startMethod = startClass.GetMethodByName(startMethodName);
            if (startMethod == null)
            {
                AnimationIsRunning = false;
                isPaused = true;
                UI.MenuManager.Instance.ShowNotSelectedPanel("method");
                yield break;
            }

            //najdeme startMethod z daneho class stringu a method stringu, ak startMethod.ExecutableCode je null tak return null alebo yield break
            EXEScopeMethod MethodExecutableCode = CurrentProgramInstance.ExecutionSpace.getClassByName(startClassName)
                .GetMethodByName(startMethodName).ExecutableCode;
            if (MethodExecutableCode == null)
            {
                Debug.Log("Warning, EXEScopeMethod of selected Method is null");
                yield break;
            }

            CurrentProgramInstance.SuperScope = MethodExecutableCode; //StartMethod.ExecutableCode
            //OALProgram.Instance.SuperScope = OALParserBridge.Parse(Code); //Method.ExecutableCode dame namiesto OALParserBridge.Parse(Code) pre metodu ktora bude zacinat
            UI.MenuManager.Instance.RefreshSourceCodePanel(MethodExecutableCode);

            Debug.Log("Abt to execute program");

            string currentClassName = startClassName;
            string currentMethodName = startMethodName;

            CDClassInstance startingInstance = MethodExecutableCode.MethodDefinition.OwningClass.CreateClassInstance();
            MethodExecutableCode.OwningObject = new EXEValueReference(startingInstance);
            objectDiagram.ShowObject(AddObjectToDiagram(startingInstance));

            MethodExecutableCode.InitializeVariables(startMethodParameters.ContainsKey(startMethodName) ?
                startMethodParameters[startMethodName] :
                new List<EXEVariable>());

            Class caller = classDiagram.FindClassByName(startClassName).ParsedClass;
            Method callerMethod = classDiagram.FindMethodByName(startClassName, startMethodName);

            MethodInvocationInfo CallerCall = MethodInvocationInfo.CreateCallerOnlyInstance(startMethod, startingInstance);
            MethodInvocationInfo CalledCall = MethodInvocationInfo.CreateCalledOnlyInstance(startMethod, startingInstance);
            assignCallInfoToAllHighlightSubjects(caller, callerMethod, null, CallerCall, CallerCall.CallerMethod);
            callerMethod.HighlightObjectSubject.InvocationInfo = CalledCall;

            caller.HighlightSubject.IncrementHighlightLevel();
            callerMethod.HighlightSubject.IncrementHighlightLevel();
            callerMethod.HighlightObjectSubject.IncrementHighlightLevel();

            consoleScheduler = new ConsoleScheduler();
            StartCoroutine(consoleScheduler.Start(this));

            AnimationThread SuperThread = new AnimationThread(currentProgramInstance.CommandStack, currentProgramInstance, this);
            yield return StartCoroutine(SuperThread.Start());

            consoleScheduler.Terminate();
            Debug.Log("Over");
            AnimationIsRunning = false;
        }

        public IEnumerator AnimateCommand(EXECommand CurrentCommand, AnimationThread AnimationThread, bool Animate = true, bool AnimateNewObjects = true)
        {
            if (CurrentCommand.GetType() == typeof(EXECommandCall))
            {
                EXECommandCall exeCommandCall = (EXECommandCall)CurrentCommand;

                if (Animate)
                {
                    MethodInvocationInfo methodCallInfo = exeCommandCall.CallInfo;

                    if (methodCallInfo != null)
                    {
                        BarrierSize = 1;
                        CurrentBarrierFill = 0;

                        objectDiagram.AddRelation(methodCallInfo.CallerObject, methodCallInfo.CalledObject, "ASSOCIATION");

                        StartCoroutine(ResolveCallFunct(methodCallInfo));

                        yield return StartCoroutine(BarrierFillCheck());
                    }
                }

                UI.MenuManager.Instance.RefreshSourceCodePanel(exeCommandCall.InvokedMethod);
            }
            else if (CurrentCommand.GetType() == typeof(EXECommandReturn))
            {
                EXECommandReturn exeCommandReturn = (EXECommandReturn)CurrentCommand;

                if (Animate)
                {
                    EXEScopeMethod exeScopeMethod = exeCommandReturn.GetCurrentMethodScope();

                    if (exeScopeMethod != null)
                    {
                        CDMethod calledMethod = exeScopeMethod.MethodDefinition;
                        EXEScopeMethod exeScopeCaller = AnimationThread.CurrentMethod;
                        CDMethod callerMethod = exeScopeCaller?.MethodDefinition;

                        if
                        (
                            exeScopeCaller != null && callerMethod !=  null &&
                            exeScopeCaller.OwningObject != null && exeScopeCaller.OwningObject is EXEValueReference &&
                            exeScopeMethod.OwningObject != null && exeScopeMethod.OwningObject is EXEValueReference
                        )
                        {
                            CDClass caller = callerMethod.OwningClass;
                            CDClass called = calledMethod.OwningClass;
                            CDRelationship relation = CurrentProgramInstance.RelationshipSpace.GetRelationshipByClasses(caller.Name, called.Name);

                            CDClassInstance callerInstance = (exeScopeCaller.OwningObject as EXEValueReference).ClassInstance;
                            CDClassInstance calledInstance = (exeScopeMethod.OwningObject as EXEValueReference).ClassInstance;

                            StartCoroutine(ResolveReturn(new MethodInvocationInfo(callerMethod, calledMethod, relation, callerInstance, calledInstance)));
                        }
                        else if
                        (
                            exeScopeCaller == null &&
                            exeScopeMethod.OwningObject != null && exeScopeMethod.OwningObject is EXEValueReference
                        )
                        {
                            CDClassInstance calledInstance = (exeScopeMethod.OwningObject as EXEValueReference).ClassInstance;

                            StartCoroutine(ResolveReturn(MethodInvocationInfo.CreateCalledOnlyInstance(calledMethod, calledInstance)));
                        }
                    }
                }

                //UI.MenuManager.Instance.AnimateSourceCodeAtMethodStart(exeCommandReturn.InvokedMethod); // TODO -> should this happen?
            }
            else if (CurrentCommand.GetType() == typeof(EXECommandQueryCreate))
            {
                BarrierSize = 1;
                CurrentBarrierFill = 0;

                if (Animate)
                {
                    StartCoroutine(ResolveCreateObject(CurrentCommand, true, AnimateNewObjects));
                    yield return StartCoroutine(BarrierFillCheck());
                }
                else
                {
                    yield return ResolveCreateObject(CurrentCommand, false, AnimateNewObjects);
                }
            }
            else if (CurrentCommand.GetType() == typeof(EXECommandAssignment))
            {
                ResolveAssignment(CurrentCommand);
            }
            else if (CurrentCommand.GetType() == typeof(EXECommandAddingToList))
            {
                EXECommandAddingToList addingToList = (EXECommandAddingToList)CurrentCommand;
                CDClassInstance listOwnerInstance = addingToList.GetAssignmentTargetOwner();
                CDClassInstance appendedInstance = addingToList.GetAppendedElementInstance();

                if (listOwnerInstance != null)
                {
                    if (appendedInstance != null)
                    {
                        objectDiagram.AddRelation(listOwnerInstance, appendedInstance, "ASSOCIATION");
                        objectDiagram.UpdateAttributeValues(listOwnerInstance);
                    }
                    else
                    {
                        ResolveAssignment(listOwnerInstance);
                    }
                }
            }
            else if (CurrentCommand.GetType().Equals(typeof(EXECommandRead)))
            {
                EXECommandRead readCommand = CurrentCommand as EXECommandRead;

                ConsoleRequestRead consoleRequest = new ConsoleRequestRead(readCommand.PromptText);
                consoleScheduler.Enqueue(consoleRequest);
                yield return new WaitUntil(() => consoleRequest.Done);

                AnimationThread.ExecutionSuccess
                    = ((EXECommandRead)CurrentCommand).AssignReadValue(consoleRequest.ReadValue, CurrentProgramInstance);
            }
            else if (CurrentCommand.GetType().Equals(typeof(EXECommandWrite)))
            {
                EXECommandWrite readCommand = CurrentCommand as EXECommandWrite;

                ConsoleRequestWrite consoleRequest = new ConsoleRequestWrite(readCommand.PromptText);
                consoleScheduler.Enqueue(consoleRequest);
                yield return new WaitUntil(() => consoleRequest.Done);
            }
            else if (CurrentCommand.GetType().Equals(typeof(EXECommandWait)))
            {
                if (Animate)
                {
                    EXECommandWait waitCommand = CurrentCommand as EXECommandWait;

                    float secondsToWait;
                    if (waitCommand.WaitTime.EvaluationResult.ReturnedOutput is EXEValueReal)
                    {
                        EXEValueReal secondsToWaitValue = waitCommand.WaitTime.EvaluationResult.ReturnedOutput as EXEValueReal;
                        secondsToWait = (float)secondsToWaitValue.Value;
                    }
                    else if (waitCommand.WaitTime.EvaluationResult.ReturnedOutput is EXEValueInt)
                    {
                        EXEValueInt secondsToWaitValue = waitCommand.WaitTime.EvaluationResult.ReturnedOutput as EXEValueInt;
                        secondsToWait = (float)secondsToWaitValue.Value;
                    }
                    else
                    {
                        throw new Exception(string.Format("Tried to wait for some seconds. The value type is {0}", waitCommand.WaitTime.EvaluationResult.ReturnedOutput.TypeName));
                    }

                    yield return new WaitForSeconds(secondsToWait);
                }
            }
            else
            {
                if (Animate)
                {
                    float speedPerAnim = AnimationData.Instance.AnimSpeed;
                    yield return new WaitForSeconds(AnimationSpeedCoefficient * speedPerAnim);
                }
            }

            yield return new WaitUntil(() => !isPaused);
        }

        private void ResolveAssignment(EXECommand currentCommand)
        {
            EXECommandAssignment assignment = (EXECommandAssignment)currentCommand;
            CDClassInstance classInstance = assignment.GetAssignmentTargetOwner();
            ResolveAssignment(classInstance);
        }
        private void ResolveAssignment(CDClassInstance classInstance)
        {
            if (classInstance == null) return;

            objectDiagram.UpdateAttributeValues(classInstance);
        }
        private ObjectInDiagram AddObjectToDiagram(CDClassInstance newObject, string name = null, bool showNewObject = true)
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
                VisitorCommandToString visitor = VisitorCommandToString.BorrowAVisitor();
                createCommand.AssignmentTarget.Accept(visitor);
                targetVariableName = visitor.GetCommandStringAndResetStateNow();
            }

            if (AnimateNewObjects)
            {

                

                var objectInDiagram = AddObjectToDiagram(createdObject, targetVariableName);
                var relation = FindInterGraphRelation(createdObject.UniqueID);
                IEnumerable<RelationInDiagram> relationsOfClass = classDiagram.FindRelationsByClass(createdObject.OwningClass.Name);

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
                    foreach (RelationInDiagram rel in relationsOfClass)
                    {
                        yield return new WaitUntil(() => rel.HighlightSubject.finishedFlag.IsUnhighlightingFinished());
                    }
                    Class highlightedClass = classDiagram.FindClassByName(createdObject.OwningClass.Name).ParsedClass;
                    highlightedClass.HighlightSubject.ClassName = highlightedClass.Name;
                    while (step < 7)
                    {
                        switch (step)
                        {
                            case 0:
                                highlightedClass.HighlightSubject.IncrementHighlightLevel();
                                //HighlightClass(createdObject.OwningClass.Name, true);
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
                                highlightedClass.HighlightSubject.DecrementHighlightLevel();
                                //HighlightClass(createdObject.OwningClass.Name, false);
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
                
            IncrementBarrier();
        }

        private IEnumerator AnimateFillInterGraph(InterGraphRelation relation)
        {
            relation.Animate(AnimationData.Instance.AnimSpeed * 20);
            yield return new WaitForSeconds(AnimationData.Instance.AnimSpeed);
        }

        private static InterGraphRelation FindInterGraphRelation(long instanceId)
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

        private int UnhighlightObjectCreationStepAnimation(int step, string className, ObjectInDiagram od)
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

        public void IncrementBarrier()
        {
            this.CurrentBarrierFill++;
        }

        public IEnumerator BarrierFillCheck()
        {
            yield return new WaitUntil(() => CurrentBarrierFill >= BarrierSize);
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

        private void assignCallInfoToAllHighlightSubjects(Class c, Method m, RelationInDiagram relation, MethodInvocationInfo Call, CDMethod method) {
            c.HighlightSubject.ClassName = method.OwningClass.Name;
            m.HighlightSubject.MethodName = method.Name;
            m.HighlightSubject.ClassName = method.OwningClass.Name;
            m.HighlightObjectSubject.InvocationInfo = Call;
            if (relation != null)
            {
                relation.HighlightSubject.InvocationInfo = Call;
            }
        }

        // Couroutine used to Resolve one OALCall consisting of Caller class, caller method, edge, called class, called method
        // Same coroutine is called for play or step mode
        public IEnumerator ResolveCallFunct(MethodInvocationInfo Call)
        {
            Debug.Log(Call.ToString());

            Class called = classDiagram.FindClassByName(Call.CalledMethod.OwningClass.Name).ParsedClass;
            Method calledMethod = classDiagram.FindMethodByName(Call.CalledMethod.OwningClass.Name, Call.CalledMethod.Name);
            RelationInDiagram relation = classDiagram.FindEdgeInfo(Call.Relation?.RelationshipName);

            assignCallInfoToAllHighlightSubjects(called, calledMethod, relation, Call, Call.CalledMethod);

            if (relation != null)
            {
                yield return new WaitUntil(() => relation.HighlightSubject.finishedFlag.IsUnhighlightingFinished());
                relation?.HighlightSubject.IncrementHighlightLevel();
                yield return new WaitUntil(() => relation.HighlightSubject.finishedFlag.IsDrawingFinished());
            }
            calledMethod.HighlightObjectSubject.IncrementHighlightLevel();
            called.HighlightSubject.IncrementHighlightLevel();
            calledMethod.HighlightSubject.IncrementHighlightLevel();
            yield return new WaitForSeconds(AnimationData.Instance.AnimSpeed * 1.25f);
            if (relation != null)
            {
                relation.HighlightSubject.finishedFlag.IncrementFlag();
            }

            IncrementBarrier();
        }

        public IEnumerator ResolveReturn(MethodInvocationInfo callInfo)
        {
            float timeModifier = 1f;

            Class called = classDiagram.FindClassByName(callInfo.CalledMethod.OwningClass.Name).ParsedClass;
            Method calledMethod = classDiagram.FindMethodByName(callInfo.CalledMethod.OwningClass.Name, callInfo.CalledMethod.Name);
            RelationInDiagram relation = classDiagram.FindEdgeInfo(callInfo.Relation?.RelationshipName);
            assignCallInfoToAllHighlightSubjects(called, calledMethod, relation, callInfo, callInfo.CalledMethod);


            if (relation != null)
            {
                yield return new WaitUntil(() => relation.HighlightSubject.finishedFlag.IsHighlightingFinished());
            }

            calledMethod.HighlightSubject.DecrementHighlightLevel();
            calledMethod.HighlightObjectSubject.DecrementHighlightLevel();
            called.HighlightSubject.DecrementHighlightLevel();
            relation?.HighlightSubject.DecrementHighlightLevel();

            if (relation != null)
            {
                relation.HighlightSubject.finishedFlag.IncrementFlag();
            }

            if (standardPlayMode)
            {
                yield return new WaitForSeconds(AnimationData.Instance.AnimSpeed * timeModifier);
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
    }
}
