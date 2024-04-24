using UnityEngine;
using System.IO;
using Visualisation.UI;

namespace Visualization.UI{
    public class PatternCatalogueCompositeLoader : MonoBehaviour
    {
        [SerializeField] private GameObject leafPrefab;
        [SerializeField] private GameObject compositePrefab;
        [SerializeField] private GameObject patternCataloguePanel;
        private GameObject parent;
        private PatternCatalogueBuilder patternBuilder = new PatternCatalogueBuilder(); 
        public void Awake(){
            parent = patternCataloguePanel;
        }
        public void Browse(PatternCatalogueComponent patternCatalogueComponent)
        {
            string folderPath = "Assets/Resources/PatternCatalogue";
            PatternCatalogueComponent root = patternCatalogueComponent;

            if (Directory.Exists(folderPath)){
                RecursivelyListFiles(folderPath, root, parent);
            }else{
                Debug.LogError("Folder does not exist: " + folderPath);
            }
        }

        void RecursivelyListFiles(string folderPath, PatternCatalogueComponent patternCatalogueComponent, GameObject parent)
        {   
            
            string[] files = Directory.GetFiles(folderPath);
            foreach (string file in files){
                if(!file.Contains(".meta")){
                    GameObject leaf = patternBuilder.GetLeafBuilder().Build(patternCatalogueComponent, parent);
                    //patternCatalogueComponent.Add(leaf.GetComponent());
                }
            }

            string[] subFolders = Directory.GetDirectories(folderPath);
            foreach (string subFolder in subFolders){
                // PatternCatalogueComponent newParent = new PatternCatalogueComposite(folderPath + Path.GetFileName(subFolder) ,Path.GetFileName(subFolder));
                // patternCatalogueComponent.Add(newParent);
                GameObject composite = patternBuilder.GetCompositeBuilder().Build(patternCatalogueComponent, parent);
                parent = composite;
                RecursivelyListFiles(subFolder, composite.GetComponent<PatternCatalogueComponent>(), parent);
                
            }
        }
    }
}

