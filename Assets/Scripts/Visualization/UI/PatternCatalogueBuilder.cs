using UnityEngine;
using System.Collections.Generic;
using Visualization.UI;
using TMPro;


namespace Visualisation.UI
{
    public class PatternCatalogueBuilder : MonoBehaviour
    {        
     
        public GameObject PrefabPattern;
        public GameObject PrefabLeaf;   
        private PatternCatalogueCompositeBuilder compositeBuilder;        
        private PatternCatalogueLeafBuilder leafBuilder;

        void Awake(){
            compositeBuilder = new PatternCatalogueCompositeBuilder(PrefabPattern);
            leafBuilder = new PatternCatalogueLeafBuilder(PrefabLeaf);
        }
        public PatternCatalogueCompositeBuilder GetCompositeBuilder(){
            return compositeBuilder;
        }

        public PatternCatalogueLeafBuilder GetLeafBuilder(){
            return leafBuilder;
        }

        public class PatternCatalogueCompositeBuilder
        {
            public GameObject PatternPrefab;
            public PatternCatalogueCompositeBuilder(GameObject PrefabPattern)
            {
                PatternPrefab = PrefabPattern;
            }
            public GameObject Build(PatternCatalogueComponent patternComposite, GameObject parent)
            {
                GameObject newParent = Instantiate(PatternPrefab);
                newParent.transform.SetParent(parent.transform, false);

                GameObject panel = newParent.transform.Find("Panel").gameObject;
                panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = patternComposite.GetName();
                parent.GetComponent<PatternCatalogueComposite>().Add(patternComposite);

                return newParent;
            }
        }

        public class PatternCatalogueLeafBuilder
        {          
            public GameObject LeafPrefab;  
            public PatternCatalogueLeafBuilder(GameObject PrefabLeaf)         
            {
                LeafPrefab = PrefabLeaf;
            }
            

            public GameObject Build(PatternCatalogueComponent patternComposite, GameObject parent)
            {
                GameObject newPattern = Instantiate(LeafPrefab);
                newPattern.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = patternComposite.GetName();
                parent.GetComponent<PatternCatalogueComposite>().Add(patternComposite);
                newPattern.transform.SetParent(parent.transform, false);
                newPattern.SetActive(false);

                return newPattern;
            }
        }

    }
}