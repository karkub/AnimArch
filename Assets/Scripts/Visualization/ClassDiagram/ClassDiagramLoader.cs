using System.IO;
using Parsers;

namespace Visualization.ClassDiagram 
{
    public abstract class IClassDiagramLoader 
    { 
        public abstract Parser LoadDiagram(string diagramPath); 
    }

    public class ClassDiagramLoaderBase : IClassDiagramLoader
    {
        protected IClassDiagramLoader successor;

    

        public override Parser LoadDiagram(string diagramPath)
        {
            CreateChain();
            return successor.LoadDiagram(diagramPath);
        }

        public void SetSuccessor(IClassDiagramLoader successor)
        {
            this.successor = successor;
        }

        public void CreateChain() 
        {
            XMIDiagramLoader XMILoader = new XMIDiagramLoader();
            JsonDiagramLoader jsonLoader = new JsonDiagramLoader();
            DefaultDiagramLoader defaultLoader = new DefaultDiagramLoader();
            SetSuccessor(XMILoader);
            XMILoader.SetSuccessor(jsonLoader);
            jsonLoader.SetSuccessor(defaultLoader);
        }
    }


    public class XMIDiagramLoader : ClassDiagramLoaderBase
    {
        public override Parser LoadDiagram(string diagramPath)
        {
            string entension = Path.GetExtension(diagramPath);
            if(entension.Equals(".xml"))
            {
                Parser parser = Parser.GetParser(entension);
                parser.LoadDiagram();
                return parser;

            }
            return successor.LoadDiagram(diagramPath);
        }
    }

    public class JsonDiagramLoader : ClassDiagramLoaderBase
    {
        public override Parser LoadDiagram(string diagramPath)
        {
            string entension = Path.GetExtension(diagramPath);
            if(entension.Equals(".json"))
            {
                Parser parser = Parser.GetParser(entension);
                parser.LoadDiagram();
                return parser;
            }

            return successor.LoadDiagram(diagramPath);
        }
    }

    public class DefaultDiagramLoader : IClassDiagramLoader
    {
        public override Parser LoadDiagram(string diagramPath)
        {
            return Parser.GetParser(Path.GetExtension(diagramPath));
        }
    }
}