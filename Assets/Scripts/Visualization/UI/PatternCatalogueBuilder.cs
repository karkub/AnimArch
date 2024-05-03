using UnityEngine;
using Visualization.UI;


namespace Visualisation.UI
{
    public abstract class  PatternCatalogueBuilder : MonoBehaviour
    {
        public abstract GameObject Build(PatternCatalogueComponent patternComposite, GameObject parent, string path);
    }
}