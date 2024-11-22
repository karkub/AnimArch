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
        public override void ActivateLeaf(GameObject leafNode)
        {
            leafNode.SetActive(!leafNode.activeSelf);
        } 
        public override string GetName()
        {
            return ComponentName;
        }
        public override GameObject GetLabel()
        {
            return Label;
        }
    }
}