using System.Collections.Generic;

namespace SnapClone.Core.StateMachine
{
    public class StateMachine
    {
        private Dictionary<string, IGameState> _states = new Dictionary<string, IGameState>();
        private IGameState _currentState;
        private string _currentStateName;
        
        public void AddState(string name, IGameState state)
        {
            _states[name] = state;
        }
        
        public void ChangeState(string name)
        {
            if (_currentState != null)
            {
                _currentState.Exit();
            }
            
            if (_states.TryGetValue(name, out IGameState newState))
            {
                _currentState = newState;
                _currentStateName = name;
                _currentState.Enter();
            }
        }
        
        public void Update(float deltaTime)
        {
            _currentState?.Update(deltaTime);
        }
        
        public void Draw()
        {
            _currentState?.Draw();
        }
        
        public string CurrentStateName => _currentStateName;
    }
}
