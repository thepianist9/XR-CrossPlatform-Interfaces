using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets.David.Scripts.DesignPatterns.StateMachine
{
    public class Task2 : ExperinmentState
    {
        private GameObject Task2GO;
        private bool isTaskRunning = false;
        private float elapsedTime = 0f;

        private GameObject startButton;
        private GameObject stopButton;
        private GameObject successButton;
        private GameObject resetButton;
        private TMP_Text timerDisplay;

        public Task2(ExperimentContext context, ExperimentStateManager.ExperimentState estate) : base(context, estate)    
        {
             ExperimentContext Context = context;
        }
        public override void InitState()
        {
            Task2GO = context.GetTask2GO;

            // Get references to buttons and timer display
            startButton = Task2GO.transform.Find("StartButton").gameObject;
            stopButton = Task2GO.transform.Find("StopButton").gameObject;
            successButton = Task2GO.transform.Find("DisplaySuccessButton").gameObject;
            resetButton = Task2GO.transform.Find("ResetButton").gameObject;

            timerDisplay = Task2GO.transform.Find("TimerDisplay").GetComponent<TMP_Text>();

            // Set up button listeners
            startButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(StartTask);
            stopButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(StopTask);
            resetButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ResetTask);
            successButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(DisplaySuccess);
        }

        public override void EnterState()
        {
            //set layout via UILayoutManager
            Task2GO.transform.GetChild(0).gameObject.SetActive(true);
            context.SetGraph("task 2");
            context.appContext = ExperimentContext.AppContext.idle;
            StartTask();
        }

        public override void ExitState()
        {
            Task2GO.transform.GetChild(0).gameObject.SetActive(false);
        }

        public override ExperimentStateManager.ExperimentState GetNextState()
        {

            return stateKey;
        }

       

        public override void UpdateState()
        {
            if (isTaskRunning)
            {
                // Increment elapsed time
                elapsedTime += Time.deltaTime;

                // Update the timer display
                timerDisplay.text = $"{elapsedTime:F2}s";
            }
        }

        // Start the task
        private void StartTask()
        {
            if (!isTaskRunning)
            {
                isTaskRunning = true;
                elapsedTime = 0f;
                Debug.Log("Task 2 started.");
            }
        }

        // Stop the task
        private void StopTask()
        {
            if (isTaskRunning)
            {
                isTaskRunning = false;
                Debug.Log($"Task 2 stopped. Total time: {elapsedTime:F2} seconds.");
            }
        }


        // Reset the task
        private void ResetTask()
        {
            isTaskRunning = false;
            elapsedTime = 0f;
            timerDisplay.text = "00:00";
        }

        // Display success feedback
        private void DisplaySuccess()
        {
            if (!isTaskRunning)
            {
                Debug.Log("Task 2 completed successfully.");
                // Display a success message (this could be a UI popup, sound, etc.)
                context.XRServerAuthoritativeSynchronousSpawning.DisplayTaskCompletedClientRPC("Task 2 successfully completed!!");
            }
            else
            {
                Debug.LogWarning("Stop the task before displaying success.");
            }
        }
    }
}