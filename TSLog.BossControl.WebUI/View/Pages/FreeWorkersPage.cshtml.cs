using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.BossControl.BO;

namespace TSLog.BossControl.WebUI.Pages
{
    public class FreeWorkersPageModel : PageModel
    {
        public IBossControlStorage Storage { get; private set; }
        public FreeWorkersPageModel(IBossControlStorage storage)
        {
            Storage = storage;
        }
        public void OnGet()
        {
        }
    }
}
