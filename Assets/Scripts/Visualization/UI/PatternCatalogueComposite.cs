using System.Collections.Generic;
using UnityEngine;

namespace Visualization.UI
{
    public class PatternCatalogueComposite : PatternCatalogueComponent
    {
        private List<PatternCatalogueComponent> children = new List<PatternCatalogueComponent>();
        public string CompositeName;
        public string CompositePath{get; set;}
        public PatternCatalogueComposite(string path, string name) : base(path, name)
        {
            CompositeName = name;
            CompositePath = path;
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
            return CompositeName;
        }
        public override List<PatternCatalogueComponent> GetChildren()
        {
            return children;
        }
    }
}