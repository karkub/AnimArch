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
                if (!files[nextIndex].EndsWith(".meta"))
                {
                    return true;
                }
                nextIndex++;
            }
            return false;
        }

        public string GetItem()
        {
            while (index + 1  < files.Length)
            {
                index++;
                if (!files[index].EndsWith(".meta"))
                {
                    return files[index];
                }
            }
            throw new InvalidOperationException("No more elements in array.");

        }
        public void ResetFiles(string[] newfiles) 
        {
            files = newfiles;
            index = -1;
        }
    }
}