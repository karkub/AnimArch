using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Visualization.UI
{
    public class ScrollableMethodList : MonoBehaviour
    {
        public GameObject MethodPrefabButton;
        public Transform ButtonParent;
        private readonly List<GameObject> Buttons = new();
        private List<string> Items = new();
        public ScrollableListState CurrentState { get; set; }
        private void Start()
        {   
            Items = new();
        }

        public void FillItems(List<string> items)
        {
            Items = new List<string>(items);
            Refresh();
        }

        public void Refresh()
        {
            foreach (GameObject button in Buttons)
            {
                Destroy(button); 
            }
            Buttons.Clear();

            ConstructButtons();
        }

        public void ClearItems()
        {
            foreach (GameObject button in Buttons)
            {
                Destroy(button); 
            }
            Buttons.Clear();
            Items.Clear();
        }

        private void ConstructButtons()
        {
            if (Items == null)
            {
                return;
            }
            foreach (string item in Items)
            {
                GameObject button = Instantiate(MethodPrefabButton, ButtonParent);
                button.GetComponentInChildren<TextMeshProUGUI>().text = item;
                
                CurrentState.HandleButtonClick(item, button.GetComponent<Button>());
  
                button.SetActive(true);
                Buttons.Add(button);
            }
        }
    }

    public abstract class ScrollableListState
    {
        public void HandleButtonClick(string item, Button button){
            button.onClick.AddListener(() => HandleButtonClickAction(item));
        }

        protected abstract void HandleButtonClickAction(string item);
    }

    public class EditModeState : ScrollableListState
    {
        private static EditModeState instance;
        public static EditModeState Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EditModeState();
                }
                return instance;
            }
        }

        protected override void HandleButtonClickAction(string item)
        {
            MenuManager.Instance.SelectMethod(item);
        }
    }

    public class PlayModeState : ScrollableListState
    {
        private static PlayModeState instance;
        public static PlayModeState Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlayModeState();
                }
                return instance;
            }
        }

        protected override void HandleButtonClickAction(string item)
        {
            MenuManager.Instance.SelectPlayMethod(item);
        }
    }
}