using System;
using UnityEngine;

namespace MetalPod.Core
{
    public enum GameState
    {
        MainMenu,
        Workshop,
        Loading,
        Racing,
        Paused,
        Results
    }

    public class GameStateManager : MonoBehaviour
    {
        public event Action<GameState, GameState> OnBeforeStateChange;
        public event Action<GameState, GameState> OnGameStateChanged;

        [field: SerializeField] public GameState CurrentState { get; private set; } = GameState.MainMenu;

        [SerializeField] private bool logInvalidTransitions = true;

        private GameState _lastNonPauseState = GameState.MainMenu;

        public bool CanTransitionTo(GameState newState)
        {
            return IsValidTransition(CurrentState, newState);
        }

        public void SetState(GameState newState)
        {
            if (newState == CurrentState)
            {
                return;
            }

            if (!IsValidTransition(CurrentState, newState))
            {
                if (logInvalidTransitions)
                {
                    Debug.LogWarning($"Invalid game state transition: {CurrentState} -> {newState}");
                }

                return;
            }

            GameState previous = CurrentState;
            OnBeforeStateChange?.Invoke(previous, newState);

            if (newState == GameState.Paused)
            {
                _lastNonPauseState = previous;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }

            CurrentState = newState;
            OnGameStateChanged?.Invoke(previous, newState);
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Racing)
            {
                SetState(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                SetState(_lastNonPauseState);
            }
        }

        private static bool IsValidTransition(GameState from, GameState to)
        {
            switch (from)
            {
                case GameState.MainMenu:
                    return to == GameState.Workshop || to == GameState.Loading;

                case GameState.Workshop:
                    return to == GameState.MainMenu || to == GameState.Loading;

                case GameState.Loading:
                    return to == GameState.MainMenu || to == GameState.Workshop || to == GameState.Racing;

                case GameState.Racing:
                    return to == GameState.Paused || to == GameState.Results || to == GameState.Loading;

                case GameState.Paused:
                    return to == GameState.Racing || to == GameState.Results || to == GameState.MainMenu;

                case GameState.Results:
                    return to == GameState.MainMenu || to == GameState.Workshop || to == GameState.Loading;

                default:
                    return false;
            }
        }
    }
}
