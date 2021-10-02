using System;
using System.Collections.Generic;

namespace KaizerWaldCode
{
    public interface IFiniteStateMachine<T> where T : Enum
    {
        public List<T> States { get; set; }
        public void InitializeStateMachine();
        public void StateMachineStart();
        public void ChangeState(T current);
    }
}
