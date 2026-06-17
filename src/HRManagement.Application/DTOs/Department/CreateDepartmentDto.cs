// File Path: src/HRManagement.Application/DTOs/Department/CreateDepartmentDto.cs
// Purpose: DTO representing creation payloads for departments.
// Code Explanation: Holds the required Name property for new department creation.

namespace HRManagement.Application.DTOs.Department
{
    public class CreateDepartmentDto
    {
        public string Name { get; set; } = string.Empty;
    }
}
