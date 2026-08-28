using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HRSystem.Application.DTO;

public class HireEmployeeRequest
{
    [Required(ErrorMessage = "Фамилия обязательна")]
    [MaxLength(100)]
    public string Surname { get; set; } = null!;

    [Required(ErrorMessage = "Имя обязательно")]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(100)]
    public string? Patronim { get; set; }

    [Required(ErrorMessage = "Дата приёма обязательна")]
    public DateTime HireDate { get; set; }

    [Required(ErrorMessage = "Не указано подразделение")]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "Не указана должность")]
    public int PositionId { get; set; }
}

public class DismissEmployeeRequest
{
    [Required(ErrorMessage = "Дата увольнения обязательна")]
    public DateTime DismissalDate { get; set; }
}

public class TransferDepartmentRequest
{
    [Required(ErrorMessage = "Не указано новое подразделение")]
    public int NewDepartmentId { get; set; }

    [Required(ErrorMessage = "Дата перевода обязательна")]
    public DateTime EffectiveDate { get; set; }
}

public class TransferPositionRequest
{
    [Required(ErrorMessage = "Не указана новая должность")]
    public int NewPositionId { get; set; }

    [Required(ErrorMessage = "Дата перевода обязательна")]
    public DateTime EffectiveDate { get; set; }
}

public class EmployeeResponse
{
    public int Id { get; set; }
    public string LastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string FullName { get; set; } = null!;
    public DateTime HireDate { get; set; }
    public DateTime? DismissalDate { get; set; }
    public bool IsActive { get; set; }
    public int? CurrentDepartmentId { get; set; }
    public string? CurrentDepartmentName { get; set; }
    public int? CurrentPositionId { get; set; }
    public string? CurrentPositionTitle { get; set; }
}