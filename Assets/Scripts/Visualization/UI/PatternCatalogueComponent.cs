using System.Collections.Generic;
using UnityEngine;

namespace Visualization.UI
{
    public class PatternCatalogueComponent : MonoBehaviour
    {
        public string ComponentName;
        public GameObject parent;
        public string ComponentPath{get; set;}
        public virtual PatternCatalogueComponent GetComponent()
        {
            return null;
        }
        public virtual void Operation(){}
        public virtual void Add(PatternCatalogueComponent component){}
        public virtual void Remove(PatternCatalogueComponent component){}
        
        public virtual string GetName()
        {
            return ComponentName;
        }
        public virtual List<PatternCatalogueComponent> GetChildren()
        {
            return new List<PatternCatalogueComponent>();
        }
        public virtual PatternCatalogueComponent GetChild(int index)
        {
            return null;
        }
        public virtual GameObject GetLabel()
        {
            return null;
        }
        public virtual GameObject GetArrow()
        {
            return null;
        }
        public virtual GameObject GetPanel()
        {
            return null;
        }
        public virtual void ActivateLeaf(GameObject gameObject){}
        
    }
}