using AnimArch.Visualization.Diagrams;
using UnityEngine;
using UnityEngine.UIElements;
using Visualization.Animation;
using Visualization.TODO;

namespace Visualization.UI
{
    public class MediatorAnimationPlayPanel : Mediator 
    {
        [SerializeField] private GameObject PlayPanel;
        [SerializeField] private GameObject ButtonPlay;
        [SerializeField] private GameObject ButtonStop;
        [SerializeField] private GameObject ButtonPause;        
        [SerializeField] private GameObject StepButton;        

        public override void OnClicked(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, ButtonPlay))
            {
                OnPlayBtnClicked();
            }
            else if (ReferenceEquals(gameObject, ButtonStop))
            {
                OnStopBtnClicked();
            }
            else if (ReferenceEquals(gameObject, ButtonPause))
            {
                OnPauseBtnClicked();
            }
            else if (ReferenceEquals(gameObject, StepButton))
            {
                OnStepBtnClicked();
            }
            else
            {
                OnClickedDefault(gameObject);
            }
        }

        private void OnPlayBtnClicked()
        {
            Animation.Animation.Instance.StartAnimation();
            ActivityDiagramManager.Instance.ClearDiagrams();
        }
        private void OnStopBtnClicked()
        {
            Animation.Animation.Instance.UnhighlightAll();
            MenuManager.Instance.HideErrorPanelOnStopButton();
        }
        private void OnPauseBtnClicked()
        {
            Animation.Animation.Instance.Pause();
        }
        private void OnStepBtnClicked()
        {
            MenuManager.Instance.ChangeMode();
        }

    }
}