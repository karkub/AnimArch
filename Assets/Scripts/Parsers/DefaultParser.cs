using System.Collections.Generic;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.Relations;

namespace Parsers
{
    public class DefaultParser : Parser
    {
        public override void LoadDiagram() {}
        public override string SaveDiagram()
        {
            return "";
        }

        public override List<Class> ParseClasses()
        {
            return new List<Class>();
        }

        public override List<Relation> ParseRelations()
        {
            return new List<Relation>();
            
        }
    }
}