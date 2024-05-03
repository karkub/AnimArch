using UnityEngine;

namespace Visualization.UI
{
    public class PatternCatalogueLeaf : PatternCatalogueComponent
    {
        public string LeafPath{get; set;}
        public GameObject Label;

        public override PatternCatalogueComponent GetComponent()
        {
            return this;
        }
        public override void Operation()
        {
            //TODO: nacitanie animace/diagramu atd...
            Debug.Log("Leaf operation");
        } 
        public override string GetName()
        {
            return ComponentName;
        }
        public override GameObject GetLabel()
        {
            return Label;
        }
        //TODO pridaj leaf masking a diagram
        //TODO pridaj sipka atribut a drag and dropni ho v editore z prefabu

    }
}