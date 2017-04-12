using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombinedSolution.Classes
{
    public enum SchedulerState
    {
        NotStarted,
        Running,
        Closing,
        Aborted
    }
    public enum TaskState
    {
        ToBeExecuted,
        Executing,
        Complete,
        Aborted
    }
    public enum SchedulingMechanism
    {
        WorkSharing,
        WorkStealing
    }
}
