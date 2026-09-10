using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models
{
    [Table("cliente")]
    public class Cliente
    {
        [Key]
        [Column("idcliente")]
        public int IdCliente { get; set; }

        [Column("ruc")]
        [StringLength(14)]
        public string? Ruc { get; set; }

        [Required]
        [Column("nombre")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Column("apellidos")]
        [StringLength(60)]
        public string? Apellidos { get; set; }

        [Required]
        [Column("idtipodoc")]
        [StringLength(2)]
        public string IdTipoDoc { get; set; } = string.Empty;

        [Required]
        [Column("nrodocumento")]
        [StringLength(22)]
        public string NroDocumento { get; set; } = string.Empty;

        [Column("fechanacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [Column("sexo")]
        [StringLength(1)]
        public string? Sexo { get; set; }

        [Column("idrubro")]
        [StringLength(6)]
        public string? IdRubro { get; set; }

        [Column("telefono")]
        [StringLength(10)]
        public string? Telefono { get; set; }

        [Column("email")]
        [StringLength(50)]
        public string? Email { get; set; }

        [Column("idtipocalle")]
        public int? IdTipoCalle { get; set; }

        [Column("direccion")]
        [StringLength(100)]
        public string? Direccion { get; set; }

        [Column("urbanizacion")]
        [StringLength(100)]
        public string? Urbanizacion { get; set; }

        [Column("codpais")]
        [StringLength(6)]
        public string? CodPais { get; set; }

        [Column("ubigeo")]
        [StringLength(6)]
        public string? Ubigeo { get; set; }

        [Column("codpto")]
        [StringLength(2)]
        public string? CodPto { get; set; }

        [Column("codprovincia")]
        [StringLength(2)]
        public string? CodProvincia { get; set; }

        [Column("coddistrito")]
        [StringLength(2)]
        public string? CodDistrito { get; set; }

        [Column("ciudad")]
        [StringLength(50)]
        public string? Ciudad { get; set; }

        [Column("direccionentrega")]
        [StringLength(100)]
        public string? DireccionEntrega { get; set; }

        [Column("mensaje")]
        public string? Mensaje { get; set; }

        [Column("sector")]
        [StringLength(100)]
        public string? Sector { get; set; }

        [Column("rucemisor")]
        [StringLength(14)]
        public string? RucEmisor { get; set; }

        [Column("razonsocial")]
        [StringLength(150)]
        public string? RazonSocial { get; set; }

        [Column("nomcomercial")]
        [StringLength(150)]
        public string? NomComercial { get; set; }

        [Column("motivo")]
        [StringLength(150)]
        public string? Motivo { get; set; }
    }
}