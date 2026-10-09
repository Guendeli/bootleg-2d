using Bootleg.Ball;
using TurnBasedStrategyFramework.Common.Cells;
using UnityEngine;
using UnityEngine.UI;

namespace Bootleg.UI
{
    /// <summary>
    /// Shows the interception risk published by <see cref="InterceptionRiskPreview"/> as a label above the hovered target,
    /// coloured by severity. Builds its own screen-space canvas; add it to any GameObject in the scene.
    /// </summary>
    public class InterceptionRiskLabel : MonoBehaviour
    {
        [SerializeField] private Vector2 _screenOffset = new Vector2(0f, 48f);
        [SerializeField] private int _fontSize = 22;
        [SerializeField, Range(0f, 1f)] private float _mediumRiskFrom = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _highRiskFrom = 0.5f;
        [SerializeField] private Color _lowRiskColor = new Color(0.55f, 0.9f, 0.55f);
        [SerializeField] private Color _mediumRiskColor = new Color(1f, 0.85f, 0.35f);
        [SerializeField] private Color _highRiskColor = new Color(1f, 0.45f, 0.4f);
        [SerializeField] private Color _backgroundColor = new Color(0f, 0f, 0f, 0.7f);

        private RectTransform _panel;
        private Text _text;
        private ICell _anchorCell;

        private void Awake()
        {
            Build();
            Hide();
        }

        private void OnEnable()
        {
            InterceptionRiskPreview.Shown += Show;
            InterceptionRiskPreview.Hidden += Hide;
        }

        private void OnDisable()
        {
            InterceptionRiskPreview.Shown -= Show;
            InterceptionRiskPreview.Hidden -= Hide;
        }

        private void Show(InterceptionRisk risk)
        {
            _anchorCell = risk.At;
            _text.text = $"Interception {Mathf.RoundToInt(risk.Chance * 100)}% · {risk.Defenders} {(risk.Defenders == 1 ? "defender" : "defenders")}";
            _text.color = risk.Chance >= _highRiskFrom ? _highRiskColor
                : risk.Chance >= _mediumRiskFrom ? _mediumRiskColor
                : _lowRiskColor;
            _panel.gameObject.SetActive(true);
            FollowAnchor();
        }

        private void Hide()
        {
            _anchorCell = null;
            _panel.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            FollowAnchor();
        }

        // On a screen-space overlay canvas, a RectTransform's position is in screen pixels.
        private void FollowAnchor()
        {
            var camera = Camera.main;
            if (_anchorCell == null || camera == null)
            {
                return;
            }
            var world = _anchorCell.WorldPosition;
            var screen = camera.WorldToScreenPoint(new Vector3(world.x, world.y, world.z));
            _panel.position = (Vector2)screen + _screenOffset;
        }

        private void Build()
        {
            var canvasObject = new GameObject("InterceptionRiskCanvas", typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110;
            // No GraphicRaycaster: the label never blocks clicks on the grid.

            var panelObject = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            panelObject.transform.SetParent(canvasObject.transform, false);
            _panel = (RectTransform)panelObject.transform;
            _panel.pivot = new Vector2(0.5f, 0f);
            var background = panelObject.GetComponent<Image>();
            background.color = _backgroundColor;
            background.raycastTarget = false;

            var layout = panelObject.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 6, 6);
            layout.childControlWidth = layout.childControlHeight = true;
            var fitter = panelObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(panelObject.transform, false);
            _text = textObject.GetComponent<Text>();
            _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _text.fontSize = _fontSize;
            _text.alignment = TextAnchor.MiddleCenter;
            _text.raycastTarget = false;
        }
    }
}
