using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

public partial class SchemaVersion
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string ScriptName { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime Applied { get; set; }
}
