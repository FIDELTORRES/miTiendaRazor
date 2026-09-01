using System.ComponentModel.DataAnnotations;

namespace miTienda.ViewModels.Categoria
{
    /// <summary>
    /// ViewModel para la gestión de Subcategorías Página (Nivel 3)
    /// Tabla: subcategoriapagina
    /// Relación: Pertenece a una Categoriapagina (Nivel 2)
    /// </summary>
    public class SubcategoriapaginaViewModel
    {
        [Key]
        public int IdSubcategoriapagina { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(100, ErrorMessage = "Máximo 100 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una categoría página")]
        [Display(Name = "Categoría Página (Nivel 2)")]
        public int IdCategoriapagina { get; set; }
        public string? Imagen { get; set; }

        [Display(Name = "Estado")]
        public int Estado { get; set; } = 1;

        // Propiedades de navegación (para mostrar en listados)
        public string? CategoriaPaginaDesc { get; set; }
        public string? CategoriaPropiaDesc { get; set; } // Para mostrar el nivel 1 también

        [Display(Name = "Estado")]
        public string EstadoDesc => Estado == 1 ? "Activo" : "Inactivo";
    }
}