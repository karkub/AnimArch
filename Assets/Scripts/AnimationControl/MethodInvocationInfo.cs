using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OALProgramControl
{
    public class MethodInvocationInfo
    {
        public CDMethod CallerMethod { get; protected set; }
        public CDMethod CalledMethod { get; protected set; }
        public CDRelationship Relation { get; protected set; }
        public CDClassInstance CallerObject { get; protected set; }
        public CDClassInstance CalledObject { get; protected set; }

        public bool HasCaller
        {
            get
            {
                return CallerObject != null && CallerMethod != null && Relation != null;
            }
        }

        public MethodInvocationInfo(CDMethod callerMethod, CDMethod calledMethod, CDRelationship relation, CDClassInstance callerObject, CDClassInstance calledObject)
        {
            if (callerMethod == null)
            {
                throw new ArgumentNullException("callerMethod");
            }

            if (calledMethod == null)
            {
                throw new ArgumentNullException("calledMethod");
            }

            if (callerObject == null)
            {
                throw new ArgumentNullException("callerObject");
            }

            if (calledObject == null)
            {
                throw new ArgumentNullException("calledObject");
            }

            this.CallerMethod = callerMethod;
            this.CalledMethod = calledMethod;
            this.Relation = relation;
            this.CallerObject = callerObject;
            this.CalledObject = calledObject;
        }
        protected MethodInvocationInfo()
        {}

        public static MethodInvocationInfo CreateCallerOnlyInstance(CDMethod callerMethod, CDClassInstance callerObject) {
            if (callerMethod == null)
            {
                throw new ArgumentNullException("calledMethod");
            }
            if (callerObject == null)
            {
                throw new ArgumentNullException("calledObject");
            }

            return new MethodInvocationInfo
            {
                CallerMethod = callerMethod,
                CallerObject = callerObject
            };
        }

        public static MethodInvocationInfo CreateCalledOnlyInstance(CDMethod calledMethod, CDClassInstance calledObject) {
            if (calledMethod == null)
            {
                throw new ArgumentNullException("calledMethod");
            }
            if (calledObject == null)
            {
                throw new ArgumentNullException("calledObject");
            }

            return new MethodInvocationInfo
            {
                CalledMethod = calledMethod,
                CalledObject = calledObject
            };
        }

        public override string ToString()
        {
            return string.Join
            (
                " ",
                new string[]
                {
                    this.CallerMethod?.OwningClass?.Name ?? "NULL",
                    this.CallerMethod?.Name ?? "NULL",
                    this.Relation?.RelationshipName ?? "NULL",
                    this.CalledMethod?.OwningClass?.Name ?? "NULL",
                    this.CalledMethod?.Name ?? "NULL"
                }
            );
        }
    }
}
