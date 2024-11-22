using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace Visualization.UI
{
    public class MethodParameterManager : MonoBehaviour
    {
        [SerializeField] public GameObject ParameterName;
        [SerializeField] public GameObject ParameterType;
        [SerializeField] public GameObject ParameterValue;
        [SerializeField] public GameObject ParameterValueText;
        [SerializeField] public GameObject PlaceholderText;
        [SerializeField] public GameObject ErrorLabel;
        [SerializeField] public GameObject WarningLabel;

        public StringVariable parameterType;

        public void SetErrorLabelText(string type)
        {
            LocalizedString localizedString = ErrorLabel.GetComponent<LocalizeStringEvent>().StringReference;
            StringVariable variable = localizedString["type"] as StringVariable;
            variable.Value = type;
        }
        public void SetPlaceholderText(string defaultValue)
        {
            LocalizedString localizedString = PlaceholderText.GetComponent<LocalizeStringEvent>().StringReference;
            StringVariable variable = localizedString["defaultValue"] as StringVariable;
            variable.Value = defaultValue;
        }

    }
}