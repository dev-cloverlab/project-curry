using System;

namespace curry.Common
{
    public class StateMachineProcess
    {
        public StateMachineProcess(Action<StateMachineProcess> action)
        {
            m_ProcessAction = action;
        }
        private Action<StateMachineProcess> m_ProcessAction;
        private int m_NextState = StateMachine.State.None;
        private enum Status
        {
            None,
            Running,
            Idling,
            Complete
        }

        private Status m_Status = Status.None;

        public bool IsCompleteProcess()
        {
            return m_Status == Status.Complete;
        }

        public void Run()
        {
            if (m_Status == Status.Running) {
                return;
            }
            m_Status = Status.Running;
            m_ProcessAction?.Invoke(this);
        }

        public void IdlingProcess()
        {
            m_Status = Status.Idling;
        }

        public void RestartProcess()
        {
            m_Status = Status.None;
            this.Run();
        }

        public void CompleteProcess()
        {
            m_Status = Status.Complete;
        }

        public void SetNextState(int nextState)
        {
            m_NextState = nextState;
        }

        public int GetNextState()
        {
            return m_NextState;
        }

        public void InitProcess()
        {
            m_Status = Status.None;
        }

    }
}