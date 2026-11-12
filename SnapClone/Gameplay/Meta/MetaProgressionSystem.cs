using System;
using SnapClone.Gameplay.Run;
using SnapClone.Persistence;

namespace SnapClone.Gameplay.Meta
{
    /// <summary>
    /// Formula y cobro de los puntos globales de una run. El award es
    /// idempotente por <see cref="ProfileDto.LastAwardedRunId"/> y solo se
    /// considera cobrado cuando la escritura del perfil termina bien.
    /// </summary>
    public class MetaProgressionSystem
    {
        public const int PointsPerNormalCombat = 3;
        public const int PointsPerSubBoss = 40;
        public const int PointsPerLevel = 8;
        public const int FinalBossBonus = 150;

        public PointsBreakdown Compute(RunState run)
        {
            if (run == null) return new PointsBreakdown();

            int normalCombats = Math.Max(0, run.NormalCombatsWon);
            int subBosses = Math.Max(0, run.SubBossesWon);
            int level = Math.Max(1, run.Level);
            bool finalBoss = run.FinalBossDefeated || run.Outcome == RunOutcome.Won;

            return new PointsBreakdown
            {
                NormalCombats = normalCombats,
                NormalCombatPoints = PointsPerNormalCombat * normalCombats,
                SubBosses = subBosses,
                SubBossPoints = PointsPerSubBoss * subBosses,
                Level = level,
                LevelPoints = PointsPerLevel * level,
                FinalBossBonus = finalBoss ? FinalBossBonus : 0
            };
        }

        /// <summary>
        /// Cobra la run una sola vez. Si <paramref name="save"/> devuelve false
        /// o lanza, los cambios en memoria se revierten para permitir reintentar.
        /// </summary>
        public AwardResult Award(RunState run, ProfileDto profile,
            Func<ProfileDto, bool> save)
        {
            if (run == null || profile == null)
                return new AwardResult(AwardStatus.Failed, new PointsBreakdown(),
                    "No hay run o perfil para cobrar.");

            var breakdown = Compute(run);
            string runId = run.EnsureRunId();

            if (string.Equals(profile.LastAwardedRunId, runId, StringComparison.Ordinal))
                return new AwardResult(AwardStatus.AlreadyCollected, breakdown, null);

            int previousPoints = profile.Points;
            string previousRunId = profile.LastAwardedRunId;

            profile.Points = previousPoints + breakdown.Total;
            profile.LastAwardedRunId = runId;

            try
            {
                if (save != null && !save(profile))
                {
                    profile.Points = previousPoints;
                    profile.LastAwardedRunId = previousRunId;
                    return new AwardResult(AwardStatus.Failed, breakdown,
                        "No se pudo guardar el perfil. Reintenta el cobro.");
                }
            }
            catch (Exception ex)
            {
                profile.Points = previousPoints;
                profile.LastAwardedRunId = previousRunId;
                return new AwardResult(AwardStatus.Failed, breakdown,
                    "No se pudo guardar el perfil: " + ex.Message);
            }

            return new AwardResult(AwardStatus.Awarded, breakdown, null);
        }
    }

    public enum AwardStatus
    {
        Awarded,
        AlreadyCollected,
        Failed
    }

    public sealed class AwardResult
    {
        public AwardResult(AwardStatus status, PointsBreakdown breakdown, string error)
        {
            Status = status;
            Breakdown = breakdown;
            Error = error;
        }

        public AwardStatus Status { get; }
        public PointsBreakdown Breakdown { get; }
        public string Error { get; }
        public bool Succeeded =>
            Status == AwardStatus.Awarded || Status == AwardStatus.AlreadyCollected;
    }

    public sealed class PointsBreakdown
    {
        public int NormalCombats { get; set; }
        public int NormalCombatPoints { get; set; }
        public int SubBosses { get; set; }
        public int SubBossPoints { get; set; }
        public int Level { get; set; }
        public int LevelPoints { get; set; }
        public int FinalBossBonus { get; set; }

        public int Total => NormalCombatPoints + SubBossPoints + LevelPoints + FinalBossBonus;
    }
}
