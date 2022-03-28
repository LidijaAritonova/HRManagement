using HRManagement.Contracts;
using HRManagement.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRManagement.Repository
{
    public class LeaveAllocationRepository : ILeaveAllocationRepository
    {
        private readonly ApplicationDbContext _db;

        public LeaveAllocationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool CheckAllocation(int leavetypeid, string empliyeeid)
        {
            var period = DateTime.Now.Year;
            return FindAll()
                .Where(q => q.EmployeeId == empliyeeid && q.LeaveTypeId == leavetypeid && q.Period == period)
                .Any();
        }

        public bool Create(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Add(entity);
            return Save();

        }

        public bool Delete(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Remove(entity);
            return Save();
        }

        public ICollection<LeaveAllocation> FindAll()
        {
           var LeaveAllocations= _db.LeaveAllocations
                                .Include(q => q.LeaveType)
                                .Include(q => q.Employee)
                                .ToList();
            return (LeaveAllocations);
        }

        public LeaveAllocation FindById(int id)
        {
          var LeaveAllocation= _db.LeaveAllocations
                .Include(q => q.LeaveType)
                .Include(q => q.Employee)
                .FirstOrDefault(q => q.Id == id);
            return (LeaveAllocation);
        }

        public ICollection<LeaveAllocation> GetLeaveAllocationsByEmployee(string id)
        {
            var period = DateTime.Now.Year;
             return FindAll().Where(q => q.EmployeeId == id && q.Period == period).ToList();
            

        }

       

        public async Task<bool> isExists(int id)
        {
            var exists = await _db.LeaveAllocations.AnyAsync(q => q.Id == id);
            return exists;
        }

        public bool Save()
        {
            return _db.SaveChanges() > 0;
        }

        public bool Update(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Update(entity);
            return Save();

        }

        bool IRepositoryBase<LeaveAllocation>.isExists(int id)
        {
            throw new NotImplementedException();
        }

        LeaveAllocation ILeaveAllocationRepository.GetLeaveAllocationsByEmployeeAndType(string id, int typeId)
        {
            var period = DateTime.Now.Year;
            return FindAll().FirstOrDefault(q => q.EmployeeId == id && q.Period == period && q.LeaveTypeId == typeId);
        }
    }
}
