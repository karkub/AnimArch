using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Visualization.Animation;
using Visualization.ClassDiagram;


namespace Visualization.UI
{
    public class MediatorAnimationPlayOpt : Mediator
    {
        [SerializeField] private GameObject ButtonExit;
        public MediatorAnimationPlay MediatorAnimationPlay;
        
        public override void OnClicked(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, ButtonExit))
            {
                OnButtonExitClicked();
            }
            else
            {
                OnClickedDefault(gameObject);
            }
        }

        private void OnButtonExitClicked()
        {
            MenuManager.Instance.UnshowAnimation();
            MenuManager.Instance.EndPlay();
            DiagramPool.Instance.ObjectDiagram.ResetDiagram();
            MediatorAnimationPlay.SetActiveAnimationPlay(false);
            MediatorAnimationPlay.SetActiveMainPanel(true);
        }

    }
}