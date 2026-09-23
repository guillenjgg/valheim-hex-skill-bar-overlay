using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HexSkillBarOverlay.UI
{
    internal sealed class SkillBarDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform _rectTransform;
        private Vector2 _startPosition;
        private Vector2 _startPointerPosition;

        internal Action<Vector2> DragEnded { get; set; }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_rectTransform == null)
            {
                return;
            }

            _startPosition = _rectTransform.anchoredPosition;
            _startPointerPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_rectTransform == null)
            {
                return;
            }

            Vector2 pointerDelta = eventData.position - _startPointerPosition;
            _rectTransform.anchoredPosition = _startPosition + pointerDelta;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_rectTransform == null)
            {
                return;
            }

            DragEnded?.Invoke(_rectTransform.anchoredPosition);
        }
    }
}