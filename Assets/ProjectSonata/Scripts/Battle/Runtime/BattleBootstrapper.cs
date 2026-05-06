using ProjectSonata.Battle.Domain;
using UnityEngine;

namespace ProjectSonata.Battle.Runtime
{
    public sealed class BattleBootstrapper : MonoBehaviour
    {
        [SerializeField] private BattleEncounterDefinition encounter;
        [SerializeField] private BattleController battleController;

        private void Start()
        {
            if (encounter == null)
            {
                Debug.LogError("BattleBootstrapper is missing an encounter asset.", this);
                return;
            }

            if (battleController == null)
            {
                Debug.LogError("BattleBootstrapper is missing a BattleController reference.", this);
                return;
            }

            battleController.StartBattle(encounter);
        }
    }
}
