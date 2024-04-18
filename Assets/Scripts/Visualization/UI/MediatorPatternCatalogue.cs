using UnityEngine;
using UnityEngine.UIElements;
using Visualisation.Animation;
using Visualization.Animation;
using Visualization.TODO;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;

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
        [SerializeField] private GameObject PrefabPattern;
        [SerializeField] private GameObject PrefabLeaf;
        private List<(PatternCatalogueComponent, GameObject)> PatternNodes = new List<(PatternCatalogueComponent, GameObject)>();
        private PatternCatalogueCompositeLoader patternCatalogueLoader = new PatternCatalogueCompositeLoader();
        public MediatorMainPanel MediatorMainPanel;
        PatternCatalogueComponent patternCatalogueComponentRoot = new PatternCatalogueComposite("null","PatternCatalogue");
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
                if (leafNode.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text == node.Item1.GetName()){
                    AnimationData.Instance.SetDiagramPath(node.Item1.GetComponent().ComponentPath);
                    MenuManager.Instance.SetDiagramPath(node.Item1.GetComponent().ComponentPath);
                    FileLoader.Instance.OpenDiagram();
                    //TODO> nacitaj diagram a animaciu
                }
            }
        }

        private void OnButtonExitClicked()
        {
            DestroyAllNodes();
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
            patternCatalogueComponentRoot =  new PatternCatalogueComposite("null","PatternCatalogue");
            patternCatalogueLoader.Browse(patternCatalogueComponentRoot);
            RecursiveCreatePatternPrefabs(PrefabCanvas,patternCatalogueComponentRoot,0.0f);
            //CreatePatternPrefabsIteratively(PrefabCanvas, patternCatalogueComponent);
        }

        //TODO Not assigning the children in the scene correctly
        // ukladať referencie detí, a setactive podla kliku na parenta
        // dvakrat načítanie katalogu - naloaduje ich moc moc
        // premenovať composite, component a leaf - na nejaké krajšie nech je jasné
        // kde-čo-ako sa nachadzajú
        // method pagination = inšpirácia na priradovanie parenta
        // priradit parent do atribútu
        // schoval vytvaranie strruktuy za builder
        private void RecursiveCreatePatternPrefabs(GameObject parent, PatternCatalogueComponent patternCatalogueComponent, float rightOffset)
        {
            float currentRigthOffset = rightOffset;
            foreach(PatternCatalogueComponent child in patternCatalogueComponent.GetComponent().GetChildren())
            {
                if(child is PatternCatalogueComposite)
                {
                    GameObject newParent = Instantiate(PrefabPattern);
                    PatternNodes.Add((child.GetComponent(), newParent));
                    
                    //newParent.transform.position += new Vector3(rightOffset, 0, 0);
                    GameObject panel = newParent.transform.Find("Panel").gameObject;
                    newParent.transform.SetParent(parent.transform, false);
                    panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = child.GetName();

                    panel.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnPatternClicked(newParent));
                  
                    RecursiveCreatePatternPrefabs(newParent, child.GetComponent(), currentRigthOffset+1);
                }
                else if(child is PatternCatalogueLeaf)
                {
                    GameObject newPattern = Instantiate(PrefabLeaf);
                    PatternNodes.Add((child.GetComponent(), newPattern));
                    newPattern.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = child.GetName();
                    newPattern.transform.SetParent(parent.transform, false);
                    newPattern.transform.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnLeafClicked(newPattern));
                    newPattern.SetActive(false);
                }
            }
        }

        private void DestroyAllNodes()
        {
            foreach(var node in PatternNodes)
            {
                Destroy(node.Item2);
            }
            PatternNodes = new List<(PatternCatalogueComponent, GameObject)>();
        }

    }
}