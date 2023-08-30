
using CCC.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;

namespace CCC.API
{
    public class CoordinatorRoleConstraint : IRouteConstraint
    {
        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var value = values[routeKey] as string;
            if (!string.IsNullOrEmpty(value))
            {
                return Enum.TryParse(value, out CoordinatorRole _);
            }
            return false;
        }
    }
}