using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.BossControl.BO;

namespace TSLog.BossControl.WebUI.Pages
{
    public class FreeShiftWorkersModel : PageModel
    {
        public IBossControlStorage Storage { get; private set; }
        public FreeShiftWorkersModel(IBossControlStorage storage)
        {
            Storage = storage;
        }
        public void OnGet()
        {
        }
    }
}
