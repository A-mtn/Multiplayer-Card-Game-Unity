namespace TurnSystem
{
    public interface IGameState
    {
        int GetID();
        void OnEnter(TurnManager turnManager);
        void OnExit(TurnManager turnManager);
        void HandleTurn(TurnManager turnManager);
    }
}


