using UnityEngine;
using System.IO;

namespace Visualization.UI{
    public class PatternCatalogueCompositeLoader : MonoBehaviour
    {
        public void Browse(PatternCatalogueComponent patternCatalogueComponent)
        {
            string folderPath = "Assets/Resources/PatternCatalogue";

            if (Directory.Exists(folderPath)){
                RecursivelyListFiles(folderPath, patternCatalogueComponent);
            }else{
                Debug.LogError("Folder does not exist: " + folderPath);
            }
        }

        void RecursivelyListFiles(string folderPath, PatternCatalogueComponent patternCatalogueComponent)
        {   
            string[] files = Directory.GetFiles(folderPath);
            foreach (string file in files){
                if(!file.Contains(".meta")){
                    patternCatalogueComponent.Add(new PatternCatalogueLeaf(Path.GetDirectoryName(file), Path.GetFileName(file)));
                }
            }

            string[] subFolders = Directory.GetDirectories(folderPath);
            foreach (string subFolder in subFolders){
                PatternCatalogueComponent newParent = new PatternCatalogueComposite(folderPath + Path.GetFileName(subFolder) ,Path.GetFileName(subFolder));
                patternCatalogueComponent.Add(newParent);
                RecursivelyListFiles(subFolder, newParent);
                
            }
        }
    }
}

