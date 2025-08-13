using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System;
using UnityEngine;

[Table("Camiones")]
public class Camiones : BaseModel
{
	[PrimaryKey("id")]
	[Column("id")]
	public int id { get; set; }

	[Column("created_at")]
	public DateTime CreatedAt { get; set; }

	[Column("DNI")]
	public string DNI { get; set; }

	[Column("NombreConductor")]
	public string NombreConductor { get; set; }

	[Column("Matricula")]
	public string Matricula { get; set; }

	[Column("UbicacionCampa")]
	public string UbicacionCampa { get; set; }

	[Column("FechaDescarga")]
	public string FechaDescarga { get; set; }

	[Column("Container")]
	public string Container { get; set; }

	[Column("Albaran")]
	public string Albaran { get; set; }

    [Column("updated_at")]
    public string updated_at { get; set; }
}

