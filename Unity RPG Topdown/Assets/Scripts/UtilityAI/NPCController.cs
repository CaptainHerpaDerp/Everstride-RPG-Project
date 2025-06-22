using UnityEngine;
using Characters;
using System.Collections;
using InventoryManagement;
using UIElements;

namespace UtilityAI.Core
{
    public enum State
    {
        Deciding, Moving, Excecuting
    }

    public class NPCController : MonoBehaviour
    {
        [SerializeField] private NPC parentNPC;
        [SerializeField] private Mover mover;
        [SerializeField] private AIBrain aiBrain;
        [SerializeField] private Inventory inventory;

        [SerializeField] private Billboard billboard;

        public Stats stats;
        private State currentState;

        private void Update()
        {
            //if (aiBrain.FinishedDeciding)
            //{
            //    aiBrain.FinishedDeciding = false;
            //    aiBrain.BestAction.Execute(this);
            //}

            FSMTick();

        }

        /// <summary>
        /// Finite State Machine Tick
        /// </summary>
        public void FSMTick()
        {
           if (currentState == State.Deciding)
           {
                aiBrain.EvaluateActions();

                if (Vector3.Distance(aiBrain.BestAction.RequiredDestination.position, this.transform.position) < 1f)
                {
                    currentState = State.Excecuting;
                }
                else
                {
                    currentState = State.Moving;
                }
            }
            else if (currentState == State.Moving)
            {
                if (Vector3.Distance(aiBrain.BestAction.RequiredDestination.position, this.transform.position) < 1f)
                {
                    currentState = State.Excecuting;
                }
                else
                {
                    mover.MoveTo(aiBrain.BestAction.RequiredDestination.position);
                }

            }
            else if (currentState == State.Excecuting)
            {
                // Execute the action
                //aiBrain.BestAction.Execute(this);
                if (!aiBrain.FinishedExcetuting)
                {
                    Debug.Log("Executing action");
                    aiBrain.BestAction.Execute(this);
                }
                else if (aiBrain.FinishedExcetuting)
                {
                    //Debug.Log("Exit execute state");
                    currentState = State.Deciding;
                }

                currentState = State.Moving;
            }
        }

        public void OnFinishedAction()
        {
            aiBrain.EvaluateActions();
        }

        #region Coroutines

        public void DoWork(int time)
        {
            billboard.UpdateJobText("Working...");
            StartCoroutine(WorkCoroutine(time));
        }

        public void DoSleep(int time)
        {
            billboard.UpdateJobText("Sleeping...");
            StartCoroutine(SleepCoroutine(time));
        }

        private IEnumerator WorkCoroutine(int time)
        {
            int counter = time;
            while (counter > 0)
            {
                yield return new WaitForSeconds(1);
                counter--;
            }

            Debug.Log("Work done!");

            stats.Energy -= 1;
            stats.Money += 5;

            // Logic to add items to the NPC's inventory

            // Decide on the next action
            aiBrain.FinishedExcetuting = true;
        }

        private IEnumerator SleepCoroutine(int time)
        {
            int counter = time;
            while (counter > 0)
            {
                Debug.Log("Sleeping...");
                yield return new WaitForSeconds(1);
                counter--;
            }

            Debug.Log("Sleep done!");

            stats.Energy += 1;

            // Logic to update the NPC's energy

            // Decide on the next action
            aiBrain.FinishedExcetuting = true;
        }

        #endregion
    }
}