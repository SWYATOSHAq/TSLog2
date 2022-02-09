using TSLog.Staff.BO;

namespace TSLog.Yard.WebUI.Models
{
    public class WorkersModel
    {
        public IDictionary<Position, IList<Employee>> AllocatedWorkers { get; private set; }
        public IDictionary<Position, short> PlanWorkersCounts { get; private set; }
        public IDictionary<Position, int> LaborCost { get; private set; }
        public bool IsEnabled { get; private set; }
        public WorkersModel(IDictionary<Position, IList<Employee>> allocatedWorkers,
            IDictionary<Position, short> planWorkersCounts,
            IDictionary<Position, int> laborCost,
            bool isEnabled)
        {
            AllocatedWorkers = allocatedWorkers;
            PlanWorkersCounts = planWorkersCounts;
            LaborCost = laborCost;
            IsEnabled = isEnabled;
        }
    }
}
