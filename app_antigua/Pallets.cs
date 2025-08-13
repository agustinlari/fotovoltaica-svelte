using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System;
using UnityEngine;

[Table("Pallets")]
public class Pallets : BaseModel
{
	[PrimaryKey("id")]
	[Column("id")]
	public string id { get; set; }

	[Column("Descarga")]
	public int Descarga { get; set; }

	[Column("Defecto")]
	public bool Defecto { get; set; }

    [Column("updated_at")]
    public string updated_at { get; set; }

}

