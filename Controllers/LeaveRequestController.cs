using AutoMapper;
using HRManagement.Contracts;
using HRManagement.Data;
using HRManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRManagement.Controllers
{
    [Authorize]
    public class LeaveRequestController : Controller
    {
        private readonly ILeaveRequestRepository _leaveaRequestRepo;
        private readonly ILeaveTypeRepository _leaveaTypeRepo;
        private readonly ILeaveAllocationRepository _leaveAllocationRepo;
        private readonly UserManager<Employee> _userManager;
        private readonly IMapper _mapper;

        public LeaveRequestController(ILeaveRequestRepository leaveaRequestRepo, ILeaveAllocationRepository leaveAllocationRepo, ILeaveTypeRepository leaveaTypeRepo, UserManager<Employee> userManager, IMapper mapper)
        {
            _leaveaRequestRepo = leaveaRequestRepo;
            _leaveAllocationRepo = leaveAllocationRepo;
            _leaveaTypeRepo = leaveaTypeRepo;
            _userManager = userManager;
            _mapper = mapper;
        }
        [Authorize (Roles = "Administrator")]
        // GET: LeaveRequestController
        public ActionResult Index()
        {
            var leaveRequests = _leaveaRequestRepo.FindAll();
            var leaveRequstsModel = _mapper.Map<List<LeaveRequestVM>>(leaveRequests);
            var model = new AdminLeaveRequestViewVM
            {
                TotalRequests = leaveRequstsModel.Count,
                ApprovedRequests = leaveRequstsModel.Count(q => q.Approved == true),
                PendingRequests = leaveRequstsModel.Count(q => q.Approved == null),
                RejectedRequests = leaveRequstsModel.Count(q => q.Approved == false),
                LeaveRequests = leaveRequstsModel
            };
            return View(model);
        }

        public ActionResult MyLeave()
        {
            var employee = _userManager.GetUserAsync(User).Result;
       

            var employeeAllocations = _leaveAllocationRepo.GetLeaveAllocationsByEmployee(employee.Id);

            var employeeRequests = _leaveaRequestRepo.GetLeaveRequestsByEmployee(employee.Id);

            var employeeAllocationsModel = _mapper.Map<List<LeaveAllocationVM>>(employeeAllocations);
            var employeeRequestsModel = _mapper.Map<List<LeaveRequestVM>>(employeeRequests);

            var model = new EmployeeLeaveRequestViewVM
            {
                LeaveAllocations = employeeAllocationsModel,
                LeaveRequests = employeeRequestsModel
            };

            return View(model);

        }


        // GET: LeaveRequestController/Details/5
        public ActionResult Details(int id)
        {
            var leaveRequest = _leaveaRequestRepo.FindById(id);
            var model = _mapper.Map<LeaveRequestVM>(leaveRequest);
            return View(model);
        }

        public ActionResult ApproveRequest(int id)
        {
           try
            {
                var user = _userManager.GetUserAsync(User).Result;
                var leaveRequest = _leaveaRequestRepo.FindById(id);
                var allocation = _leaveAllocationRepo.GetLeaveAllocationsByEmployeeAndType(leaveRequest.RequestingEmployeeId, leaveRequest.LeaveTypeId);
                int daysRequested = (int)(leaveRequest.EndDate - leaveRequest.EndDate).TotalDays;
                allocation.NumberOfDays -= daysRequested;
                
                
                leaveRequest.Approved = true;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;

                 _leaveaRequestRepo.Update(leaveRequest);
                _leaveAllocationRepo.Update(allocation);

                return RedirectToAction(nameof(Index));
            }

            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index), "Home");
            }
        }


        public ActionResult RejectRequest(int id)
        {
            try
            {
                var user2 = _userManager.GetUserAsync(User).Result;
                var leaveRequest2 = _leaveaRequestRepo.FindById(id);
                leaveRequest2.Approved = false;
                leaveRequest2.ApprovedById = user2.Id;
                leaveRequest2.DateActioned = DateTime.Now;

                 _leaveaRequestRepo.Update(leaveRequest2);
                return RedirectToAction(nameof(Index));
            }

            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
            var user = _userManager.GetUserAsync(User).Result;
            var leaveRequest = _leaveaRequestRepo.FindById(id);
            return View();
        }

        public ActionResult CancelRequest(int id)
        {
            var leaveRequest = _leaveaRequestRepo.FindById(id);
            leaveRequest.Cancelled = true;
            _leaveaRequestRepo.Update(leaveRequest);
            _leaveaRequestRepo.Save();

            // send email to user 
            return RedirectToAction("MyLeave");
        }

        // GET: LeaveRequestController/Create
        public ActionResult Create()
        {
            var leaveTypes = _leaveaTypeRepo.FindAll();
            var leaveTypesItem = leaveTypes.Select(q => new SelectListItem { Text = q.Name, Value=q.Id.ToString()});
            var model = new CreateLeaveRequestVM
            {
                    LeaveTypes= leaveTypesItem
                   
            };
            return View(model);
        }

        // POST: LeaveRequestController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateLeaveRequestVM model)
        {
            try
            {
                var startDate = Convert.ToDateTime(model.StartDate);
                var endDate = Convert.ToDateTime(model.EndDate);
                var leaveTypes = _leaveaTypeRepo.FindAll();
                var leaveTypesItem = leaveTypes.Select(q => new SelectListItem { Text = q.Name, Value = q.Id.ToString() });
                model.LeaveTypes = leaveTypesItem;
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if(DateTime.Compare(startDate, endDate)>1)
                {
                    ModelState.AddModelError("", "The StartDate can't be further in the future than the EndDate");
                    return View(model);
                }

                var employee = _userManager.GetUserAsync(User).Result;
                var allocation = _leaveAllocationRepo.GetLeaveAllocationsByEmployeeAndType(employee.Id, model.LeaveTypeId);
                int daysRequested = (int)(endDate - startDate).TotalDays;
                 if (daysRequested > allocation.NumberOfDays)
                {
                    ModelState.AddModelError("", "You don't hane enought days for this request. Please try again.");
                    return View(model);
                }
                var leaveRequestModel = new LeaveRequestVM
                {
                    RequestingEmployeeId = employee.Id,
                    StartDate = startDate,
                    EndDate =endDate,
                    Approved = null,
                    DateRequested = DateTime.Now,
                    DateActioned = DateTime.Now,
                    LeaveTypeId = model.LeaveTypeId
                };

                var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestModel);
                var isSuccess = _leaveaRequestRepo.Create(leaveRequest);
                if (! isSuccess)
                {
                    ModelState.AddModelError("", "Error-something went wrong. Try again.");
                    return View(model);
                }



                    return RedirectToAction("MyLeave");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error-something went wrong");
                return View(model);
            }
        }

        // GET: LeaveRequestController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveRequestController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
