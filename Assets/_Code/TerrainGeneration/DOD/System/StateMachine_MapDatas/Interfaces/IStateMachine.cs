using System.Collections.Generic;

namespace KaizerWaldCode
{
    public interface IStateMachine<T>
    {
        List<T> States { get; set; }
        void InitializeStateMachine();
    }
}
