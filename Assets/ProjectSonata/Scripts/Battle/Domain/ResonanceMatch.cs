using System.Collections.Generic;

namespace ProjectSonata.Battle.Domain
{
    public sealed class ResonanceMatch
    {
        public static readonly ResonanceMatch Empty =
            new ResonanceMatch(ResonanceType.None, System.Array.Empty<Tone>());

        public ResonanceType Type { get; }
        public IReadOnlyList<Tone> Tones { get; }

        public ResonanceMatch(ResonanceType type, IReadOnlyList<Tone> tones)
        {
            Type = type;
            Tones = tones ?? System.Array.Empty<Tone>();
        }

        public bool IsDetected => Type != ResonanceType.None;
    }
}
