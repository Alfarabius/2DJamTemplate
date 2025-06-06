﻿using TMPro;
using UnityEngine;

namespace ToolTip
{
    public class TooltipScreenSpaceUI : MonoBehaviour 
    {
        public static TooltipScreenSpaceUI Instance { get; private set; }

        [SerializeField] private RectTransform canvasRectTransform;

        private RectTransform _backgroundRectTransform;
        private TextMeshProUGUI _textMeshPro;
        private RectTransform _rectTransform;

        private System.Func<string> _getTooltipTextFunc;

        private void Awake() {
            Instance = this;

            _backgroundRectTransform = transform.Find("Background").GetComponent<RectTransform>();
            _textMeshPro = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _rectTransform = transform.GetComponent<RectTransform>();

            HideTooltip();
            //TestTooltip();
        }

        private void SetText(string tooltipText) 
        {
            _textMeshPro.SetText(tooltipText);
            _textMeshPro.ForceMeshUpdate();

            Vector2 textSize = _textMeshPro.GetRenderedValues(false);
            Vector2 paddingSize = new Vector2(8, 8);

            _backgroundRectTransform.sizeDelta = textSize + paddingSize;
        }

        private void Update() 
        {
            SetText(_getTooltipTextFunc());

            Vector2 anchoredPosition = Input.mousePosition / canvasRectTransform.localScale.x;

            if (anchoredPosition.x + _backgroundRectTransform.rect.width > canvasRectTransform.rect.width) {
                // Tooltip left screen on right side
                anchoredPosition.x = canvasRectTransform.rect.width - _backgroundRectTransform.rect.width;
            }
            if (anchoredPosition.y + _backgroundRectTransform.rect.height > canvasRectTransform.rect.height) {
                // Tooltip left screen on top side
                anchoredPosition.y = canvasRectTransform.rect.height - _backgroundRectTransform.rect.height;
            }

            _rectTransform.anchoredPosition = anchoredPosition;
        }

        private void ShowTooltip(string tooltipText) 
        {
            ShowTooltip(() => tooltipText);
        }

        private void ShowTooltip(System.Func<string> getTooltipTextFunc) 
        {
            _getTooltipTextFunc = getTooltipTextFunc;
            gameObject.SetActive(true);
            SetText(getTooltipTextFunc());
        }

        private void HideTooltip() 
        {
            gameObject.SetActive(false);
        }

        public static void ShowTooltip_Static(string tooltipText) 
        {
            Instance.ShowTooltip(tooltipText);
        }

        public static void ShowTooltip_Static(System.Func<string> getTooltipTextFunc) 
        {
            Instance.ShowTooltip(getTooltipTextFunc);
        }

        public static void HideTooltip_Static() 
        {
            Instance.HideTooltip();
        }
    }
}
