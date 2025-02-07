using System.Collections;
using UnityEngine;
using Assets.David.UILayout;
using XRSpatiotemopralAuthoring;
using Game.ServerAuthoritativeSynchronousSpawning;

namespace Assets.David.Scripts.DesignPatterns.StateMachine
{
    public class ExperimentContext
    {
        public enum AppContext
        {
            idle,
            switchNext,
            switchPrevious
        }

        private GameObject _titleText;
        private GameObject _Task1;
        private GameObject _Task2;
        private GameObject _Task3;
        public AppContext appContext;
        public GameObject DataObjects;

        private XRServerAuthoritativeSynchronousSpawning xRServerAuthoritativeSynchronousSpawning;

        private ExperimentStateManager experimentStateManager;


        public GameObject GetTitleText { get => _titleText; }
        public GameObject GetTask1GO { get => _Task1; }    
        public GameObject GetTask2GO { get => _Task2; }
        public GameObject GetTask3GO { get => _Task3; }
        public GameObject GetDATAGO { get => DataObjects; }
        public ExperimentStateManager ExperimentStateManager { get => experimentStateManager; }
        public XRServerAuthoritativeSynchronousSpawning XRServerAuthoritativeSynchronousSpawning { get => xRServerAuthoritativeSynchronousSpawning; }



        // Use this for initialization
       public ExperimentContext(GameObject titleText, GameObject Task1, GameObject Task2, GameObject Task3,ExperimentStateManager experimentStateManager, XRServerAuthoritativeSynchronousSpawning serverSpawnManager, GameObject dataObjects)
       {
            this._titleText = titleText;
            this._Task1 = Task1;
            this._Task2 = Task2;
            this._Task3 = Task3;
            this.appContext = AppContext.idle;
            this.experimentStateManager = experimentStateManager;
            this.xRServerAuthoritativeSynchronousSpawning = serverSpawnManager;
            this.DataObjects = dataObjects;
       }

        public void SetGraph(string task)
        {
            xRServerAuthoritativeSynchronousSpawning.SetTaskGraphOnClients(task);
        }

    }
}