using UnityEngine;
using System.Collections.Generic;
using Visualization.ClassDiagram;
using Visualization.Animation;
using Visualisation.Animation;
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
        public MediatorMainPanel MediatorMainPanel;
        private PatternCatalogueCompositeLoader patternLoader;
        public List<GameObject> PatternComposites;
        public List<GameObject> PatternLeafs;
        private IClassDiagramBuilder _classDiagramBuilder;
        private PatternCatalogueBuilder PatternCatalogueBuilder;
        public void Awake()
        {
            patternLoader = PrefabCanvas.GetComponent<PatternCatalogueCompositeLoader>();
            PatternCatalogueBuilder = PrefabCanvas.GetComponent<PatternCatalogueBuilder>();
        }

        public override void OnClicked(GameObject Button)
        {
            //TODO ppozeraj cez listy a porovnaj ci je to composite alebo leaf
            //else pre neznamy GUI prvok
            if (ReferenceEquals(Button, ButtonExit))
            {
                OnButtonExitClicked();
            } else if (patternLoader.GetPatternComposites().Contains(Button))
            {
                OnPatternClicked(Button);
            } else if (patternLoader.GetPatternLeafs().Contains(Button))
            {
                OnLeafClicked(Button);
            }else{
                Debug.LogError("Unknown button clicked in MediatorPatternCatalogue: " + Button.name);
            }
        }

        private void OnPatternClicked(GameObject patternNode)
        {
            patternNode.GetComponent<PatternCatalogueComposite>().ActivateChildren(patternNode);
        }

        private void OnLeafClicked(GameObject leafNode)
        {
            string parentPath = leafNode.transform.parent.GetComponent<PatternCatalogueComposite>().ComponentPath;
            string path = leafNode.GetComponent<PatternCatalogueLeaf>().ComponentPath;
            OnButtonExitClicked();
            _classDiagramBuilder = ClassDiagramBuilderFactory.Create();

            MenuManager.Instance.SetDiagramPath(parentPath);
            AnimationData.Instance.SetDiagramPath(path);
            MenuManager.Instance.SetDiagramPath(parentPath);
            _classDiagramBuilder.LoadDiagram();

            Anim loadedAnim = new Anim(path.Replace(".json", ""));
            loadedAnim.LoadCode(path);
            MenuManager.SetAnimationButtonsActive(true);
            AnimationData.Instance.selectedAnim = loadedAnim;
            MenuManager.Instance.SetSelectedAnimation(loadedAnim.AnimationName);
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

            PatternCatalogueComponent patternCatalogueComponentRoot = PatternCatalogueBuilder.BuildRoot();
            
            if(PatternComposites.Count == 0 && PatternLeafs.Count == 0)
            {
                patternLoader.Browse(patternCatalogueComponentRoot);
                PatternComposites = patternLoader.patternComposites;
                PatternLeafs = patternLoader.patternLeafs;
            }
        }

        void DestroyAllChildren(GameObject parent)
        {
            foreach (Transform child in parent.transform)
            {
                Destroy(child.gameObject);
            }
            PatternComposites.Clear();
            PatternLeafs.Clear();
        }
    }
}