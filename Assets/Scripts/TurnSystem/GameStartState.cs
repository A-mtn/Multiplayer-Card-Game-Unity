namespace TurnSystem
{
    public class GameStartState : IGameState
    {
        public int GetID()
        {
            return 1;
        }
        public void OnEnter(TurnManager turnManager)
        {
            turnManager.SpawnInitialPrefabsForPlayers();
        }

        public void OnExit(TurnManager turnManager)
        {
            
        }

        public void HandleTurn(TurnManager turnManager)
        {
            
        }
    } 
}