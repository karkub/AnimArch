using UnityEngine;
using System.Collections.Generic;
using Visualization.UI;
using TMPro;
using Visualisation.UI;
using System.IO;
using UnityEngine.UI;

public class PatternCatalogueCompositeBuilder : PatternCatalogueBuilder
{
    public GameObject PatternPrefab;
    public PatternCatalogueCompositeBuilder(GameObject PrefabPattern)
    {
        PatternPrefab = PrefabPattern;
    }
    public override GameObject Build(PatternCatalogueComponent patternComposite, GameObject parent, string path)
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
}