using System.ComponentModel.DataAnnotations;

namespace miTienda.ViewModels.Categoria
{
    /// <summary>
    /// ViewModel para la gestión de Categorías Propias (Nivel 1)
    /// Tabla: categoriapropia
    /// </summary>
    public class CategoriapropiaViewModel
    {
        [Key]
        public int IdCategoriapropia { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(100, ErrorMessage = "Máximo 100 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;
        [Display(Name = "Imagen")]
        public string? Imagen { get; set; }

        [Display(Name = "Estado")]
        public int Estado { get; set; } = 1; // 1 = Activo, 0 = Inactivo

        // Para mostrar en listados
        [Display(Name = "Estado")]
        public string EstadoDesc => Estado == 1 ? "Activo" : "Inactivo";
    }
}