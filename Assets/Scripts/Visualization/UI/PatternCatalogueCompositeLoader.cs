using UnityEngine;
using System.IO;
using Visualisation.UI;
using System.Collections.Generic;

namespace Visualization.UI{
    public class PatternCatalogueCompositeLoader : MonoBehaviour
    {
        [SerializeField] private GameObject parent;
        [SerializeField] private GameObject PatternPanel;
        public List<GameObject> patternComposites;
        public List<GameObject> patternLeafs;
        private PatternCatalogueBuilder PatternCatalogueBuilder;

        public void Awake()
        {
            PatternCatalogueBuilder = GetComponent<PatternCatalogueBuilder>();
        }

        public List<GameObject> GetPatternComposites()
        {
            return patternComposites;
        }
        public List<GameObject> GetPatternLeafs()
        {
            return patternLeafs;
        }
   
        public void Browse(PatternCatalogueComponent patternCatalogueComponent)
        {
            patternComposites = new List<GameObject>();
            patternLeafs = new List<GameObject>();
            string folderPath = "Assets/Resources/PatternCatalogue";

            if (Directory.Exists(folderPath)){
                RecursivelyListFiles(folderPath, patternCatalogueComponent, parent);
            }else{
                Debug.LogError("Folder does not exist: " + folderPath);
            }
        }

        void RecursivelyListFiles(string folderPath, PatternCatalogueComponent patternCatalogueComponent, GameObject parent)
        {   
            string[] files = Directory.GetFiles(folderPath);
            foreach (string file in files){
                if(!file.Contains(".meta")){
                    GameObject leaf = PatternCatalogueBuilder.BuildLeaf(patternCatalogueComponent, parent, file);
                    patternLeafs.Add(leaf);
                }
            }

            string[] subFolders = Directory.GetDirectories(folderPath);
            foreach (string subFolder in subFolders){
                GameObject composite = PatternCatalogueBuilder.BuildComposite(patternCatalogueComponent, parent, subFolder);
                if(ReferenceEquals(parent, PatternPanel)){
                    GameObject arrow = composite.GetComponent<PatternCatalogueComposite>().GetArrow();
                    arrow.transform.Rotate(0,0,0);
                }else{
                    composite.SetActive(false);
                }
                GameObject newParent = composite;
                patternComposites.Add(composite);
                RecursivelyListFiles(subFolder, composite.GetComponent<PatternCatalogueComponent>(), newParent);
            }
        }
    }
}

