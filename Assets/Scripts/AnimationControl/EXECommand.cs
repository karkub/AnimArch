using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OALProgramControl
{
    public abstract class EXECommand
    {
        public long CommandID { get; private set; }

        public bool IsActive { get; set; } = false;
        public bool IsDirectlyInCode { get; set; } = false;
        public static EXEScopeNull NullScope = EXEScopeNull.GetInstance();
        public EXEScopeBase SuperScope { get; set; } = NullScope; 
        public EXEExecutionStack CommandStack { get; set; } = null;
        
        public virtual IEnumerable<EXEScopeBase> ScopesToTop()
        {
            EXEScopeBase currentScope = this.SuperScope;
            yield return currentScope;

            foreach (EXEScopeBase scope in currentScope.ScopesToTop())
            {
                yield return scope;
            }
        }
        public EXEScopeMethod GetCurrentMethodScope()
        {
            return ScopesToTop()
                .FirstOrDefault(scope => scope is EXEScopeMethod)
                as EXEScopeMethod;
        }
        public EXEScopeLoop GetCurrentLoopScope()
        {
            return ScopesToTop()
                .FirstOrDefault(scope => scope is EXEScopeLoop)
                as EXEScopeLoop;
        }
        public EXEExecutionResult PerformExecution(OALProgram OALProgram)
        {
            EXEExecutionResult Result = Execute(OALProgram);

            return Result;
        }
        protected abstract EXEExecutionResult Execute(OALProgram OALProgram);
        public virtual bool SubmitReturn(EXEValueBase returnedValue, OALProgram programInstance)
        {
            return false;
        }
        protected EXEExecutionResult Success()
        {
            return EXEExecutionResult.Success(this);
        }
        protected EXEExecutionResult Error(string errorCode, string errorMessage)
        {
            return EXEExecutionResult.Error(errorCode, errorMessage, this);
        }
        public EXEScopeBase GetSuperScope()
        {
            return this.SuperScope;
        }
        public virtual void SetSuperScope(EXEScopeBase SuperScope)
        {
            if (this == SuperScope)
            {
                return;
            }

            this.SuperScope = SuperScope;

            if (this.CommandStack == null)
            {
                this.CommandStack = SuperScope?.CommandStack;
            }
        }
        public EXEScopeBase GetTopLevelScope()
        {
            EXEScopeBase CurrentScope = this.SuperScope;

            if (CurrentScope is EXEScopeNull)
            {
                return this as EXEScopeBase;
            }

            return CurrentScope.GetTopLevelScope();
        }
        public virtual Boolean IsComposite()
        {
            return false;
        }
        public EXECommand CreateClone()
        {
            EXECommand copy = CreateCloneCustom();

            // Shared behaviour of cloning goes here
            copy.IsDirectlyInCode = IsDirectlyInCode;
            copy.CommandID = CommandID;

            return copy;
        }
        protected abstract EXECommand CreateCloneCustom();
        public virtual void Accept(Visitor v) {
            v.VisitExeCommand(this);
        }

        public virtual void ToggleActiveRecursiveBottomUp(bool active)
        {
            this.IsActive = active;
            this.SuperScope.ToggleActiveRecursiveBottomUp(active);
        }
        /**Use this when evaluating AST nodes and Execute might need to be called again.**/
        protected bool HandleRepeatableASTEvaluation(EXEExecutionResult executionResult)
        {
            if (!executionResult.IsDone)
            {
                // It's a stack-like structure, so we enqueue the current command first, then the pending command.
                this.CommandStack.Enqueue(this);
                executionResult.PendingCommand.SetSuperScope(this.SuperScope);
                executionResult.PendingCommand.CommandStack = this.CommandStack;
                this.CommandStack.Enqueue(executionResult.PendingCommand);
                return false;
            }

            if (!executionResult.IsSuccess)
            {
                executionResult.OwningCommand = this;
                return false;
            }

            return true;
        }
        /**Use this when performing an action and Execute cannot be called again.**/
        protected bool HandleSingleShotASTEvaluation(EXEExecutionResult executionResult)
        {
            executionResult.OwningCommand = this;

            if (!executionResult.IsDone)
            {
                executionResult.IsSuccess = false;
                executionResult.ErrorMessage = "Unexpected request to re-evaluate during command execution";
                executionResult.ErrorCode = "XEC2024";
                return false;
            }

            return executionResult.IsSuccess;
        }

        public override bool Equals(object obj) //TODOa asi netreba
        {
            // Debug.Log("[Karin] EXECommand.Equals");
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            EXECommand other = (EXECommand)obj;
            return IsActive == other.IsActive &&
                   IsDirectlyInCode == other.IsDirectlyInCode &&
                   EqualityComparer<EXEScopeBase>.Default.Equals(SuperScope, other.SuperScope) &&
                   EqualityComparer<EXEExecutionStack>.Default.Equals(CommandStack, other.CommandStack);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IsActive, IsDirectlyInCode, SuperScope, CommandStack);
        }

        public virtual void SetCommandID()
        {
            CommandID = EXEScopeMethod.CommandIDSeed++;
            VisitorCommandToString v = new VisitorCommandToString();
            this.Accept(v);
            Debug.Log($"[Karin] SetCommandID: {this.GetType()}; CommandID - {CommandID}; Code: {v.GetCommandString()}");     
        }

        public virtual EXECommand FindByCommandID(long CommandID)
        {
            Debug.Log($"[Karin] AA FIND this.CommandID - {this.CommandID} ==? CommandID - {CommandID}");
            if (this.CommandID == CommandID)
            {
                Debug.Log("[Karin] AA EXECommand.FindByCommandID: found " + CommandID);
                return this;
            }
            Debug.LogError("[Karin] AA EXECommand.FindByCommandID: not found " + CommandID);
            return null;
        }
    }
}
