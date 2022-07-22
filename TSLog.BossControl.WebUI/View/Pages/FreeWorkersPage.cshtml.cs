using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.BossControl.BO;

namespace TSLog.BossControl.WebUI.Pages
{
    public class FreeWorkersModel : PageModel
    {
        public IBossControlStorage Storage { get; private set; }
        public FreeWorkersModel(IBossControlStorage storage)
        {
            Storage = storage;
        }
        public void OnGet()
        {
        }
    }
}
