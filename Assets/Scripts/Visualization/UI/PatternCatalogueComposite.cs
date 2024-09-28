using System.Collections.Generic;
using UnityEngine;

namespace Visualization.UI
{
    public class PatternCatalogueComposite : PatternCatalogueComponent
    {
        public List<PatternCatalogueComponent> children;
        public string CompositePath{get; set;}
        public GameObject Label;
        public GameObject Arrow;
        public GameObject Panel;
        public void Awake()
        {
            children = new List<PatternCatalogueComponent>();
        }
        public override void Add(PatternCatalogueComponent component)
        {
            children.Add(component);
        }
        public override void Remove(PatternCatalogueComponent component)
        {
            children.Remove(component);
        }
        public override PatternCatalogueComponent GetComponent()
        {
            return this;
        }
        public override PatternCatalogueComponent GetChild(int index)
        {
            return children[index];
        }
        public override string GetName()
        {
            return ComponentName;
        }
        public void ActivateChildren(GameObject patternNode)
        {
            foreach (Transform child in patternNode.transform){
                if (!ReferenceEquals(child.gameObject, child.gameObject.GetComponentInParent<PatternCatalogueComposite>().GetPanel())){
                    child.gameObject.SetActive(!child.gameObject.activeSelf);
                    if(child.gameObject.GetComponent<PatternCatalogueComposite>() != null){
                        child.gameObject.GetComponent<PatternCatalogueComposite>().ActivateLeaf(child.gameObject);
                    }
                }else{
                    RotateArrow();
                }
            
            }
        }
        public void RotateArrow()
        {
            if(Arrow.transform.rotation.z == 0){
                Arrow.transform.Rotate(0,0,90);
            }else{
                Arrow.transform.Rotate(0,0,-90);
            }
        }
        public override List<PatternCatalogueComponent> GetChildren()
        {
            return children;
        }
        public override GameObject GetLabel()
        {
            return Label;
        }
        public override GameObject GetArrow()
        {
            return Arrow;
        }
        public override GameObject GetPanel()
        {
            return Panel;
        }
    }
}