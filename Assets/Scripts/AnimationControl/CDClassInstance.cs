using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OALProgramControl
{

    public class CDClassInstance
    {
        public long UniqueID { get; }
        // Attributes
        public Dictionary<string, EXEValueBase> State { get; }
        public readonly CDClass OwningClass;

        private readonly List<EXEValueReference> ReferencingValues;

        public CDClassInstance(long UniqueID, List<CDAttribute> attributes, CDClass owningClass)
        {
           this.State = new Dictionary<string, EXEValueBase>();

            foreach (CDAttribute Attribute in attributes)
            {
                EXEValueBase attributeValue = EXETypes.DefaultValue(Attribute.Type, owningClass.OwningClassPool);
                attributeValue.WasInitialized = false;

                this.State.Add(Attribute.Name, attributeValue);
            }

            this.UniqueID = UniqueID;
            this.State.Add(EXETypes.UniqueIDAttributeName, new EXEValueInt(UniqueID.ToString()));

            this.OwningClass = owningClass;

            this.ReferencingValues = new List<EXEValueReference>();

            UnityEngine.Debug.LogErrorFormat("Created object instance with id {0}", UniqueID);
        }
        public EXEValueBase GetAttributeValue(String name)
        {
            EXEValueBase Result = null;
            if (this.State.ContainsKey(name))
            {
                Result = this.State[name];
            }

            return Result;
        }
        public void Destroy()
        {
            foreach (EXEValueReference referencingValue in this.ReferencingValues)
            {
                referencingValue.Dereference();
            }
        }
    }
}
