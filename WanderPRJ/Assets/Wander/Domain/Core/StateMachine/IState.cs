namespace Wander.Domain.Core.StateMachine 
{
    public interface IState {
        void Enter();
        void Tick();
        void FixedTick();
        void Exit();
    }
}

