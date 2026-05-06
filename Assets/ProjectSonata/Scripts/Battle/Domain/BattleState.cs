namespace ProjectSonata.Battle.Domain
{
    public sealed class BattleState
    {
        public BattleState(BattleUnit[] playerUnits, BattleUnit[] enemyUnits, Tonality startingTonality)
        {
            PlayerUnits = playerUnits ?? System.Array.Empty<BattleUnit>();
            EnemyUnits = enemyUnits ?? System.Array.Empty<BattleUnit>();
            StartingTonality = startingTonality;
            CurrentTurnIndex = 0;
        }

        public BattleUnit[] PlayerUnits { get; }
        public BattleUnit[] EnemyUnits { get; }
        public Tonality StartingTonality { get; }
        public int CurrentTurnIndex { get; private set; }

        public BattleUnit CurrentPlayerUnit
        {
            get
            {
                if (PlayerUnits.Length == 0)
                {
                    return null;
                }

                var clampedIndex = CurrentTurnIndex;
                if (clampedIndex < 0)
                {
                    clampedIndex = 0;
                }

                if (clampedIndex >= PlayerUnits.Length)
                {
                    clampedIndex = PlayerUnits.Length - 1;
                }

                return PlayerUnits[clampedIndex];
            }
        }

        public void SetCurrentTurnIndex(int index)
        {
            if (PlayerUnits.Length == 0)
            {
                CurrentTurnIndex = 0;
                return;
            }

            if (index < 0)
            {
                index = 0;
            }

            if (index >= PlayerUnits.Length)
            {
                index = PlayerUnits.Length - 1;
            }

            CurrentTurnIndex = index;
        }

        public static BattleState FromEncounter(BattleEncounterDefinition encounter)
        {
            var playerUnits = BuildPlayerUnits(encounter);
            var enemyUnits = BuildEnemyUnits(encounter);
            return new BattleState(playerUnits, enemyUnits, encounter.StartingTonality);
        }

        private static BattleUnit[] BuildPlayerUnits(BattleEncounterDefinition encounter)
        {
            if (encounter.PlayerParty == null || encounter.PlayerParty.Length == 0)
            {
                return System.Array.Empty<BattleUnit>();
            }

            var units = new BattleUnit[encounter.PlayerParty.Length];
            for (var i = 0; i < encounter.PlayerParty.Length; i++)
            {
                var definition = encounter.PlayerParty[i];
                units[i] = definition == null ? null : BattleUnit.FromCharacter(definition);
            }

            return units;
        }

        private static BattleUnit[] BuildEnemyUnits(BattleEncounterDefinition encounter)
        {
            if (encounter.EnemyParty == null || encounter.EnemyParty.Length == 0)
            {
                return System.Array.Empty<BattleUnit>();
            }

            var units = new BattleUnit[encounter.EnemyParty.Length];
            for (var i = 0; i < encounter.EnemyParty.Length; i++)
            {
                var definition = encounter.EnemyParty[i];
                units[i] = definition == null ? null : BattleUnit.FromEnemy(definition);
            }

            return units;
        }
    }
}