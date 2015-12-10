using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Claims
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            Setup(config);
        }
        
        public static void Setup(HttpConfiguration config)
        {
            //config.EnableSystemDiagnosticsTracing();


            // Web API configuration and services
            config.Formatters.Clear();
            config.Formatters.Add(new MitchellXmlFormatter());

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
