using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models
{
    [Table("usuario_cliente")]
    public class UsuarioCliente : IdentityUser<int>
    {
        // ============================================================
        // 📌 PROPIEDADES DE IDENTITY
        // ============================================================

        [Column("id")]
        public override int Id { get; set; }

        [Column("user_name")]
        [MaxLength(256)]
        public override string? UserName { get; set; }

        [Column("email")]
        [MaxLength(256)]
        public override string? Email { get; set; }

        [Column("normalized_email")]
        [MaxLength(256)]
        public override string? NormalizedEmail { get; set; }

        [Column("normalized_user_name")]
        [MaxLength(256)]
        public override string? NormalizedUserName { get; set; }

        [Column("password_hash")]
        public override string? PasswordHash { get; set; }

        [Column("security_stamp")]
        public override string? SecurityStamp { get; set; }

        [Column("concurrency_stamp")]
        public override string? ConcurrencyStamp { get; set; }

        [Column("phone_number")]
        [MaxLength(20)]
        public override string? PhoneNumber { get; set; }

        [Column("phone_number_confirmed")]
        public override bool PhoneNumberConfirmed { get; set; }

        [Column("email_confirmed")]
        public override bool EmailConfirmed { get; set; }

        [Column("lockout_enabled")]
        public override bool LockoutEnabled { get; set; }

        [Column("lockout_end")]
        public override DateTimeOffset? LockoutEnd { get; set; }

        [Column("access_failed_count")]
        public override int AccessFailedCount { get; set; }

        [Column("two_factor_enabled")]
        public override bool TwoFactorEnabled { get; set; }

        // ============================================================
        // 📌 CAMPOS ADICIONALES
        // ============================================================

        [Column("idcliente")]
        public int? IdCliente { get; set; }

        [Column("nombres")]
        [MaxLength(100)]
        public string? Nombres { get; set; }

        [Column("apellidos")]
        [MaxLength(100)]
        public string? Apellidos { get; set; }

        [Column("telefono")]
        [MaxLength(20)]
        public string? Telefono { get; set; }

        [Column("direccion")]
        [MaxLength(255)]
        public string? Direccion { get; set; }

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Column("estado")]
        public int Estado { get; set; } = 1;

        [Column("fecha_ultimo_login")]
        public DateTime? FechaUltimoLogin { get; set; }

        // ============================================================
        // 📌 PROPIEDAD DE NAVEGACIÓN (SOLO UNA VEZ)
        // ============================================================

        /// <summary>
        /// Relación con la tabla cliente (SOLO UNA VEZ, no duplicar)
        /// </summary>
        [ForeignKey("IdCliente")]
        public virtual Cliente? Cliente { get; set; }

        // ============================================================
        // 📌 PROPIEDADES DE CONVENIENCIA
        // ============================================================

        [NotMapped]
        public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();

        [NotMapped]
        public bool IsActive => Estado == 1;
    }
}