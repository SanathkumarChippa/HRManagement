// File Path: src/HRManagement.Application/DTOs/Department/DepartmentDto.cs
// Purpose: DTO representing department details.
// Code Explanation: Exposes the primary key Id and the Department Name.

namespace HRManagement.Application.DTOs.Department
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
