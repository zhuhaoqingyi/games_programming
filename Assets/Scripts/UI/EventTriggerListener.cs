using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class EventTriggerListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public delegate void VoidDelegate();
        
        public event VoidDelegate OnEnter;
        public event VoidDelegate OnExit;
        public event VoidDelegate OnDown;
        public event VoidDelegate OnUp;

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnExit?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnUp?.Invoke();
        }
    }
}