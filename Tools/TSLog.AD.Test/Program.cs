using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    /* Получаем данные о ветке. */
    DirectoryEntry CurrentDomain = new();

    var domain = Domain.GetCurrentDomain();
    Console.WriteLine(domain.Name);

    // Set up domain context.
    PrincipalContext ctx = new(ContextType.Domain, domain.Name);

    // Find a user.
    UserPrincipal user = UserPrincipal.FindByIdentity(ctx, "m.burov");

    // Find the group in question.
    GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, "Managers");
    GroupPrincipal group2 = GroupPrincipal.FindByIdentity(ctx, "YardAdministrators");

    if (user != null)
    {
        // Check if user is member of that group.
        if (user.IsMemberOf(group))
            Console.WriteLine("In group");
        if (user.IsMemberOf(group2))
            Console.WriteLine("In group2");
    }

    Console.WriteLine(CurrentDomain.Name);
    Console.ReadKey();
}
