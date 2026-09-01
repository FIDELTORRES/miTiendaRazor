using System.ComponentModel.DataAnnotations;

namespace miTienda.ViewModels.Categoria
{
    public class CategoriapaginaViewModel
    {
        public int IdCategoriapagina { get; set; }

        [Required(ErrorMessage = "Seleccione una categoría propia")]
        [Display(Name = "Categoría Propia (Nivel 1)")]
        public int IdCategoriapropia { get; set; }

        [Required(ErrorMessage = "Seleccione una subcategoría UNSPSC")]
        [Display(Name = "Subcategoría (UNSPSC)")]
        public string IdSubcategoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(100, ErrorMessage = "Máximo 100 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Imagen")]
        public string? Imagen { get; set; }

        [Display(Name = "Estado")]
        public int Estado { get; set; } = 1;

        // Propiedades para mostrar en listados
        public string? CategoriaPropiaDesc { get; set; }
        public string? SubcategoriaDesc { get; set; }

        [Display(Name = "Estado")]
        public string EstadoDesc => Estado == 1 ? "Activo" : "Inactivo";
    }
}