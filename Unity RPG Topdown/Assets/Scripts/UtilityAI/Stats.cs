using UIElements;
using UnityEngine;

namespace UtilityAI.Core
{

    public class Stats : MonoBehaviour
    {
        private int energy;
        public int Energy
        {
            get { return energy; }
            set
            {
                energy = Mathf.Clamp(value, 0, 100);
                OnStatValueChanged?.Invoke();
            }
        }

        private int hunger;
        public int Hunger
        {
            get { return hunger; }
            set
            {
                hunger = Mathf.Clamp(value, 0, 100);
                OnStatValueChanged?.Invoke();
            }
        }

        private int money;
        public int Money
        {
            get { return money; }
            set
            {
                money = value;
                OnStatValueChanged?.Invoke();
            }
        }

        [SerializeField] private float timeToDecreaseHunger = 5f;
        [SerializeField] private float timeToDecreaseEnergy = 5f;
        private float timeLeftEnergy;
        private float timeLeftHunger;

        public delegate void StatValueChangedHandler();
        public event StatValueChangedHandler OnStatValueChanged;

        [SerializeField] private Billboard billboard;

        // Start is called before the first frame update
        void Start()
        {
            //hunger = Random.Range(20, 80);
            //energy = Random.Range(20, 80);
            //money = Random.Range(10, 100);

            // Test case: NPC will likely work
            hunger = 0;
            energy = 100;
            money = 50;

            //// Test case: NPC will likely eat
            //hunger = 90;
            //energy = 50;
            //money = 500;

            // Test case: NPC will likely sleep
            //hunger = 0;
            //energy = 10;
            //money = 500;
        }

        private void OnEnable()
        {
            OnStatValueChanged += UpdateDisplayText;
        }

        private void OnDisable()
        {
            OnStatValueChanged -= UpdateDisplayText;
        }

        public void UpdateHunger()
        {
            if (timeLeftHunger > 0)
            {
                timeLeftHunger -= Time.deltaTime;
                return;
            }

            timeLeftHunger = timeToDecreaseHunger;
            hunger += 1;
        }

        public void UpdateEnergy(bool shouldNotUpdateEnergy)
        {
            if (shouldNotUpdateEnergy)
            {
                return;
            }

            if (timeLeftEnergy > 0)
            {
                timeLeftEnergy -= Time.deltaTime;
                return;
            }

            timeLeftEnergy = timeToDecreaseEnergy;
            energy -= 1;
        }

        void UpdateDisplayText()
        {
           billboard.UpdateStatsText(energy, hunger, money);
        }
    }
}