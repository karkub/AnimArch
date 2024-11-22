using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Visualization.Animation;

namespace Visualization.UI
{
    public class MediatorMasking : Mediator
    {
        [SerializeField] private GameObject ExpandButton;
        [SerializeField] private GameObject CollapseButton;
        [SerializeField] private GameObject Separator;
        [SerializeField] private GameObject Buttons;
        [SerializeField] private GameObject CreateButton;
        [SerializeField] private GameObject LoadButton;
        [SerializeField] private GameObject EditButton;
        [SerializeField] private GameObject Label;
        [SerializeField] private TextMeshProUGUI LabelText;
        [SerializeField] private UnityEngine.UI.Button RemoveButton;
        public MediatorMainPanel MediatorMainPanel;

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
            else if (ReferenceEquals(gameObject, LoadButton))
            {
                OnLoadButtonClicked();
            }
            else if (ReferenceEquals(gameObject, RemoveButton))
            {
                OnRemoveButtonClicked();
            }
            else
            {
                OnClickedDefault(gameObject);
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
        private void OnLoadButtonClicked()
        {
            TooltipManager.Instance.HideTooltip();
            FileLoader.Instance.OpenMaskingFile();
        }
        private void OnRemoveButtonClicked()
        {
            TooltipManager.Instance.HideTooltip();
            MaskingHandler.Instance.RemoveMasking();
        }     
        
    }
}