using TMPro;
using UnityEngine;
using System.Linq;
using AnimArch.Extensions;
using UnityEngine.UI;
using Visualization.Animation;

namespace Visualization.UI
{
    public class ConsolePanel : Singleton<ConsolePanel>
    {
        public GameObject inputField;
        public GameObject outputField;

        private TMP_InputField tmpInpField;
        private TMP_InputField tmpOutpField;
        private Scrollbar tmpScrollbar;

        private ConsoleRequestRead CurrentRequest;

        private void Awake()
        {
            tmpInpField = inputField.GetComponent<TMP_InputField>();
            tmpOutpField = outputField.GetComponent<TMP_InputField>();
            tmpScrollbar = tmpOutpField.verticalScrollbar.GetComponent<Scrollbar>();
            this.CurrentRequest = null;
            DeActivateInputField();
        }


        public void YieldOutput(string output, ConsoleRequestWrite request)
        {
            tmpOutpField.text += output + "\n";

            // Scroll to bottom
            tmpScrollbar.value = 1.0f;

            if (request != null)
            {
                request.Done = true;
            }
        }

        public void ActivateInputField(ConsoleRequestRead request)
        {
            tmpInpField.interactable = true;
            this.CurrentRequest = request;
        }
        private void DeActivateInputField()
        {
            tmpInpField.interactable = false;
        }

        public void InputEntered(string enteredText)
        {
            DeActivateInputField();

            tmpOutpField.text += enteredText + "\n";
            tmpInpField.text = "";

            ConsoleRequestRead currentRequest = CurrentRequest;
            CurrentRequest = null;
            
            currentRequest.ReadValue = enteredText;
            currentRequest.Done = true;

            if (currentRequest.Request != null) 
            {
                currentRequest.Request.ReadValue = enteredText;
                currentRequest.Request.Done = true;
            }
            
        }
    }
}
