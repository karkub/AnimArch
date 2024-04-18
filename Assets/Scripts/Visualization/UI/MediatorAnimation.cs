using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Visualisation.Animation;
using Visualization.Animation;

namespace Visualization.UI
{
    public class MediatorAnimation : Mediator
    {
        [SerializeField] private GameObject ExpandButton;
        [SerializeField] private GameObject CollapseButton;
        [SerializeField] private GameObject Separator;
        [SerializeField] private GameObject Buttons;
        [SerializeField] private GameObject CreateButton;
        [SerializeField] private GameObject LoadButton;
        [SerializeField] private GameObject EditButton;
        [SerializeField] private GameObject PlayButton;
        [SerializeField] private GameObject Label;
        [SerializeField] private TextMeshProUGUI LabelText;
        [SerializeField] private UnityEngine.UI.Button RemoveButton;
        public MediatorMainPanel MediatorMainPanel;
        public MediatorCodePanel MediatorCodePanel;

        public override void OnClicked(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, ExpandButton))
            {
                OnExpandButtonClicked();
            }
            else if (ReferenceEquals(gameObject, CollapseButton))
            {
                OnCollapseButtonClicked();
            }
            else if (ReferenceEquals(gameObject, CreateButton))
            {
                OnCreateButtonClicked();
            }
            else if (ReferenceEquals(gameObject, LoadButton))
            {
                OnLoadButtonClicked();
            }
            else if (ReferenceEquals(gameObject, EditButton))
            {
                OnEditButtonClicked();
            }
            else if (ReferenceEquals(gameObject, PlayButton))
            {
                OnPlayButtonClicked();
            }
            else if (ReferenceEquals(gameObject, RemoveButton))
            {
                OnRemoveButtonClicked();
            }
        }

        private void OnExpandButtonClicked()
        {
            TooltipManager.Instance.HideTooltip();
            Separator.SetActive(true);
            Buttons.SetActive(true);
            Label.SetActive(true);
            ExpandButton.SetActive(false);
            CollapseButton.SetActive(true);
        }
        private void OnCollapseButtonClicked()
        {
            TooltipManager.Instance.HideTooltip();
            Separator.SetActive(false);
            Buttons.SetActive(false);
            Label.SetActive(false);
            ExpandButton.SetActive(true);
            CollapseButton.SetActive(false);
        }
        private void OnCreateButtonClicked()
        {
            TooltipManager.Instance.HideTooltip();
            MenuManager.Instance.StartAnimate();
            MenuManager.Instance.InitializeAnim();
        }
        private void OnLoadButtonClicked()
        {
            TooltipManager.Instance.HideTooltip();
            FileLoader.Instance.OpenAnimation();
        }
        //TODO chcem zopakovat vsetko a len vymenim cestu k animacii
        
        private void OnEditButtonClicked()
        {
            TooltipManager.Instance.HideTooltip();
            MenuManager.Instance.OpenAnimation();
            MediatorCodePanel.UpdateColors();
        }
        private void OnPlayButtonClicked()
        {
            TooltipManager.Instance.HideTooltip();
            MenuManager.Instance.PlayAnimation();
        }
        private void OnRemoveButtonClicked()
        {
            LabelText.text = "";
            RemoveButton.interactable = false;
            TooltipManager.Instance.HideTooltip();
            AnimationData.Instance.RemoveAnimation();
        }

    }
}