using UnityEngine;
using static Assets.David.Scripts.DesignPatterns.StateMachine.ExperimentStateManager;
using System;
using Game.ServerAuthoritativeSynchronousSpawning;

namespace Assets.David.Scripts.DesignPatterns.StateMachine
{
    public class ExperimentStateManager : StateManager<ExperimentState>
    {
        [SerializeField] private ExperimentStateManager experimentStateManager;
        [SerializeField] private XRServerAuthoritativeSynchronousSpawning serverSpawnManager;
        [SerializeField] private GameObject DataObjects;
        
        [SerializeField] private GameObject titleText;
        [SerializeField] private GameObject Task1;
        [SerializeField] private GameObject Task2;
        [SerializeField] private GameObject Task3;

        public enum ExperimentState
        {
            Task1,
            Task2,
            Task3,
        }
        private ExperimentContext context;

        // Use this for initialization
        void Start()
        {
            ValidateConstraints();
            context = new ExperimentContext(titleText, Task1, Task2, Task3, experimentStateManager, serverSpawnManager, DataObjects);

            InitializeStates();
        }

        // Update is called once per frame
        void ValidateConstraints()
        {
/*            Assert.IsNotNull(DesktopState, "_mainMenu is null at start");
            Assert.IsNotNull(ARState, "_loginMenu is null at start");
            Assert.IsNotNull(VRState, "_drawerMenu is null at start");
            Assert.IsNotNull(MRState, "_navbarMenu is null at start");
            Assert.IsNotNull(CombiState, "_navbarMenu is null at start");*/
        }

        void InitializeStates()
        {
            states.Add(ExperimentState.Task1, new Task1(context, ExperimentState.Task1));
            states.Add(ExperimentState.Task2, new Task2(context, ExperimentState.Task2));
            states.Add(ExperimentState.Task3, new Task3(context, ExperimentState.Task3));
        }

        public void SwitchToState(string state)
        {
            ExperimentState newState;
            if (Enum.TryParse(state, out newState))
            {
                if (currentState != null)
                    currentState.ExitState();
                currentState = states[newState];
                currentState.InitState();
                currentState.EnterState();
                //Switct State on client as well
            }
            else
            {
                Debug.LogError("Invalid state: " + state);
            }
        }




    }
}