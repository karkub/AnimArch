using UnityEngine;
using UnityEngine.UIElements;
using Visualization.Animation;
using Visualization.TODO;

namespace Visualization.UI
{
    public class MediatorColorSelectionPanel : Mediator
    {
        [SerializeField] private GameObject ColorSelectionPanel;
        [SerializeField] private GameObject ButtonExit;
        [SerializeField] private GameObject ButtonConfirm;
        public MediatorAnimationPlay MediatorAnimationPlay;

        public override void OnClicked(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, ButtonExit))
            {
                OnButtonExitClicked();
            }
            else if (ReferenceEquals(gameObject, ButtonConfirm))
            {
                OnButtonConfirmClicked();
            }
            else
            {
                OnClickedDefault(gameObject);
            }
        }

        private void OnButtonExitClicked()
        {
            ColorSelectionPanel.SetActive(false);
            MediatorAnimationPlay.SetActiveAnimationPlay(true);
        }
        private void OnButtonConfirmClicked()
        {
            ColorSelectionPanel.GetComponent<SelectColorFromPallete>().SetColor();
            ColorSelectionPanel.SetActive(false);
            MediatorAnimationPlay.SetActiveAnimationPlay(true);
        }

        public void SetActiveColorSelectionPanel(bool active)
        {
            ColorSelectionPanel.SetActive(active);
        }
        public void SetPreset(string preset)
        {
            ColorSelectionPanel.GetComponent<SelectColorFromPallete>().SetPreset(preset);
        }
        public void RandomPallete()
        {
            ColorSelectionPanel.GetComponent<SelectColorFromPallete>().RandomPallete();
        }

    }
}