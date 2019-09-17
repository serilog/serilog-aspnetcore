
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Serilog;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Serilog.AspNetCore.Mvc
{
    /// <summary>
    /// Supports logging Asp.Net Core Mvc route information.
    /// </summary>
    public class MvcRequestLoggingFilter : ActionFilterAttribute
    {
        IDiagnosticContext diag;
        public MvcRequestLoggingFilter(
            IDiagnosticContext diag)
        {
            this.diag = diag;
        }

        /// <summary>
        /// Logs values taken from the HttpContext Request.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="context"></param>
        protected virtual void LogHttpContextRequest(
            Dictionary<string,object> values,
            HttpContext context)
        {
            var hostValue = context.Request.Host.Value;
            var hostName = context.Request.Host.Host;
            var hostPort = context.Request.Host.Port;

            Add(values, "HttpContext.Request.Host.Value", hostValue);
            Add(values, "HttpContext.Request.Host.Host", hostName);
            Add(values, "HttpContext.Request.Host.Port", hostPort);
        }

        /// <summary>
        /// Logs values from the ActionExecutingContext.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="context"></param>
        protected virtual void LogControllerExecutingAction(
             Dictionary<string, object> values, 
             ActionExecutingContext context)
        {
            var displayName = context.ActionDescriptor.DisplayName;
            Add(values, "ActionDescriptor.DisplayName", displayName);
        }

        /// <summary>
        /// Logs values from the ControllerActionDescriptor.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="ctrlActionDesc"></param>
        protected virtual void LogControllerActionDescriptor(
             Dictionary<string, object> values, 
             ControllerActionDescriptor ctrlActionDesc)
        {
            var controllerName = ctrlActionDesc.ControllerName;
            var actionName = ctrlActionDesc.ActionName;
            var nameSpace = ctrlActionDesc.ControllerTypeInfo.Namespace;

            Add(values, "ControllerActionDescriptor.ControllerName", controllerName);
            Add(values, "ControllerActionDescriptor.ActionName", actionName);
            Add(values, "ControllerActionDescriptor.ControllerTypeInfo.Namespace", nameSpace);

            var routeTemplate = ctrlActionDesc.AttributeRouteInfo.Template;

            Add(values, "ControllerActionDescriptor.AttributeRouteInfo.Template", routeTemplate);

          
        }

        /// <summary>
        /// Adds value into the temporary cache of values, these are then all writen to the IDiagnosticContext when
        /// OnActionExecuting calls SetDiagFromValues.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual Dictionary<string, object> Add(
            Dictionary<string, object> values, 
            string key, 
            object value)
        {
            values.Add(key, value);
            return values;
        }

        /// <summary>
        /// Can be used to create or modify the cache of items before they are writen to the IDiagnosticContext
        /// </summary>
        /// <param name="values"></param>
        protected virtual void LogCompoundValues(Dictionary<string, object>  values)
        {
            LogCompoundValueTemplateWithHost(values);
        }

        /// <summary>
        /// Built in compoud writer which combines the host and the MVC template.
        /// </summary>
        /// <param name="values"></param>
        protected virtual void LogCompoundValueTemplateWithHost(Dictionary<string, object> values)
        {
            var template = values["ControllerActionDescriptor.AttributeRouteInfo.Template"];
            var host = values["HttpContext.Request.Host.Value"];
            var templateWithHost = host + "/" + template;

            Add(values,
                "ControllerActionDescriptor.AttributeRouteInfo.TemplateWithHost",
                templateWithHost);

        }

        /// <summary>
        /// Writes the values supplied to the IDiagnosticContext for this request.
        /// </summary>
        /// <param name="values"></param>
        protected virtual void SetDiagFromValues(Dictionary<string, object> values)
        {
            foreach (var value in values)
            {
                diag.Set(value.Key, value.Value);
            }
        }

        /// <summary>
        /// Overrides the ActionFilterAttribute. Calls the built in log writers, compound and then finshes by writing to
        /// IDiagnosticContext by calling SetDiagFromValues.
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var values = new Dictionary<string, object>();
            if (context == null)
                return;

            if(context.HttpContext!=null)
            {
                LogHttpContextRequest(values, context.HttpContext);
            }

            if (context.ModelState.IsValid)
            {

                if(context!=null)
                {
                    LogControllerExecutingAction(values, context);
                }

                var ctrlActionDesc = context.ActionDescriptor as ControllerActionDescriptor;
                if(ctrlActionDesc!=null)
                    LogControllerActionDescriptor(values, ctrlActionDesc);
            }

            LogCompoundValues(values);
            SetDiagFromValues(values);
        }
    }
}

