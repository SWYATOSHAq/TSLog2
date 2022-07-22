using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.BossControl.BO;

namespace TSLog.BossControl.WebUI.Pages
{
    public class BusyWorkersPageModel : PageModel
    {
        public IBossControlStorage Storage { get; private set; }
        public BusyWorkersPageModel(IBossControlStorage storage)
        {
            Storage = storage;
        }

        public void OnGet()
        {
        }
    }
}
