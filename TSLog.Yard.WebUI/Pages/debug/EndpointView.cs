using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Linq;

// [GET] https://localhost:5001/-/info/endpoints
// https://stackoverflow.com/questions/28435734/how-to-get-a-list-of-all-routes-in-asp-net-core
// TODO debug, delete on release

//[Authorize]
[Route("/-/{controller}")]
public class InfoController : Controller
{
    private readonly IEnumerable<EndpointDataSource> _endpointSources;

    public InfoController(
        IEnumerable<EndpointDataSource> endpointSources
    )
    {
        _endpointSources = endpointSources;
    }

    [HttpGet("endpoints")]
    public ActionResult ListAllEndpoints()
    {
        var endpoints = _endpointSources
            .SelectMany(es => es.Endpoints)
            .OfType<RouteEndpoint>();
        var output = endpoints.Select(
            e =>
            {
                var controller = e.Metadata
                    .OfType<ControllerActionDescriptor>()
                    .FirstOrDefault();
                var action = controller != null
                    ? $"{controller.ControllerName}.{controller.ActionName}"
                    : null;
                var controllerMethod = controller != null
                    ? $"{controller.ControllerTypeInfo.FullName}:{controller.MethodInfo.Name}"
                    : null;
                return new
                {
                    Method = e.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods?[0],
                    Route = $"/{e.RoutePattern.RawText.TrimStart('/')}",
                    Action = action,
                    ControllerMethod = controllerMethod
                };
            }
        );

        return Json(output);
    }
}