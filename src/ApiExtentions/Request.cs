namespace MinimapApiExample.ApiExtentions;

using System.Security.Claims;

public interface IRequest
{
}

public interface IFromQuery
{
    void BindFromQuery(IQueryCollection queryCollection);
}

public interface IFromRoute
{
    void BindFromRoute(RouteValueDictionary routeValueDictionary);
}

public interface IWithUserContext
{
    void BindFromUser(ClaimsPrincipal claimsPrincipal);
}

public interface IFromJsonBody
{
}

public interface IRequest<TOut> : IRequest
{
} 