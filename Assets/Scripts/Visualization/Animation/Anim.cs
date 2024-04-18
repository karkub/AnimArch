//Data structure for single animation

using OALProgramControl;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AnimArch.Visualization.Diagrams;
using UnityEngine;
using Assets.Scripts.AnimationControl.OAL;
using Visualization.Animation;
using Visualization.ClassDiagram;
using Visualization.ClassDiagram.Relations;
using Assets.Scripts.AnimationControl;
using Microsoft.Msagl.Core.DataStructures;
using System;

namespace Visualisation.Animation
{
    [System.Serializable]
    public struct Anim
    {
        [SerializeField]
        public string AnimationName; //{ set; get; }
        [SerializeField]
        private List<AnimClass> MethodsCodes;
        public Anim(string animation_name)
        {
            AnimationName = animation_name;
            MethodsCodes = new List<AnimClass>();
        }

        public void Initialize()
        {
            List<CDClass> ClassPool = Visualization.Animation.Animation.Instance.CurrentProgramInstance.ExecutionSpace.Classes;

            if (ClassPool.Any())
            {
                List<Relation> Relations = DiagramPool.Instance.ClassDiagram.GetRelationList().Where(r => ("Generalization".Equals(r.PropertiesEaType) || "Realisation".Equals(r.PropertiesEaType))).ToList();//

                string SuperClass;
                Relation Relation;
                List<string> Attributes;
                List<AnimMethod> Methods;
                List<string> Parameters;

                foreach (CDClass ClassItem in ClassPool)
                {
                    SuperClass = "";
                    Relation = Relations.FirstOrDefault(r => r.FromClass.Equals(ClassItem.Name));
                    if (Relation != null)
                    {
                        SuperClass = Relation.ToClass;
                    }
                    Attributes = ClassItem.GetAttributes().Select(a => a.Name).ToList();

                    Methods = new List<AnimMethod>();
                    foreach (CDMethod MethodItem in ClassItem.GetMethods())
                    {
                        Parameters = MethodItem.Parameters.Select(p => p.Name).ToList();
                        Methods.Add(new AnimMethod(MethodItem.Name, Parameters, ""));
                    }

                    MethodsCodes.Add(new AnimClass(ClassItem.Name, SuperClass, Attributes, Methods));
                }
            }
        }

        public void SetMethodCode(string className, string methodName, string code)
        {
            int index = methodName.IndexOf("(");
            if (index >= 0)
            {
                methodName = methodName.Substring(0, index); // remove "(...)" from method name
            }

            AnimClass classItem = MethodsCodes.FirstOrDefault(c => c.Name.Equals(className));   //alebo SingleOrDefault
            if (classItem != null)
            {
                AnimMethod methodItem = classItem.Methods.FirstOrDefault(m => m.Name.Equals(methodName));  //alebo SingleOrDefault
                if (methodItem != null)
                {
                    if (string.IsNullOrWhiteSpace(code))
                    {
                        methodItem.Code = "";

                        CDMethod Method = Visualization.Animation.Animation.Instance.CurrentProgramInstance.ExecutionSpace.getClassByName(className).GetMethodByName(methodName);
                        Method.ExecutableCode = null;
                    }
                    else
                    {
                        methodItem.Code = code;
                    }
                }
            }
        }

        public string GetMethodBody(string className, string methodName)
        {
            int index = methodName.IndexOf("(");
            if (index >= 0)
            {
                methodName = methodName.Substring(0, index); // remove "(...)" from method name
            }

            AnimClass classItem = MethodsCodes.FirstOrDefault(c => c.Name.Equals(className));   //alebo SingleOrDefault
            if (classItem != null)
            {
                AnimMethod methodItem = classItem.Methods.FirstOrDefault(m => m.Name.Equals(methodName));  //alebo SingleOrDefault
                if (methodItem != null)
                {
                    return methodItem.Code;
                }
            }
            return "";  // className or methodName does not exist
        }

        public List<AnimClass> GetMethodsCodesList()
        {
            return MethodsCodes;
        }

        // Return Methods that have a code
        public List<AnimMethod> GetMethodsByClassName(string className)
        {
            List<AnimMethod> Methods = null;
            AnimClass classItem = MethodsCodes.FirstOrDefault(c => c.Name.Equals(className));   //alebo SingleOrDefault

            if (classItem != null)
            {
                Methods = new List<AnimMethod>();

                foreach (AnimMethod methodItem in classItem.Methods)
                {
                    if (!string.IsNullOrEmpty(methodItem.Code))
                    {
                        Methods.Add(methodItem);
                    }
                }
            }
            return Methods;
        }

        public void SaveCode(string path)
        {
            string text = JsonUtility.ToJson(this);
            File.WriteAllText(path, text);
        }

        public void LoadCode(string path)
        {
            string text = File.ReadAllText(path);
            Anim anim = JsonUtility.FromJson<Anim>(text);
            MethodsCodes = anim.GetMethodsCodesList();
        }

