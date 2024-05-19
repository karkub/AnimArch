using UnityEngine;
using Visualization.UI;
using Visualisation.UI;
using System.IO;
using UnityEngine.UI;
using TMPro;

namespace Visualisation.UI
{
    public class  PatternCatalogueBuilder : MonoBehaviour
    {
        public GameObject LeafPrefab;
        public GameObject PatternPrefab;
        [SerializeField] private GameObject PrefabCanvas;

        public GameObject BuildLeaf(PatternCatalogueComponent patternComposite, GameObject parent, string path)
        {
            GameObject newPattern = Instantiate(LeafPrefab);
            PatternCatalogueLeaf patternCatalogueLeaf = newPattern.GetComponent<PatternCatalogueLeaf>();
            TextMeshProUGUI LeafText = patternCatalogueLeaf.GetLabel().GetComponent<TextMeshProUGUI>();
            patternCatalogueLeaf.ComponentName = Path.GetFileName(path);
            patternCatalogueLeaf.ComponentPath = path;
            LeafText.text = patternCatalogueLeaf.ComponentName;
            patternCatalogueLeaf.GetComponent<Button>().onClick.AddListener(() => GetComponent<MediatorPatternCatalogue>().OnClicked(newPattern));

            parent.GetComponent<PatternCatalogueComponent>().Add(patternComposite);
            newPattern.transform.SetParent(parent.transform, false);
            newPattern.SetActive(false);

            return newPattern;
        }
        public GameObject BuildComposite(PatternCatalogueComponent patternComposite, GameObject parent, string path)    
        {
            GameObject newParent = Instantiate(PatternPrefab);
            PatternCatalogueComposite patternCatalogueComposite = newParent.GetComponent<PatternCatalogueComposite>();
            TextMeshProUGUI ComponentText = patternCatalogueComposite.GetLabel().GetComponent<TextMeshProUGUI>();
            patternCatalogueComposite.ComponentName = Path.GetFileName(path);
            ComponentText.text = patternCatalogueComposite.ComponentName;
            patternCatalogueComposite.ComponentPath = path;
            patternCatalogueComposite.GetPanel().GetComponent<Button>().onClick.AddListener(() => GetComponent<MediatorPatternCatalogue>().OnClicked(newParent));

            newParent.transform.SetParent(parent.transform, false);

            parent.GetComponent<PatternCatalogueComposite>().Add(patternCatalogueComposite);

            return newParent;
        }
        public PatternCatalogueComposite BuildRoot()
        {
            return PrefabCanvas.GetComponent<PatternCatalogueComposite>();
        }
    }
}