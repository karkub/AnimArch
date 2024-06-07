using System;
namespace Visualization.UI
{
    public class PatternCatalogueIterator 
    {
        private string[] files;
        private int index;

        public PatternCatalogueIterator(string[] files) 
        {
            this.files = files;
            index = -1;
        }
        public bool HasNext()
        {
            int nextIndex = index + 1;
            while (nextIndex < files.Length)
            {
                if (!IsExcluded(files[nextIndex]))
                {
                    return true;
                }
                nextIndex++;
            }
            return false;
        }

        public string GetItem()
        {
            while (HasNext())
            {
                index++;
                string item = files[index];
                if (!IsExcluded(item))
                {
                    break;
                }
            }
            return files[index];
        }

        public bool IsExcluded(string filename)
        {
            return filename.EndsWith(".meta");

        }
        public void ResetFiles(string[] newfiles) 
        {
            files = newfiles;
            index = -1;
        }
    }
}