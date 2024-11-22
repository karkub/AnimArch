using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OALProgramControl
{
    public class EXEExecutionResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public EXECommand OwningCommand { get; set; }

        // When a command is being executed, sometimes it will require another command to be executed first (e.g. when its parameter is a method call).
        // Such "Pending" command is to be stored here.
        public EXECommand PendingCommand { get; set; }
        public EXEValueBase ReturnedOutput { get; set; }
        public bool IsDone
        {
            get
            {
                return PendingCommand == null;
            }
        }

        private EXEExecutionResult(bool success, EXECommand owningCommand) : this(success, string.Empty, string.Empty, owningCommand) { }
        private EXEExecutionResult(bool success, string errorMessage, string errorCode, EXECommand owningCommand)
        {
            this.IsSuccess = success;
            this.ErrorMessage = errorMessage;
            this.ErrorCode = errorCode;
            this.OwningCommand = owningCommand;
            this.PendingCommand = null;
            this.ReturnedOutput = null;
        }

        public static EXEExecutionResult Success()
        {
            return Success(null);
        }
        public static EXEExecutionResult Success(EXECommand owningCommand)
        {
            return new EXEExecutionResult(true, owningCommand);
        }

        public static EXEExecutionResult Error(string errorCode, string errorMessage)
        {
            return Error(errorCode, errorMessage, null);
        }
        public static EXEExecutionResult Error(string errorCode, string errorMessage, EXECommand owningCommand)
        {
            return new EXEExecutionResult(false, errorMessage, errorCode, owningCommand);
        }

        public override string ToString()
        {
            VisitorCommandToString visitor = new VisitorCommandToString();
            if (this.OwningCommand != null) {
                this.OwningCommand.Accept(visitor);
            }
            return string
                    .Format
                    (
                        "{{\n\tsuccess: '{0}',\n\tcommand: '{1}',\n\tcommandType: '{2}',\n\terrorMessage: '{3}',\n\terrorCode: '{4}'\n}}",
                        this.IsSuccess,
                        OwningCommand == null ? string.Empty : visitor.GetCommandString(),
                        OwningCommand == null ? string.Empty : this.OwningCommand.GetType().Name,
                        ErrorMessage ?? string.Empty,
                        ErrorCode ?? string.Empty
                    );
        }
    }
}
