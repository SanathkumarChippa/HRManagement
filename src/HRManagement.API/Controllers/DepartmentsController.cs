// File Path: src/HRManagement.API/Controllers/DepartmentsController.cs
// Purpose: API controller managing Department endpoints.
// Code Explanation: Provides standard CRUD operations to manage departments, using IUnitOfWork and AutoMapper.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Application.DTOs.Department;
using HRManagement.Domain.Entities;

namespace HRManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/departments")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DepartmentsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departments = await _unitOfWork.Departments.GetAllAsync();
            var dtos = _mapper.Map<List<DepartmentDto>>(departments);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<DepartmentDto>(department));
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
        {
            var department = _mapper.Map<Department>(dto);
            department.CreatedDate = DateTime.UtcNow;

            var result = await _unitOfWork.Departments.AddAsync(department);
            await _unitOfWork.SaveAsync();

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, _mapper.Map<DepartmentDto>(result));
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateDepartmentDto dto)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            _mapper.Map(dto, department);
            department.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Departments.UpdateAsync(department);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            await _unitOfWork.Departments.DeleteAsync(department);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }
    }
}
