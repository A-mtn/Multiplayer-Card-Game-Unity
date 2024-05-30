namespace TurnSystem
{
    public class CardPlayState : IGameState
    {
        public int GetID()
        {
            return 2;
        }

        public void OnEnter(TurnManager turnManager)
        {
            turnManager.ClearPlayedCards();
        }

        public void OnExit(TurnManager turnManager)
        {
            
        }

        public void HandleTurn(TurnManager turnManager)
        {
            
        }
    }
}