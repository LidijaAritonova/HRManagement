using AutoMapper;
using HRManagement.Data;
using HRManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRManagement.Mappings
{
    public class Maps: Profile
    {

      public Maps()
        {
            CreateMap<LeaveType, LeaveTypeVM>().ReverseMap();
            CreateMap<LeaveRequest, LeaveRequestVM>().ReverseMap();
            CreateMap<LeaveAllocation, LeaveAllocationVM>().ReverseMap();
            CreateMap<LeaveAllocation, ViewAllocationsVM>().ReverseMap();
            CreateMap<LeaveAllocation, CreateLeaveAllocationVM>().ReverseMap();
            CreateMap<LeaveAllocation, EditLeaveAllocationVM>().ReverseMap();
            CreateMap<Employee,EmployeeVM>().ReverseMap();

        }
         
    }
}
