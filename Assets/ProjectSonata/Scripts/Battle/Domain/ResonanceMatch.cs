using System.Collections.Generic;

namespace ProjectSonata.Battle.Domain
{
    public sealed class ResonanceMatch
    {
        public static readonly ResonanceMatch Empty =
            new ResonanceMatch(ResonanceType.None, System.Array.Empty<Tone>(), ResonanceFormationPath.Direct);

        public ResonanceType Type { get; }
        public IReadOnlyList<Tone> Tones { get; }
        public ResonanceFormationPath FormationPath { get; }

        public ResonanceMatch(ResonanceType type, IReadOnlyList<Tone> tones)
            : this(type, tones, ResonanceFormationPath.Direct)
        {
        }

        public ResonanceMatch(ResonanceType type, IReadOnlyList<Tone> tones, ResonanceFormationPath formationPath)
        {
            Type = type;
            Tones = tones ?? System.Array.Empty<Tone>();
            FormationPath = formationPath;
        }

        public bool IsDetected => Type != ResonanceType.None;
    }
}
