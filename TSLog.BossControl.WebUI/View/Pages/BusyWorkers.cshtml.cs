using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.BossControl.BO;

namespace TSLog.BossControl.WebUI.Pages
{
    public class BusyWorkersModel : PageModel
    {
        public readonly IBossControlStorage Storage;
        public BusyWorkersModel(IBossControlStorage storage)
        {
            Storage = storage;
        }

        public void OnGet()
        {
        }
    }
}
