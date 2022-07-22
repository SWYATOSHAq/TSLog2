using Microsoft.AspNetCore.Mvc.RazorPages;
using TSLog.BossControl.BO;

namespace TSLog.BossControl.WebUI.Pages
{
    public class BusyWorkersModel : PageModel
    {
        public IBossControlStorage Storage { get; private set; }
        public BusyWorkersModel(IBossControlStorage storage)
        {
            Storage = storage;
        }

        public void OnGet()
        {
        }
    }
}
