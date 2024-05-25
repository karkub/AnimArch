using System.Collections.Generic;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.Relations;

namespace Parsers
{
    public abstract class Parser
    {
        public abstract string SaveDiagram();
        public abstract void LoadDiagram();
        public abstract List<Class> ParseClasses();
        public abstract List<Relation> ParseRelations();
        
        public static Parser GetParser(string path)
        {
            return path switch
            {
                ".xml" => new XMIParser(),
                ".json" => new JsonParser(),
                _ => new DefaultParser()
            };
        }
    }
}
