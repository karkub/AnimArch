using System.Collections.Generic;
using UnityEngine;

namespace Visualization.UI
{
    public class PatternCatalogueComponent : MonoBehaviour
    {
        public string ComponentName;
        public GameObject parent;
        public string ComponentPath{get; set;}

        public PatternCatalogueComponent(string path, string name)
        {
            ComponentName = name;
            ComponentPath = path;
        }
        public virtual PatternCatalogueComponent GetComponent()
        {
            return null;
        }
        public virtual void Operation(){}
        public virtual void Add(PatternCatalogueComponent component){}
        public virtual void Remove(PatternCatalogueComponent component){}
        
        public void Awake(){
            // tu setovať parenta
        }
        public virtual string GetName()
        {
            return ComponentName;
        }
        public virtual List<PatternCatalogueComponent> GetChildren()
        {
            return null;
        }
        public virtual PatternCatalogueComponent GetChild(int index)
        {
            return null;
        }
        //TODO pridaj label atribut a drag and dropni ho v editore z prefabu
    }
}