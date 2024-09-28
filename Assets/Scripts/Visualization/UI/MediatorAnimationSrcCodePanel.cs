using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Visualization.Animation;
using Visualization.ClassDiagram;


namespace Visualization.UI
{
    public class MediatorAnimationSrcCodePanel : Mediator
    {
        [SerializeField] private GameObject SrcCodeAnimationPanel;
        public MediatorAnimationPlay MediatorAnimationPlay;
        public MediatorCodePanel MediatorCodePanel;
        
        public override void OnClicked(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, SrcCodeAnimationPanel))
            {
                OnSrcCodeAnimationPanelSelected();
            }
            else
            {
                OnClickedDefault(gameObject);
            }
        }

        private void OnSrcCodeAnimationPanelSelected()
        {
            MediatorCodePanel.SetInteractableButtonSave(true);
            SrcCodeAnimationPanel.GetComponentInChildren<CodeHighlighter>().RemoveColors();
        }
        
    }
}