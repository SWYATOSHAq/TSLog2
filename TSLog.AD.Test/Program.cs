using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;


/*получаем данные о ветке */
DirectoryEntry CurrentDomain = new DirectoryEntry();

var domain = Domain.GetCurrentDomain();
Console.WriteLine(domain.Name);

var domains = domain.Children;
var controllers = domain.DomainControllers;

//var AD = new DirectoryEntry();
//string QWRY = @"(&(objectCategory=person)(objectClass=user))";
//DirectorySearcher searchInAD = new DirectorySearcher(AD, QWRY);
//searchInAD.PageSize = 1001;
//SearchResultCollection results = searchInAD.FindAll();

//int I = 0;
//foreach (SearchResult searchResult in results)
//    Console.WriteLine(I++ + ") " + searchResult.Properties["name"][0]);


// set up domain context
PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain.Name);

// find a user
UserPrincipal user = UserPrincipal.FindByIdentity(ctx, "m.burov");

// find the group in question
GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, "Managers");
GroupPrincipal group2 = GroupPrincipal.FindByIdentity(ctx, "YardAdministrators");

if (user != null)
{
    // check if user is member of that group
    if (user.IsMemberOf(group))
        Console.WriteLine("In group");
    if (user.IsMemberOf(group2))
        Console.WriteLine("In group2");
}



Console.WriteLine(CurrentDomain.Name);

Console.ReadKey();