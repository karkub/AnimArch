using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Visualization.ClassDiagram.ComponentsInDiagram;

namespace OALProgramControl
{
    public class ObjectCreationInfo : MethodInvocationInfo
    {
        public ObjectInDiagram CreatedObject { get; private set; }

        public ObjectCreationInfo(CDClassInstance callerObject, CDClassInstance calledObject, ObjectInDiagram createdObject) : base()
        {
            this.CallerObject = callerObject;
            this.CalledObject = calledObject;
            this.CreatedObject = createdObject;
        }
    }
}
