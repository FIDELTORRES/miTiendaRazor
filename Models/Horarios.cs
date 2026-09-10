using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models
{
    /// <summary>
    /// Modelo para la tabla horarios (horarios de atención)
    /// </summary>
    [Table("tbhorarios")]
    public class HorarioEntrega
    {
        [Key]
        [Column("idhorario")]
        public int IdHorario { get; set; }

        [Column("horas")]
        [MaxLength(5)]
        public string? HoraEntrega { get; set; }

        [Column("estado")]
        public int? Estado { get; set; }

        [Column("rucemisor")]
        [MaxLength(20)]
        public string? RucEmisor { get; set; }
    }
}  