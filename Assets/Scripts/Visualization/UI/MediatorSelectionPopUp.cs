using System;
using UnityEngine;
using UnityEngine.UIElements;
using Visualization.Animation;
using Visualization.TODO;
using Visualization.UI.PopUps;

namespace Visualization.UI
{
    public class MediatorSelectionPopUp : Mediator
    {
        [SerializeField] private GameObject SelectionPopUp;

        public override void OnClicked(GameObject gameObject)
        {
            OnClickedDefault(gameObject);
        }

        public void SetActiveSelectionPopUp(bool active)
        {
            SelectionPopUp.SetActive(active);
        }
        public void ActivateCreation()
        {
            SelectionPopUp.GetComponent<SelectionPopUp>().ActivateCreation();
        }

    }
}