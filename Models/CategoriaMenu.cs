namespace miTienda.Models
{
    // Clase para los datos planos devueltos por SQL
    public class CategoriaMenu
    {
        public string? IdSubcategoria { get; set; }
        public string? IdCategoriapropia { get; set; }
        public string? CategoriaPropiaDesc { get; set; }
        public string? IdCategoriapagina { get; set; }
        public string? CategoriaPaginaDesc { get; set; }
        public string? IdSubcategoriapagina { get; set; }
        public string? SubcategoriaPaginaDesc { get; set; }
    }

    // Nivel 1
    public class CategoriaNivel1
    {
        public string? IdCategoriapropia { get; set; }
        public string? CategoriaPropiaDesc { get; set; }
        public List<CategoriaNivel2> Nivel2 { get; set; } = new();
    }

    // Nivel 2
    public class CategoriaNivel2
    {
        public string? IdCategoriapagina { get; set; }
        public string? CategoriaPaginaDesc { get; set; }
        public List<CategoriaNivel3> Nivel3 { get; set; } = new();
    }

    // Nivel 3 (hoja)
    public class CategoriaNivel3
    {
        public string? IdSubcategoria { get; set; }
        public string? SubcategoriaPaginaDesc { get; set; }
    }
}