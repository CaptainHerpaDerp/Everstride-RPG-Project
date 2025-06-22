using UnityEngine;

namespace UtilityAI.Core
{
    public class AIBrain : MonoBehaviour
    {
        public Action BestAction { get; set; }


        private NPCController npcController;

        public bool FinishedDeciding {get; set;}
        public bool FinishedExcetuting { get; set;}

        [SerializeField] private Action[] availableActions;

        private void Start()
        {
            npcController = GetComponent<NPCController>();
            FinishedDeciding = false;
            FinishedExcetuting = false;
        }

        private void Update()
        {
            //if (BestAction is null)
            //{
            //    EvaluateActions(availableActions);
            //}
        }

        public void EvaluateActions()
        {
            float bestScore = float.MinValue;
            int bestActionIndex = 0;

            for (int i = 0; i < availableActions.Length; i++)
            {
                if (ScoreAction(availableActions[i]) > bestScore)
                {
                    bestActionIndex = i;
                    bestScore = availableActions[i].Score;
                }
            }

            BestAction = availableActions[bestActionIndex];
            BestAction.SetRequiredDestination(npcController);

            FinishedDeciding = true;
        }

        public float ScoreAction(Action action)
        {
            float score = 1f;

            for (int i = 0; i < action.considerations.Length; i++)
            {
                float considerationScore = action.considerations[i].ScoreConsideration(npcController);
                score *= considerationScore;

                if (score == 0)
                {
                    action.Score = 0;
                    return action.Score;
                }
            }

            // Average the score
            float originalScore = score;

            float modFactor = 1 - (1 / action.considerations.Length);
            float makeupValue = (1 - originalScore) * modFactor;
            action.Score = originalScore + (makeupValue * originalScore);

            return action.Score;
        }
    }
}

