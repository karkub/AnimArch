using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UIElements;
using Visualization.Animation;
using Visualization.TODO;

namespace Visualization.UI
{
    public class MediatorExtraPanel : Mediator        
    {
        [SerializeField] private GameObject GenerateToPythonButton;
        [SerializeField] private GameObject ToSKButton;
        [SerializeField] private GameObject ToENButton;

        public override void OnClicked(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, GenerateToPythonButton))
            {
                OnGenerateToPythonButtonClicked();
            }
            else if (ReferenceEquals(gameObject, ToSKButton))
            {
                OnToSKButtonClicked();
            }
            else if (ReferenceEquals(gameObject, ToENButton))
            {
                OnToENButtonClicked();
            }
            else
            {
                OnClickedDefault(gameObject);
            }
        }

        private void OnGenerateToPythonButtonClicked()
        {
            TooltipManager.Instance.HideTooltip();
            FileLoader.Instance.SaveAnimationToPython();
        }
        private void OnToSKButtonClicked()
        {
            TooltipManager.Instance.HideTooltip();
            MenuManager.Instance.SetLanguage(1);
        }
        private void OnToENButtonClicked()
        {
            TooltipManager.Instance.HideTooltip();
            MenuManager.Instance.SetLanguage(0);
        }
        
    }
}