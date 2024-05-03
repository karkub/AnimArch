using UnityEngine;
using System.Collections.Generic;
using Visualization.UI;
using TMPro;
using Visualisation.UI;
using System;
using System.IO;
using UnityEngine.UI;

public class PatternCatalogueLeafBuilder : PatternCatalogueBuilder
{          
    public GameObject LeafPrefab;
    public PatternCatalogueLeafBuilder(GameObject PrefabLeaf)         
    {
        LeafPrefab = PrefabLeaf;
    }
    

    public override GameObject Build(PatternCatalogueComponent patternComposite, GameObject parent, string path)
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
}
