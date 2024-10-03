using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OALProgramControl;
using TMPro;


public class ExecutionErrorPanel : MonoBehaviour
{
    [SerializeField] private GameObject ErrorIDText;
    [SerializeField] private GameObject ErrorDescriptionText;
    [SerializeField] private GameObject ErrorCommandTypeText;
    [SerializeField] private GameObject ErrorSourceCodeText;
    private TMP_Text ErrorIDTextComponent;
    private TMP_InputField ErrorDescriptionTextComponent;
    private TMP_Text ErrorCommandTypeTextComponent;
    private TMP_InputField ErrorSourceCodeTextComponent;

    void Awake()
    {
        ErrorIDTextComponent = ErrorIDText.GetComponent<TMP_Text>();
        ErrorDescriptionTextComponent = ErrorDescriptionText.GetComponent<TMP_InputField>();
        ErrorCommandTypeTextComponent = ErrorCommandTypeText.GetComponent<TMP_Text>();
        ErrorSourceCodeTextComponent = ErrorSourceCodeText.GetComponent<TMP_InputField>();
    }

    public void FillPanel(EXEExecutionResult executionSuccess)
    {
        if (executionSuccess == null)
        {
            return;
        }
        VisitorCommandToString visitor = new VisitorCommandToString();

        ErrorIDTextComponent.text = executionSuccess.ErrorCode;
        ErrorDescriptionTextComponent.text = executionSuccess.ErrorMessage;
        ErrorCommandTypeTextComponent.text = executionSuccess.OwningCommand.GetType().Name;
        executionSuccess.OwningCommand.Accept(visitor);

        ErrorSourceCodeTextComponent.text = visitor.GetCommandString();
    }
}
