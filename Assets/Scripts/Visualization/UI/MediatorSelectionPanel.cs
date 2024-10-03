using UnityEngine;
using UnityEngine.UIElements;
using Visualization.Animation;
using Visualization.TODO;

namespace Visualization.UI
{
    public class MediatorSelectionPanel : Mediator 
    {
        [SerializeField] private GameObject SelectionPanel; 
        [SerializeField] private GameObject CancelButton;
        public MediatorCreationPanel MediatorCreationPanel;

        public override void OnClicked(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, CancelButton))
            {
                OnCancelButtonClicked();
            }
            else
            {
                OnClickedDefault(gameObject);
            }
        }

        private void OnCancelButtonClicked()
        {
            UIEditorManager.Instance.EndSelection();
            MediatorCreationPanel.SetActiveCreationPanel(true);
        }
        
    }
}