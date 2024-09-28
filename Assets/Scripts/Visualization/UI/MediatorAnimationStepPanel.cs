using UnityEngine;
using UnityEngine.UIElements;
using Visualization.Animation;
using Visualization.TODO;

namespace Visualization.UI
{
    public class MediatorAnimationStepPanel : Mediator 
    {
        [SerializeField] private GameObject StepPanel;
        [SerializeField] private GameObject StepBackButton;
        [SerializeField] private GameObject StepForwardButton;
        [SerializeField] private GameObject PlayModeButton;      

        public override void OnClicked(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, StepBackButton))
            {
                OnStepBackBtnClicked();
            }
            else if (ReferenceEquals(gameObject, StepForwardButton))
            {
                OnStepForwardBtnClicked();
            }
            else if (ReferenceEquals(gameObject, PlayModeButton))
            {
                OnPlayModeBtnClicked();
            }
            else
            {
                OnClickedDefault(gameObject);
            }
        }

        private void OnStepBackBtnClicked()
        {
            Animation.Animation.Instance.PrevStep();
        }
        private void OnStepForwardBtnClicked()
        {
            Animation.Animation.Instance.NextStep();
        }
        private void OnPlayModeBtnClicked()
        {
            MenuManager.Instance.ChangeMode();
        }
        
    }
}