using TMPro;
 
namespace Assets.David.Scripts.DesignPatterns.StateMachine
{
    public abstract class ExperinmentState : BaseState<ExperimentStateManager.ExperimentState>
    {
        protected ExperimentContext context;   

        public ExperinmentState(ExperimentContext context, ExperimentStateManager.ExperimentState experimentState) : base(experimentState) 
        {
            this.context = context;
        }

        public void ChangeTitleText(string text)
        {
            context.GetTitleText.GetComponent<TMP_Text>().text = text;
        }
    }
}