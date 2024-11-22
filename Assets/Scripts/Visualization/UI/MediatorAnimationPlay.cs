using UnityEngine;
using UnityEngine.UIElements;
using Visualisation.Animation;
using Visualization.Animation;
using Visualization.TODO;

namespace Visualization.UI
{
    public class MediatorAnimationPlay : Mediator 
    {
        [SerializeField] private GameObject AnimationPlay;
        [SerializeField] private GameObject Slider;
        [SerializeField] private GameObject Toggle;
        [SerializeField] private GameObject ShowErrorBtn;
        [SerializeField] private GameObject ClassPreset;
        [SerializeField] private GameObject MethodPreset;
        [SerializeField] private GameObject RelationPreset;
        [SerializeField] private GameObject PalleteBtn;
        public MediatorMainPanel MediatorMainPanel;
        public MediatorColorSelectionPanel MediatorColorSelectionPanel;

        public override void OnClicked(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, Slider))
            {
                OnSliderSelected();
            }
            else if (ReferenceEquals(gameObject, Toggle))
            {
                OnToggleValueChanged();
            }
            else if (ReferenceEquals(gameObject, ShowErrorBtn))
            {
                OnShowErrorBtnClicked();
            }
            else if (ReferenceEquals(gameObject, ClassPreset))
            {
                OnClassPresetClicked();
            }
            else if (ReferenceEquals(gameObject, MethodPreset))
            {
                OnMethodPresetClicked();
            }
            else if (ReferenceEquals(gameObject, RelationPreset))
            {
                OnRelationPresetClicked();
            }
            else if (ReferenceEquals(gameObject, PalleteBtn))
            {
                OnPalleteBtnClicked();
            }
            else
            {
                OnClickedDefault(gameObject);
            }
        }

        private void OnSliderSelected()
        {
            MenuManager.Instance.UpdateSpeed();
        }
        private void OnToggleValueChanged()
        {
            MenuManager.Instance.ChangeEdgeHighlighting();
        }
        private void OnShowErrorBtnClicked()
        {
            MenuManager.Instance.ShowErrorPanel();
        }
        private void OnClassPresetClicked()
        {
            AnimationPlay.SetActive(false);
            MediatorColorSelectionPanel.SetActiveColorSelectionPanel(true);
            MediatorColorSelectionPanel.SetPreset("class");
        }
        private void OnMethodPresetClicked()
        {
            AnimationPlay.SetActive(false);
            MediatorColorSelectionPanel.SetActiveColorSelectionPanel(true);
            MediatorColorSelectionPanel.SetPreset("method");
        }
        private void OnRelationPresetClicked()
        {
            AnimationPlay.SetActive(false);
            MediatorColorSelectionPanel.SetActiveColorSelectionPanel(true);
            MediatorColorSelectionPanel.SetPreset("relation");
        }
        private void OnPalleteBtnClicked()
        {
            MediatorColorSelectionPanel.RandomPallete();
        }

        public void SetActiveAnimationPlay(bool active)
        {
            AnimationPlay.SetActive(active);
        }
        public void SetActiveMainPanel(bool active)
        {
            MediatorMainPanel.SetActiveMainPanel(active);
        }
    
    }
}