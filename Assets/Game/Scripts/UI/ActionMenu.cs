using System.Collections.Generic;
using System.Linq;
using Bootleg.Units;
using TurnBasedStrategyFramework.Common.Controllers.GameResolvers;
using TurnBasedStrategyFramework.Common.Players;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Unity.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace Bootleg.UI
{
    /// <summary>
    /// Minimal action picker shown while a human player's <see cref="TacticsUnit"/> is selected:
    /// one button per <see cref="TacticsUnit.Actions"/> entry the unit can perform right now, plus Wait.
    /// Builds its own screen-space canvas, so it only needs the grid controller assigned.
    /// The scene needs an EventSystem (BootcampScene has one).
    /// </summary>
    public class ActionMenu : MonoBehaviour
    {
        [SerializeField] private UnityGridController _gridController;

        [Header("Look")]
        [SerializeField] private Vector2 _buttonSize = new Vector2(140, 48);
        [SerializeField] private Color _panelColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private Color _buttonColor = new Color(0.22f, 0.24f, 0.28f);
        [SerializeField] private Color _activeButtonColor = new Color(0.2f, 0.45f, 0.8f);
        [SerializeField] private Color _textColor = Color.white;

        private readonly HashSet<TacticsUnit> _hookedUnits = new HashSet<TacticsUnit>();
        private readonly List<GameObject> _buttons = new List<GameObject>();
        private RectTransform _panel;
        private Font _font;
        private TacticsUnit _shownUnit;

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildCanvas();
            Hide();

            // Units are registered when the game initializes (UnityGridController.Start).
            _gridController.GameInitialized += OnGameInitialized;
            _gridController.GameEnded += OnGameEnded;
        }

        private void OnDestroy()
        {
            if (_gridController != null)
            {
                _gridController.GameInitialized -= OnGameInitialized;
                _gridController.GameEnded -= OnGameEnded;
            }
            foreach (var unit in _hookedUnits.Where(u => u != null))
            {
                unit.UnitSelected -= OnUnitSelected;
                unit.UnitDeselected -= OnUnitDeselected;
            }
        }

        private void OnGameInitialized()
        {
            foreach (var unit in _gridController.UnitManager.GetUnits())
            {
                Hook(unit);
            }
            _gridController.UnitManager.UnitAdded += Hook;
        }

        private void OnGameEnded(GameResult result)
        {
            Hide();
        }

        private void Hook(IUnit unit)
        {
            if (unit is TacticsUnit tacticsUnit && _hookedUnits.Add(tacticsUnit))
            {
                tacticsUnit.UnitSelected += OnUnitSelected;
                tacticsUnit.UnitDeselected += OnUnitDeselected;
            }
        }

        private void OnUnitSelected(IUnit unit)
        {
            var turn = _gridController.TurnContext;
            if (unit is TacticsUnit tacticsUnit
                && tacticsUnit.Actions.Count > 0
                && turn.CurrentPlayer.PlayerType == PlayerType.HumanPlayer
                && turn.PlayableUnits().Contains(unit))
            {
                Show(tacticsUnit);
            }
        }

        private void OnUnitDeselected(IUnit unit)
        {
            if (ReferenceEquals(unit, _shownUnit))
            {
                Hide();
            }
        }

        private void Show(TacticsUnit unit)
        {
            ClearButtons();
            _shownUnit = unit;

            // Only actions the unit can perform right now; Wait is always available.
            var activeIndex = unit.ActiveActionIndex;
            for (var i = 0; i < unit.Actions.Count; i++)
            {
                var index = i;
                var action = unit.Actions[i];
                if (!unit.CanPerform(action, _gridController))
                {
                    continue;
                }
                AddButton(action.Label, isActive: i == activeIndex, onClick: () => unit.SelectAction(index, _gridController));
            }
            AddButton("Wait", isActive: false, onClick: () => unit.Wait(_gridController));

            _panel.gameObject.SetActive(true);
        }

        private void Hide()
        {
            _shownUnit = null;
            _panel.gameObject.SetActive(false);
        }

        private void ClearButtons()
        {
            foreach (var button in _buttons)
            {
                Destroy(button);
            }
            _buttons.Clear();
        }

        private void BuildCanvas()
        {
            var canvasObject = new GameObject("ActionMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var panelObject = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            panelObject.transform.SetParent(canvasObject.transform, false);
            _panel = (RectTransform)panelObject.transform;
            _panel.anchorMin = _panel.anchorMax = new Vector2(0.5f, 0f);
            _panel.pivot = new Vector2(0.5f, 0f);
            _panel.anchoredPosition = new Vector2(0f, 24f);
            panelObject.GetComponent<Image>().color = _panelColor;

            var layout = panelObject.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;

            var fitter = panelObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void AddButton(string label, bool isActive, UnityEngine.Events.UnityAction onClick)
        {
            var buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.transform.SetParent(_panel, false);

            var layoutElement = buttonObject.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = _buttonSize.x;
            layoutElement.preferredHeight = _buttonSize.y;

            var image = buttonObject.GetComponent<Image>();
            image.color = isActive ? _activeButtonColor : _buttonColor;

            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            var textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);
            var textRect = (RectTransform)textObject.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = textRect.offsetMax = Vector2.zero;

            var text = textObject.GetComponent<Text>();
            text.text = label;
            text.font = _font;
            text.fontSize = 22;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = _textColor;

            _buttons.Add(buttonObject);
        }
    }
}
