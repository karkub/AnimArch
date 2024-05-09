using UnityEngine;
using System.Collections.Generic;
using Visualization.ClassDiagram;
using Visualization.Animation;
using Visualisation.Animation;

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
        public MediatorMainPanel MediatorMainPanel;
        private PatternCatalogueCompositeLoader patternLoader;
        public List<GameObject> PatternPrefabs;
        private IClassDiagramBuilder _classDiagramBuilder;

        public override void OnClicked(GameObject Button)
        {
            if (ReferenceEquals(Button, ButtonExit))
            {
                OnButtonExitClicked();
            } else if (Button.GetComponentInParent<PatternCatalogueComposite>() != null && Button.GetComponentInParent<PatternCatalogueLeaf>() == null)
            {
                OnPatternClicked(Button);
            } else
            {
                OnLeafClicked(Button);
            }
        }

        private void OnPatternClicked(GameObject patternNode)
        {
            foreach (Transform child in patternNode.transform)
            {
                if (!child.name.Equals("Panel"))
                {
                    child.gameObject.SetActive(!child.gameObject.activeSelf);
                }else{
                    GameObject arrow = child.gameObject.transform.GetChild(1).gameObject;
                    RotateArrow(arrow);                   
                }
                
            }
            Debug.Log("PATH: " + patternNode.GetComponent<PatternCatalogueComposite>().ComponentPath);
        }

        private void OnLeafClicked(GameObject leafNode)
        {
            string parentPath = leafNode.transform.parent.GetComponent<PatternCatalogueComposite>().ComponentPath;
            string parentName = leafNode.transform.parent.GetComponent<PatternCatalogueComposite>().ComponentName;
            string path = leafNode.GetComponent<PatternCatalogueLeaf>().ComponentPath;
            string name = leafNode.GetComponent<PatternCatalogueLeaf>().ComponentName;
            Debug.Log("parentPath " + parentPath);
            Debug.Log("parentName " + parentName);
            Debug.Log("path " + path);
            Debug.Log("name " + name);
            OnButtonExitClicked();
            _classDiagramBuilder = ClassDiagramBuilderFactory.Create();

            MenuManager.Instance.SetDiagramPath(parentPath);
            AnimationData.Instance.SetDiagramPath(path);
            MenuManager.Instance.SetDiagramPath(parentPath);
            _classDiagramBuilder.LoadDiagram();

            Anim loadedAnim = new Anim(path.Replace(".json", ""));
            loadedAnim.LoadCode(path);
            //loadedAnim.Code = GetCleanCode(loadedAnim.Code);
            MenuManager.SetAnimationButtonsActive(true);
            AnimationData.Instance.selectedAnim = loadedAnim;
            MenuManager.Instance.SetSelectedAnimation(loadedAnim.AnimationName);

            //FileLoader.Instance.OpenDiagram();

/* 
            foreach (var node in PatternNodes)
            {
                if (leafNode.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text == node.GetName()){
                    AnimationData.Instance.SetDiagramPath(node.ComponentPath);
                    MenuManager.Instance.SetDiagramPath(node.ComponentPath);
                    FileLoader.Instance.OpenDiagram();
                    //TODO> nacitaj diagram a animaciu
                }
            } */
        }

        private void RotateArrow(GameObject arrow)
        {
            if(arrow.transform.rotation.z == 0)
            {
                arrow.transform.Rotate(0,0,90);
            }else{  
                arrow.transform.Rotate(0,0,-90);
            }
        }

        private void OnButtonExitClicked()
        {
            DestroyAllChildren(PrefabCanvas);
            SetActivePatternCataloguePanel(false);
            SetActiveMainPanel(true);
        }

        public void SetActiveMainPanel(bool active)
        {
            MediatorMainPanel.SetActiveMainPanel(active);
        }

        public void SetActivePatternCataloguePanel(bool active)
        {
            PatternCataloguePanel.SetActive(active);
            Separator.SetActive(active);
            UpperSeparator.SetActive(active);
            PatternCatalogueLabel.SetActive(active);
            ButtonExit.SetActive(active);

            PatternCatalogueComponent patternCatalogueComponentRoot = PrefabCanvas.GetComponent<PatternCatalogueComposite>();
            
            if(PatternPrefabs.Count == 0)
            {
                patternLoader = PrefabCanvas.GetComponent<PatternCatalogueCompositeLoader>();
                patternLoader.Browse(patternCatalogueComponentRoot);
                PatternPrefabs = patternLoader.patternPrefabs;
            }
        }

        //TODO Not assigning the children in the scene correctly
        // ukladať referencie detí, a setactive podla kliku na parenta
        // dvakrat načítanie katalogu - naloaduje ich moc moc
        // premenovať composite, component a leaf - na nejaké krajšie nech je jasné
        // kde-čo-ako sa nachadzajú
        // method pagination = inšpirácia na priradovanie parenta
        // priradit parent do atribútu
        // schoval vytvaranie strruktuy za builder

        void DestroyAllChildren(GameObject parent)
        {
            foreach (Transform child in parent.transform)
            {
                Destroy(child.gameObject);
            }
            PatternPrefabs.Clear();
        }

    }
}