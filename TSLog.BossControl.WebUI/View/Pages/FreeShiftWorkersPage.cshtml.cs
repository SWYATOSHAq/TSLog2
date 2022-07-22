using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.BossControl.BO;

namespace TSLog.BossControl.WebUI.Pages
{
    public class FreeShiftWorkersPageModel : PageModel
    {
        public IBossControlStorage Storage { get; private set; }
        public FreeShiftWorkersPageModel(IBossControlStorage storage)
        {
            Storage = storage;
        }
        public void OnGet()
        {
        }
    }
}
