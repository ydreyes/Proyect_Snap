namespace SnapClone.Core.StateMachine
{
    public interface IGameState
    {
        void Enter();
        void Update(float deltaTime);
        void Draw();
        void Exit();
    }
}
