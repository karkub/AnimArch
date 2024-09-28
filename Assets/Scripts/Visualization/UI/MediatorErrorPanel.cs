using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UIElements;
using Visualization.Animation;
using Visualization.TODO;

namespace Visualization.UI
{
    public class MediatorErrorPanel : Mediator  
    {
        [SerializeField] private GameObject CloseButton;

        public override void OnClicked(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, CloseButton))
            {
                OnCloseButtonClicked();
            }
            else
            {
                OnClickedDefault(gameObject);
            }
        }

        private void OnCloseButtonClicked()
        {
            MenuManager.Instance.HideErrorPanel();
        }
        
    }
}