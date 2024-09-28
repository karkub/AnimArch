using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OALProgramControl
{
    public class EXECommandFileRead : EXECommand
    {
        public EXEASTNodeAccessChain AssignmentTarget { get; }
        public EXEASTNodeBase FileToReadFrom { get; }

        public EXECommandFileRead(EXEASTNodeAccessChain assignmentTarget, EXEASTNodeBase fileToReadFrom)
        {
            this.AssignmentTarget = assignmentTarget;
            this.FileToReadFrom = fileToReadFrom;
        }

        protected override EXEExecutionResult Execute(OALProgram OALProgram)
        {
            EXEExecutionResult evaluationResultOfAssignmentTarget
                = this.AssignmentTarget.Evaluate
                (
                    this.SuperScope,
                    OALProgram,
                    new EXEASTNodeAccessChainContext()
                    {
                        CreateVariableIfItDoesNotExist = true,
                        VariableCreationType = EXETypes.StringTypeName
                    }
                );

            if (!HandleRepeatableASTEvaluation(evaluationResultOfAssignmentTarget))
            {
                return evaluationResultOfAssignmentTarget;
            }

            EXEExecutionResult FileToReadFromEvaluation = FileToReadFrom.Evaluate(this.SuperScope, OALProgram);
            if (!HandleRepeatableASTEvaluation(FileToReadFromEvaluation))
            {
                return FileToReadFromEvaluation;
            }

            if (FileToReadFromEvaluation.ReturnedOutput is not EXEValueString)
            {
                return EXEExecutionResult.Error("XEC2045", string.Format("Tried to write value '{0}' of type '{1}' to a file. The value needs to be a string.", FileToReadFromEvaluation.ReturnedOutput.ToObjectDiagramText(), FileToReadFromEvaluation.ReturnedOutput.TypeName));
            }

            string fileToReadFrom = (FileToReadFromEvaluation.ReturnedOutput as EXEValueString).Value;

            string readText = string.Empty;
            try
            {
                using (StreamReader readtext = new StreamReader(fileToReadFrom))
                {
                    readText = readtext.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                string errorMessage = string.Format("Failed read from file '{0}', because of {1}:'{2}'", fileToReadFrom, e.GetType().Name, e.Message);
                return EXEExecutionResult.Error("XEC2046", errorMessage);
            }

            EXEValueString readTextValue = new EXEValueString(string.Format("\"{0}\"", readText));

            EXEExecutionResult assignmentResult
                = evaluationResultOfAssignmentTarget
                    .ReturnedOutput
                    .AssignValueFrom(readTextValue);

            if (!HandleSingleShotASTEvaluation(assignmentResult))
            {
                return assignmentResult;
            }

            return Success();
        }

        public override EXECommand CreateClone()
        {
            return new EXECommandFileRead(AssignmentTarget, FileToReadFrom);
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeCommandFileRead(this);
        }
    }
}