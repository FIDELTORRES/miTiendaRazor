using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace miTienda.Pages.Pagos
{
    public class ExitosoModel : PageModel
    {
        public int PedidoId { get; set; }

        public void OnGet(int id)
        {
            PedidoId = id;
        }
    }
}