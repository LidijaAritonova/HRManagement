using HRManagement.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRManagement.Contracts
{
    public interface ILeaveRequestRepository : IRepositoryBase<LeaveRequest>
    {
       public ICollection<LeaveRequest> GetLeaveRequestsByEmployee(string employeeid);
    }
}
