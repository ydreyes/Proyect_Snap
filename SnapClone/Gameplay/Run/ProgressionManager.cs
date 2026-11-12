namespace SnapClone.Gameplay.Run
{
    /// <summary>
    /// Fachada delgada de compatibilidad sobre <see cref="RunDirector"/>.
    /// No decide la secuencia: solo delega la clasificacion y el enrutado.
    /// </summary>
    public class ProgressionManager
    {
        private readonly RunDirector _director;

        public ProgressionManager(RunDirector director)
        {
            _director = director;
        }

        public CombatKind GetCurrentCombatKind() =>
            _director.GetCurrentCombatKind();

        public string ResolveCombat(string result)
        {
            _director.ApplyCombatResult(result);
            return _director.GetNextScene();
        }

        public string CompleteLevelUp()
        {
            _director.OnLevelUpComplete();
            return _director.GetNextScene();
        }
    }
}
