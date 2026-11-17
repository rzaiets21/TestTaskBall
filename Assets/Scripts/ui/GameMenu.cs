using DG.Tweening;
using TestTaskBall.controllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestTaskBall.ui {
    public class GameMenu : MonoBehaviour {
        private const float SHOW_PANEL_DURATION = 0.7f;
        
        [SerializeField] private GameController _gameController;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private string _winText;
        [SerializeField] private string _loseText;
        [SerializeField] private Button _restartButton;

        private Tween _showTween;

        private void Start() {
            ShowPanel(false, true);
            _gameController.OnGameFinished += OnGameFinished;
            _restartButton.onClick.AddListener(OnRestartClick);
        }

        private void OnDestroy() {
            _showTween?.Kill();
            _showTween = null;
            _gameController.OnGameFinished -= OnGameFinished;
        }

        private void OnRestartClick() {
            ShowPanel(false, true);
            _gameController.Restart();
        }

        private void ShowPanel(bool state, bool immediately = false) {
            _showTween?.Kill();
            var targetAlpha = state ? 1f : 0f;
            if (immediately) {
                _canvasGroup.alpha = targetAlpha;
                _canvasGroup.interactable = state;
                _canvasGroup.blocksRaycasts = state;
                return;
            }
            
            _canvasGroup.alpha = 1f - targetAlpha;
            _canvasGroup.interactable = !state;
            _canvasGroup.blocksRaycasts = !state;
            
            _showTween = DOVirtual.Float(_canvasGroup.alpha, targetAlpha, SHOW_PANEL_DURATION, a => {
                _canvasGroup.alpha = a;
            }).OnComplete(() => {
                _canvasGroup.interactable = state;
                _canvasGroup.blocksRaycasts = state;
            });
        }
        
        private void OnGameFinished(bool win) {
            _titleText.text = win ? _winText : _loseText;
            ShowPanel(true);
        }
    }
}