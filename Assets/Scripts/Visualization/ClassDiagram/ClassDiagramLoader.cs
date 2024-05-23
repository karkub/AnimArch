using System.Collections.Generic;
using System.IO;
using Parsers;
using UMSAGL.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Visualization.ClassDiagram 
{
    public abstract class ClassDiagramLoader 
    {
        protected ClassDiagramLoader successor;

        public void SetSuccesor(ClassDiagramLoader successor) 
        {
            this.successor = successor;
        } 
        
        public abstract Parser LoadDiagram(string diagramPath); 
    }


    public class XMIDiagramLoader : ClassDiagramLoader
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
            else {
                SetSuccesor(new JsonDiagramLoader());
                return successor.LoadDiagram(diagramPath);
            }
        }
    }

    public class JsonDiagramLoader : ClassDiagramLoader
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
            else
            {
                SetSuccesor(new DefaultDiagramLoader());
                return successor.LoadDiagram(diagramPath);

            }
        }
    }

    public class DefaultDiagramLoader : ClassDiagramLoader
    {
        public override Parser LoadDiagram(string diagramPath)
        {
            // TODO Show error message for default loader
            return null;
        }
    }
}