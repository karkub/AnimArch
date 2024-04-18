using OALProgramControl;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Visualization.ClassDiagram
{
    public class ObjectDiagramValueVisitor : IValueVisitor
    {
        private readonly StringBuilder resultBuilder;
        public string Result
        {
            get
            {
                string result = resultBuilder.ToString();
                resultBuilder.Clear();
                return result;
            }
        }

        public ObjectDiagramValueVisitor()
        {
            resultBuilder = new StringBuilder();
        }

        public void VisitExeValueArray(EXEValueArray value)
        {
            resultBuilder.Append("[");

            bool isFirst = true;
            foreach (EXEValueBase elementValue in value.Elements)
            {
                if (!isFirst)
                {
                    resultBuilder.Append(", ");
                }
                isFirst = false;

                elementValue.Accept(this);
            }

            resultBuilder.Append("]");
        }

        public void VisitExeValueBool(EXEValueBool value)
        {
            string result = value.Value ? EXETypes.BooleanTrue : EXETypes.BooleanFalse;
            resultBuilder.Append(result);
        }

        public void VisitExeValueInt(EXEValueInt value)
        {
            string result = value.Value.ToString();
            resultBuilder.Append(result);
        }

        public void VisitExeValueReal(EXEValueReal value)
        {
            string result = value.Value.ToString();
            resultBuilder.Append(result);
        }

        public void VisitExeValueReference(EXEValueReference value)
        {
            string objectName = EXETypes.UnitializedName;

            if (value.ClassInstance != null)
            {
                objectName = DiagramPool.Instance.ObjectDiagram.GetObjectName(value.ClassInstance.UniqueID);
            }

            resultBuilder.Append(objectName);
        }

        public void VisitExeValueString(EXEValueString value)
        {
            string result = value.Value;
            resultBuilder.Append(result);
        }
    }
}