using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OALProgramControl
{
    public class EXECommandFileExists : EXECommand
    {
        public EXEASTNodeAccessChain AssignmentTarget { get; }
        public EXEASTNodeBase FileToCheck { get; }

        public EXECommandFileExists(EXEASTNodeAccessChain assignmentTarget, EXEASTNodeBase fileToReadFrom)
        {
            this.AssignmentTarget = assignmentTarget;
            this.FileToCheck = fileToReadFrom;
        }

        protected override EXEExecutionResult Execute(OALProgram OALProgram)
        {
            EXEExecutionResult evaluationResultOfAssignmentTarget
                = this.AssignmentTarget.Evaluate
                (
                    this.SuperScope, OALProgram,
                    new EXEASTNodeAccessChainContext()
                    {
                        CreateVariableIfItDoesNotExist = true,
                        VariableCreationType = EXETypes.BooleanTypeName
                    }
                );

            if (!HandleRepeatableASTEvaluation(evaluationResultOfAssignmentTarget))
            {
                return evaluationResultOfAssignmentTarget;
            }

            EXEExecutionResult FileToCheckEvaluation = FileToCheck.Evaluate(this.SuperScope, OALProgram);
            if (!HandleRepeatableASTEvaluation(FileToCheckEvaluation))
            {
                return FileToCheckEvaluation;
            }

            if (FileToCheckEvaluation.ReturnedOutput is not EXEValueString)
            {
                return EXEExecutionResult.Error("XEC2047", string.Format("Tried to write value '{0}' of type '{1}' to a file. The value needs to be a string.", FileToCheckEvaluation.ReturnedOutput.ToObjectDiagramText(), FileToCheckEvaluation.ReturnedOutput.TypeName));
            }

            string fileToCheck = (FileToCheckEvaluation.ReturnedOutput as EXEValueString).Value;

            bool fileExists = false;
            try
            {
                fileExists = File.Exists(fileToCheck);
            }
            catch (Exception e)
            {
                string errorMessage = string.Format("Failed read from file '{0}', because of {1}:'{2}'", fileToCheck, e.GetType().Name, e.Message);
                return EXEExecutionResult.Error("XEC2048", errorMessage);
            }

            EXEValueBool fileCheckValue = new EXEValueBool(fileExists);

            EXEExecutionResult assignmentResult
                = evaluationResultOfAssignmentTarget
                    .ReturnedOutput
                    .AssignValueFrom(fileCheckValue);

            if (!HandleSingleShotASTEvaluation(assignmentResult))
            {
                return assignmentResult;
            }

            return Success();
        }

        protected override EXECommand CreateCloneCustom()
        {
            return new EXECommandFileExists(AssignmentTarget, FileToCheck);
        }

        public override void Accept(Visitor v)
        {
            v.VisitExeCommandFileExists(this);
        }
    }
}