using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Visualization.Animation;
using Visualization.ClassDiagram;


namespace Visualization.UI
{
    public class MediatorEnterParameterPopUp : Mediator
    {
        [SerializeField] private GameObject EnterParameterPopUp;
        [SerializeField] private GameObject MethodLabel;
        [SerializeField] public GameObject Content;
        [SerializeField] private GameObject CloseButton;
        [SerializeField] private GameObject SaveButton;

        public override void OnClicked(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, CloseButton))
            {
                OnCloseButtonClicked();
            }
            else if (ReferenceEquals(gameObject, SaveButton))
            {
                OnSaveButtonClicked();
            }
        }

        private void OnCloseButtonClicked()
        {
            EnterParameterPopUp.SetActive(false);
            Animation.Animation.Instance.HighlightClass(Animation.Animation.Instance.startClassName, false);
        }
        private void OnSaveButtonClicked()
        {
            MenuManager.Instance.SaveParametersForInitialMethod();
        }

        public void SetActiveEnterParameterPopUp(bool active)
        {
            EnterParameterPopUp.SetActive(active);
        }

        public void SetMethodLabelText(string text)
        {
            MethodLabel.GetComponent<TMP_Text>().text = text;
        }

        public string GetMethodLabelText()
        {
            return MethodLabel.GetComponent<TMP_Text>().text;
        }
    }
}