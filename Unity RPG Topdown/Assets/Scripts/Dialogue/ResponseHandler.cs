using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DialogueSystem.Data;

namespace UIWindows
{
    public class ResponseHandler : MonoBehaviour
    {
        public static ResponseHandler Instance { get; private set; }

        [SerializeField] private GameObject responseBox;
        [SerializeField] private GameObject optionTemplate;
        [SerializeField] private Transform responseParent;

        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private Transform dialogueBorder;

        [SerializeField] private RectTransform responsePivot;
        [SerializeField] private RectTransform responseArea;

        [SerializeField] private int responseAreaSeperation;

        List<GameObject> tempResponseButtons = new();

        [SerializeField] private Scrollbar scrollbar;

        public event Action<DialogueSystemDialogueChoiceData> OnPickedResponse;

        public UnityEvent responsesOpen, responsesClosed;

        private bool responsesActive;

        GameObject selectedResponse = null;
        int selectedResponseIndex = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("There are multiple ResponseHandlers in the scene!");
                Destroy(this);
            }
        }

        public void DisableResponses()
        {
            responsesClosed.Invoke();
            responsesActive = false;
            responseBox.SetActive(false);

            selectedResponseIndex = 0;

            foreach (GameObject responseButton in tempResponseButtons)
            {
                Destroy(responseButton);
            }

            tempResponseButtons.Clear();
        }

        public void EnterPointer(Button button)
        {
            if (tempResponseButtons != null && tempResponseButtons.Contains(button.gameObject))
            {
                tempResponseButtons[selectedResponseIndex].transform.GetChild(0).gameObject.SetActive(false);
                selectedResponse = button.gameObject;
                selectedResponseIndex = tempResponseButtons.IndexOf(button.gameObject);
                tempResponseButtons[selectedResponseIndex].transform.GetChild(0).gameObject.SetActive(true);
                UpdateResponseColors();
            }
        }

        private void UpdateResponseColors()
        {
            for (int i = 0; i < tempResponseButtons.Count; i++)
            {
                if (i == selectedResponseIndex)
                {
                    tempResponseButtons[i].GetComponent<TMPro.TextMeshProUGUI>().color = Color.yellow;
                }
                else
                {
                    tempResponseButtons[i].GetComponent<TMPro.TextMeshProUGUI>().color = Color.white;
                }
            }
        }

        public void ShowResponses(List<DialogueSystemDialogueChoiceData> choices)
        {
            // Calculate the position for the responsePivot

            RectTransform dialogueBorderRectTransform = dialogueBorder.GetComponent<RectTransform>();

            float dialogueBorderRemainder = (dialogueBorder.GetComponent<RectTransform>().sizeDelta.y - 100);

            dialogueBorderRectTransform.sizeDelta = new Vector2(dialogueBorderRectTransform.sizeDelta.x, dialogueBorderRectTransform.sizeDelta.y + responseArea.sizeDelta.y + responseAreaSeperation);

            // Set the responsePivot's position to align with the bottom of the dialogueBorder

            foreach (var choice in choices)
            {
                GameObject newResponse = Instantiate(optionTemplate, responseParent);
                tempResponseButtons.Add(newResponse);
                newResponse.SetActive(true);
                newResponse.GetComponentInChildren<TextMeshProUGUI>().text = choice.Text;
                newResponse.GetComponent<Button>().onClick.AddListener(() =>
                {
                    OnPickedResponse?.Invoke(choice);
                    DisableResponses();
                });

            }
            responsesOpen.Invoke();
            responseBox.SetActive(true);
            responsesActive = true;

            tempResponseButtons[selectedResponseIndex].GetComponent<TMPro.TextMeshProUGUI>().color = Color.yellow;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                SelectPreviousResponse();
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                SelectNextResponse();
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.F))
            {
                PickResonse();
            }

            // Detect mouse scroll
            if (responsesActive && Input.mouseScrollDelta.y != 0)
            {
                if (Input.mouseScrollDelta.y > 0)
                {
                    SelectPreviousResponse();

                }
                else
                {
                    SelectNextResponse();

                }
            }
        }

        private void PickResonse()
        {
            if (tempResponseButtons.Count > 0)
                tempResponseButtons[selectedResponseIndex].GetComponent<Button>().onClick.Invoke();
        }

        private void SelectPreviousResponse()
        {
            if (selectedResponseIndex > 0)
            {
                tempResponseButtons[selectedResponseIndex].GetComponent<TextMeshProUGUI>().color = Color.white;
                tempResponseButtons[selectedResponseIndex].transform.GetChild(0).gameObject.SetActive(false);
                selectedResponseIndex--;
                tempResponseButtons[selectedResponseIndex].GetComponent<TextMeshProUGUI>().color = Color.yellow;
                tempResponseButtons[selectedResponseIndex].transform.GetChild(0).gameObject.SetActive(true);

                // Calculate the normalized scroll position based on the selected response.
                float normalizedScrollPosition = (float)selectedResponseIndex / (tempResponseButtons.Count - 1);

                // Set the scrollbar value based on the normalized position.
                scrollbar.value = 1.0f - normalizedScrollPosition;
            }
        }

        private void SelectNextResponse()
        {
            if (selectedResponseIndex < tempResponseButtons.Count - 1)
            {
                tempResponseButtons[selectedResponseIndex].GetComponent<TextMeshProUGUI>().color = Color.white;
                tempResponseButtons[selectedResponseIndex].transform.GetChild(0).gameObject.SetActive(false);
                selectedResponseIndex++;
                tempResponseButtons[selectedResponseIndex].GetComponent<TextMeshProUGUI>().color = Color.yellow;
                tempResponseButtons[selectedResponseIndex].transform.GetChild(0).gameObject.SetActive(true);


                // Calculate the normalized scroll position based on the selected response.
                float normalizedScrollPosition = (float)selectedResponseIndex / (tempResponseButtons.Count - 1);

                // Set the scrollbar value based on the normalized position.
                scrollbar.value = 1.0f - normalizedScrollPosition;
            }
        }


        //private void OnPickedResponse(DialogueSystemDialogueChoiceData choice)
        //{
        //    DisableResponses();

        //    dialogueController.ShowDialogue(choice.NextDialogue);
        //}

    }
}