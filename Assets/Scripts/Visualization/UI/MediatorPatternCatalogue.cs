using UnityEngine;
using UnityEngine.UIElements;
using Visualisation.Animation;
using Visualization.Animation;
using Visualization.TODO;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using Visualisation.UI;

namespace Visualization.UI
{
    public class MediatorPatternCatalogue : Mediator 
    {
        [SerializeField] private GameObject PatternCataloguePanel;
        [SerializeField] private GameObject ButtonExit;
        [SerializeField] private GameObject Separator;
        [SerializeField] private GameObject UpperSeparator;
        [SerializeField] private GameObject PatternCatalogueLabel;
        [SerializeField] private GameObject PrefabCanvas;
        [SerializeField] private GameObject patternCatalogueComponentRoot;
        private List<PatternCatalogueComponent> PatternNodes = new();
        private List<GameObject> PatternPrefabs = new List<GameObject>();
        private PatternCatalogueCompositeLoader patternCatalogueLoader = new PatternCatalogueCompositeLoader();
        public MediatorMainPanel MediatorMainPanel;
        private PatternCatalogueBuilder patternBuilder = new PatternCatalogueBuilder(); 
        public override void OnClicked(GameObject Button)
        {
            if (ReferenceEquals(Button, ButtonExit))
            {
                OnButtonExitClicked();
            } else if (Button.CompareTag("PatternNode"))
            {
                OnPatternClicked(Button);
                Debug.Log("PatternNode MEDIATOR CLICKED");
            } else if (Button.CompareTag("LeafNode"))
            {
                OnLeafClicked(Button);
                Debug.Log("LeafNode MEDIATOR CLICKED");
            }else{
                Debug.Log("Uknown MEDIATOR CLICKED");
            }
            //TODO pridaj onclick na button a posli mediatora, prekonaj podla typu componentu
        }

        public void OnPatternClicked(GameObject patternNode)
        {
            foreach (Transform child in patternNode.transform)
            {
                if (!child.name.Equals("Panel"))
                {
                    child.gameObject.SetActive(!child.gameObject.activeSelf);
                }else{
                    GameObject arrow = child.gameObject.transform.GetChild(1).gameObject;
                    if(arrow.transform.rotation.z == 0)
                    {
                        arrow.transform.Rotate(0,0,90);
                    }else{  
                        arrow.transform.Rotate(0,0,-90);
                    }                    
                }
                
            }
        }

        public void OnLeafClicked(GameObject leafNode)
        {
            Debug.Log(leafNode.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text);

            foreach (var node in PatternNodes)
            {
                if (leafNode.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text == node.GetName()){
                    AnimationData.Instance.SetDiagramPath(node.ComponentPath);
                    MenuManager.Instance.SetDiagramPath(node.ComponentPath);
                    FileLoader.Instance.OpenDiagram();
                    //TODO> nacitaj diagram a animaciu
                }
            }
        }

        private void OnButtonExitClicked()
        {
            DestroyAllNodes();
            DestroyAllChildren(PrefabCanvas);
            MediatorMainPanel.UnshowPatternCatalogue();
            ButtonExit.SetActive(false);
        }

        public void SetActivePatternCataloguePanel(bool active)
        {
            PatternCataloguePanel.SetActive(active);
            Separator.SetActive(active);
            UpperSeparator.SetActive(active);
            PatternCatalogueLabel.SetActive(active);
            ButtonExit.SetActive(active);
            PatternCatalogueComponent patternCatalogueComponentRoot = PatternCataloguePanel.GetComponent<PatternCatalogueComposite>();
            //var components = PatternCataloguePanel.GetComponents<MonoBehaviour>();

            patternCatalogueLoader.Browse(patternCatalogueComponentRoot);
            RecursiveCreatePatternPrefabs(PrefabCanvas,patternCatalogueComponentRoot);
        }

        //TODO Not assigning the children in the scene correctly
        // ukladať referencie detí, a setactive podla kliku na parenta
        // dvakrat načítanie katalogu - naloaduje ich moc moc
        // premenovať composite, component a leaf - na nejaké krajšie nech je jasné
        // kde-čo-ako sa nachadzajú
        // method pagination = inšpirácia na priradovanie parenta
        // priradit parent do atribútu
        // schoval vytvaranie strruktuy za builder
        private void RecursiveCreatePatternPrefabs(GameObject parent, PatternCatalogueComponent patternCatalogueComponent)
        {
            foreach(PatternCatalogueComponent child in patternCatalogueComponent.GetComponent().GetChildren())
            {
                if(child is PatternCatalogueComposite)
                {
                    if(child == null)
                    {
                        Debug.Log("Child is null");
                    }
                    GameObject newParent = patternBuilder.GetCompositeBuilder().Build(child, parent);
                    PatternPrefabs.Add(newParent);
                    GameObject panel = newParent.transform.Find("Panel").gameObject;
                    panel.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnPatternClicked(newParent));
                    RecursiveCreatePatternPrefabs(newParent, child.GetComponent());
                }
                else if(child is PatternCatalogueLeaf)
                {
                    GameObject newPattern = patternBuilder.GetLeafBuilder().Build(child, parent);
                    newPattern.transform.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnLeafClicked(newPattern));
                    PatternPrefabs.Add(newPattern);
                }
            }
        }

        private void DestroyAllNodes()
        {
            foreach(GameObject node in PatternPrefabs)
            {
                Destroy(node);
            }
            PatternPrefabs.Clear();
            Debug.Log(PatternPrefabs.Count);
        }

        void DestroyAllChildren(GameObject parent)
        {
            foreach (Transform child in parent.transform)
            {
                Destroy(child.gameObject);
            }
        }

    }
}