using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System;
using UnityEngine;

[Table("Estructura")]
public class Estructura : BaseModel
{
    [PrimaryKey("id")]
    [Column("id")]
    public int id { get; set; }

    [Column("DNI")]
    public string DNI { get; set; }

    [Column("Conductor")]
    public string Conductor { get; set; }

    [Column("Matricula")]
    public string Matricula { get; set; }

    [Column("Proveedor")]
    public string Proveedor { get; set; }

    [Column("FechaDescarga")]
    public string FechaDescarga { get; set; }

    [Column("PackingList")]
    public string PackingList { get; set; }

    [Column("Albaran")]
    public string Albaran { get; set; }

}

