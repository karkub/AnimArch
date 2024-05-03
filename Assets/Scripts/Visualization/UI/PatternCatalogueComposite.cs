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
        public override void Operation()
        {
            foreach (PatternCatalogueComponent child in children)
            {
                child.Operation();
                // child.gameObject;
            }
        }
        public override PatternCatalogueComponent GetChild(int index)
        {
            return children[index];
        }
        public override string GetName()
        {
            return ComponentName;
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