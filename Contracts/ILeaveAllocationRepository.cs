using HRManagement.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRManagement.Contracts
{
   public interface ILeaveAllocationRepository : IRepositoryBase<LeaveAllocation>
    {
        bool CheckAllocation(int leavetypeid, string empliyeeid);
        ICollection<LeaveAllocation> GetLeaveAllocationsByEmployee(string id);
       LeaveAllocation GetLeaveAllocationsByEmployeeAndType(string id, int typeId);
    }
}
