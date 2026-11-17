using TestTaskBall.controllers;
using TestTaskBall.game;
using UnityEngine;

namespace TestTaskBall {
    public class GameStartup : MonoBehaviour {
        [SerializeField] private BallPlayer _playerPrefab;
        [SerializeField] private Transform _playerPosition;
        [SerializeField] private BallBullet _ballBulletPrefab;
        [SerializeField] private ObstacleView _obstaclePrefab;
        [SerializeField] private FinishPoint _finishPointPrefab;
        [SerializeField] private Transform _finishPointContainer;
        [SerializeField] private GameController _gameController;

        private void Start() {
            var player = Instantiate(_playerPrefab, _playerPosition.position, Quaternion.identity);
            var finishPoint = Instantiate(_finishPointPrefab, _finishPointContainer);
            _gameController.Init(player, _ballBulletPrefab, _obstaclePrefab, finishPoint);
        }
    }
}