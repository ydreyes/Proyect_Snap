namespace SnapClone.Gameplay.Run
{
    /// <summary>
    /// Nodos del grafo de la run. El director es el unico que decide cual
    /// sigue; las escenas solo abren el nodo pendiente.
    /// </summary>
    public enum RunNode
    {
        Draft,
        EnemySelect,
        Combat,
        LevelUp,
        Shop,
        Event,
        ActTransition,
        SubBossIntro,
        Summary
    }
}
