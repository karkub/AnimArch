using System;
using UnityEngine;

namespace Visualization.UI
{
    public abstract class Mediator : MonoBehaviour
    {
        public abstract void OnClicked(GameObject element);
        protected void OnClickedDefault(GameObject element)
        {
            throw new System.Exception(String.Format("Unknown element: {0} was clicked.", element));
        }
    }
}