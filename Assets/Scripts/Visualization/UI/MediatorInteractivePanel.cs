using UnityEngine;
using UnityEngine.UIElements;
using Visualization.Animation;
using Visualization.TODO;

namespace Visualization.UI
{
    public class MediatorInteractivePanel : Mediator
    {
        [SerializeField] private GameObject InteractivePanel;

        public override void OnClicked(GameObject gameObject)
        {
            OnClickedDefault(gameObject);
        }

    }
}