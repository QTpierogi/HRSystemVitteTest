using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HRSystem.Application.DTO;

public class CreateDepartmentRequest
{
    [Required(ErrorMessage = "Название подразделения обязательно")]
    [MaxLength(150)]
    public string Name { get; set; } = null!;
    public int? ParentDepartmentId { get; set; }
    public DateTime? EffectiveDate { get; set; }
}

public class MoveDepartmentRequest
{
    public int? NewParentDepartmentId { get; set; }

    [Required(ErrorMessage = "Дата изменения структуры обязательна")]
    public DateTime EffectiveDate { get; set; }
}

public class RenameDepartmentRequest
{
    [Required(ErrorMessage = "Название подразделения обязательно")]
    [MaxLength(150)]
    public string Name { get; set; } = null!;
}

public class DismissDepartmentRequest
{
    [Required(ErrorMessage = "Дата роспуска обязательна")]
    public DateTime EffectiveDate { get; set; }
}

public class DepartmentResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int? ParentDepartmentId { get; set; }
    public bool IsDissolved { get; set; }
    public DateTime? DissolvedDate { get; set; }
}

public class DepartmentTreeNode
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<DepartmentTreeNode> Children { get; set; }
    public List<EmployeeResponse> Employees { get; set; }
}