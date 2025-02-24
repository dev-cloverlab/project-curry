using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace curry.Common
{
    public class StateMachine
    {
        public class State
        {
            public const int None = -1;
            public const int Init = 0;
        }

        protected int m_CurrentState = -1;
        protected Dictionary<int, StateMachineProcess> m_ProcessList = new Dictionary<int, StateMachineProcess>();

        public void AddState(int state, Action<StateMachineProcess> processAction)
        {
            m_ProcessList.Add(state, new StateMachineProcess(processAction));
        }

        public void NextState()
        {
            if (!m_ProcessList.ContainsKey(m_CurrentState)) {
                throw new Exception(string.Format("no state. state:{0}", m_CurrentState));
            }
            int nextState = m_ProcessList[m_CurrentState].GetNextState();
            this.NextState(nextState);
        }

        public void NextState(int nextState)
        {
            if (!m_ProcessList.ContainsKey(nextState)) {
                throw new Exception(string.Format("no state. state:{0}", nextState));
            }
            m_CurrentState = nextState;
            m_ProcessList[m_CurrentState].InitProcess();
            m_ProcessList[m_CurrentState].Run();
        }

        public int GetCurrentState()
        {
            return m_CurrentState;
        }

        public bool IsState(int state)
        {
            return GetCurrentState() == state;
        }

        public bool IsComplete()
        {
            if (!m_ProcessList.ContainsKey(m_CurrentState)) {
                return false;
            }
            return m_ProcessList[m_CurrentState].IsCompleteProcess();
        }
    }
}