        private void ClassToPython(StringBuilder Code, AnimClass classItem) {
            if (string.Empty.Equals(classItem.SuperClass))
                {
                    Code.AppendLine("class " + classItem.Name + ":");
                }
                else
                {
                    Code.AppendLine("class " + classItem.Name + "(" + classItem.SuperClass + "):");
                }
                Code.AppendLine("\t" + "instances = []");
                Code.AppendLine();

                AnimMethod constructor = classItem.Methods.FirstOrDefault(m => m.Name.Equals(classItem.Name));  //alebo SingleOrDefault
                if (constructor == null)
                {
                    Code.AppendLine("\t" + "def __init__(self):");

                    foreach (string attributeName in classItem.Attributes)
                    {
                        Code.AppendLine("\t\t" + "self." + attributeName + " = None");
                    }
                }
                else
                {
                    Code.Append("\t" + "def __init__(self");

                    foreach (string parameterName in constructor.Parameters)
                    {
                        Code.Append(", " + parameterName);
                    }
                    Code.AppendLine("):");

                    foreach (string attributeName in classItem.Attributes)
                    {
                        Code.AppendLine("\t\t" + "self." + attributeName + " = None");
                    }

                    if (!string.Empty.Equals(constructor.Code))
                    {
                        VisitorPythonCode visitor = VisitorPythonCode.BorrowAVisitor();
                        EXEScopeMethod _scope = OALParserBridge.Parse(constructor.Code);
                        visitor.SetIndentation(2);
                        _scope.Accept(visitor);
                        string result = visitor.GetCommandStringAndResetStateNow();
                        Code.AppendLine(result);
                    }

                    classItem.Methods.Remove(constructor);
                }
                Code.AppendLine("\t\t" + classItem.Name + ".instances.append(self)");
                Code.AppendLine();

                foreach (AnimMethod methodItem in classItem.Methods)
                {
                    Code.Append("\t" + "def " + methodItem.Name);

                    if (methodItem.Parameters.Any())
                    {
                        Code.AppendLine("(self, " + string.Join(", ", methodItem.Parameters) + "):");
                    }
                    else
                    {
                        Code.AppendLine("(self):");
                    }

                    if (string.Empty.Equals(methodItem.Code))
                    {
                        Code.AppendLine("\t\t" + "pass");
                        Code.AppendLine();
                    }
                    else
                    {
                        VisitorPythonCode visitor = VisitorPythonCode.BorrowAVisitor();
                        EXEScopeMethod _scope = OALParserBridge.Parse(methodItem.Code);
                        visitor.SetIndentation(2);
                        _scope.Accept(visitor);
                        string result = visitor.GetCommandStringAndResetStateNow();
                        Code.AppendLine(result);
                    }
            }
        }

        public string GeneratePythonCode()
        {
            StringBuilder Code = new StringBuilder();
            Code.AppendLine("import time");
            Code.AppendLine("from time import sleep");
            Code.AppendLine("from pathlib import Path");
            Code.AppendLine("from threading import Thread");
            Code.AppendLine();

            OALProgram currentProgram = Visualization.Animation.Animation.Instance.CurrentProgramInstance;
            List<CDClass> classes = currentProgram.ExecutionSpace.Classes.Where(_class => _class.SuperClass == null).ToList();

            while (classes.Any())
            {
                List<CDClass> nextClasses = new List<CDClass>();
                foreach (CDClass classCD in classes) 
                {
                    AnimClass classAnim = MethodsCodes.Where(_class => _class.Name.Equals(classCD.Name)).ToList().FirstOrDefault();

                    if (classAnim == null)
                    {
                        string className = classCD.Name;
                        string availableMethodClasses = string.Join(",", MethodsCodes.Select(_class => _class.Name));
                        throw new Exception(string.Format("No code found for class '{0}' in list of method codes. Available method codes classes are: {1}.", className, availableMethodClasses));
                    }

                    ClassToPython(Code, classAnim);
                    nextClasses.AddRange(currentProgram.ExecutionSpace.Classes.Where(_class => _class.SuperClass == classCD).ToList());
                }
                
                classes = nextClasses;
            }

            Code.AppendLine("def boolean(value):");
            Code.AppendLine("\t" + "if value == \"True\":");
            Code.AppendLine("\t\t" + "return True");
            Code.AppendLine("\t" + "elif value == \"False\":");
            Code.AppendLine("\t\t" + "return False");
            Code.AppendLine("\t" + "raise ValueError(\"could not convert string to boolean: '\" + value + \"'\")");
            Code.AppendLine();

            Code.AppendLine("def cardinality(variable):");
            Code.AppendLine("\t" + "if isinstance(variable, list):");
            Code.AppendLine("\t\t" + "return len(variable)");
            Code.AppendLine("\t" + "elif hasattr(variable, '__dict__'):");
            Code.AppendLine("\t\t" + "return 1");
            Code.AppendLine("\t" + "else:");
            Code.AppendLine("\t\t" + "return 0");
            Code.AppendLine("def contains(array, value):");
            Code.AppendLine("\t" + "return len([x for x in array if x.Equals(value)]) > 0");
            Code.AppendLine();

            return Code.ToString();
        }
    }
}