using UnityEngine;
using UnityEngine.UIElements;
using Visualization.Animation;
using Visualization.TODO;

namespace Visualization.UI
{
    public class MediatorCodePanel : Mediator
    {
        [SerializeField] private GameObject CodePanel;
        [SerializeField] private GameObject ButtonFull;
        [SerializeField] private GameObject ButtonSep;
        [SerializeField] private GameObject ButtonExit;
        [SerializeField] private GameObject FullInput;
        [SerializeField] private GameObject SepInput;
        [SerializeField] private GameObject ButtonCheck;
        [SerializeField] private GameObject ButtonSave;

        private UnityEngine.UI.Button _buttonSaveButtonComponent;

        public override void OnClicked(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, ButtonFull))
            {
                OnButtonFullClicked();
            }
            else if (ReferenceEquals(gameObject, ButtonSep))
            {
                OnButtonSepClicked();
            }
            else if (ReferenceEquals(gameObject, ButtonExit))
            {
                OnButtonExitClicked();
            }
            else if (ReferenceEquals(gameObject, FullInput))
            {
                OnFullInputSelected();
            }
            else if (ReferenceEquals(gameObject, SepInput))
            {
                OnSepInputSelected();
            }
            else if (ReferenceEquals(gameObject, ButtonCheck))
            {
                OnButtonCheckClicked();
            }
            else if (ReferenceEquals(gameObject, ButtonSave))
            {
                OnButtonSaveClicked();
            }
            else
            {
                OnClickedDefault(gameObject);
            }
        }

        private void OnButtonFullClicked()
        {
            FullInput.SetActive(true);
            SepInput.SetActive(false);
        }
        private void OnButtonSepClicked()
        {
            FullInput.SetActive(false);
            SepInput.SetActive(true);
        }
        private void OnButtonExitClicked()
        {
            MenuManager.Instance.EndAnimate();
            Animation.Animation.Instance.UnhighlightAll();
        }
        private void OnFullInputSelected()
        {
            SetInteractableButtonSave(false);
            FullInput.GetComponent<CodeHighlighter>().RemoveColors();
        }
        private void OnSepInputSelected()
        {
            SetInteractableButtonSave(false);
            SepInput.GetComponent<CodeHighlighter>().RemoveColors();
        }
        private void OnButtonCheckClicked()
        {
            CodePanel.GetComponent<CheckScript>().CheckScriptCode();
            FullInput.GetComponent<CodeHighlighter>().UpdateColors();
            SetInteractableButtonSave(true);
        }
        private void OnButtonSaveClicked()
        {
            MenuManager.Instance.SaveAnimation();
        }

        public void UpdateColors()
        {
            FullInput.GetComponent<CodeHighlighter>().UpdateColors();
        }
        public void SetInteractableButtonSave(bool active)
        {
            if (_buttonSaveButtonComponent == null)
            {
                _buttonSaveButtonComponent = ButtonSave.GetComponent<UnityEngine.UI.Button>();
            }

            _buttonSaveButtonComponent.interactable = active;
        }
        
    }
}