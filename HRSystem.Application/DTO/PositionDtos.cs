using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HRSystem.Application.DTO;

public class CreatePositionRequest
{
    [Required(ErrorMessage = "Название должности обязательно")]
    [MaxLength(150)]
    public string Title { get; set; } = null!;
}

public class RenamePositionRequest
{
    [Required(ErrorMessage = "Название должности обязательно")]
    [MaxLength(150)]
    public string Title { get; set; } = null!;
}

public class PositionResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
}
