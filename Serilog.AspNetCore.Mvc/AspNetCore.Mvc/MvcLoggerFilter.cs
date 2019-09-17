using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Serilog.AspNetCore.Mvc
{
    /// <summary>
    /// Supports logging AspNetCore Mvc route information to Serilog hosting request logging.
    /// </summary>
    public class MvcRequestLoggingFilter : ActionFilterAttribute
    {
        private const string Name_ActionControllerName = "ControllerActionDescriptor.ControllerName";
        private const string Name_ActionName = "ControllerActionDescriptor.ActionName";
        private const string Name_ActionControllerNamespace = "ControllerActionDescriptor.ControllerTypeInfo.Namespace";
        private const string Name_Template = "ControllerActionDescriptor.AttributeRouteInfo.Template";
        private const string Name_HostValue = "HttpContext.Request.Host.Value";
        private const string Name_HostHost = "HttpContext.Request.Host.Host";
        private const string Name_HostPort = "HttpContext.Request.Host.Post";
        private const string Name_ActionDisplayName = "ActionDescriptor.DisplayName";
        private const string Name_TemplateWithHost = "ControllerActionDescriptor.AttributeRouteInfo.TemplateWithHost";
        private const string Name_TemplateWithHostJoiner = "/";

        private readonly ILogger<MvcRequestLoggingFilter> _logger;
        private readonly IDiagnosticContext _diag;

        public MvcRequestLoggingFilter(
            IDiagnosticContext diag,
            ILogger<MvcRequestLoggingFilter> logger)
        {
            this._logger = logger;
            this._diag = diag;
        }

        /// <summary>
        /// Logs values taken from the HttpContext Request.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="context"></param>
        protected virtual void LogHttpContextRequest(
            IDictionary<string, object> values,
            HttpContext context)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Add(values, Name_HostValue, context.Request.Host.Value);
            Add(values, Name_HostHost, context.Request.Host.Host);
            Add(values, Name_HostPort, context.Request.Host.Port);
        }

        /// <summary>
        /// Logs values from the ActionExecutingContext.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="context"></param>
        protected virtual void LogControllerExecutingAction(
             IDictionary<string, object> values,
             ActionExecutingContext context)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Add(values, Name_ActionDisplayName, context.ActionDescriptor.DisplayName);
        }

        /// <summary>
        /// Logs values from the ControllerActionDescriptor.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="ctrlActionDesc"></param>
        protected virtual void LogControllerActionDescriptor(
             IDictionary<string, object> values,
             ControllerActionDescriptor ctrlActionDesc)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (ctrlActionDesc == null)
                throw new ArgumentNullException(nameof(ctrlActionDesc));

            Add(values, Name_ActionControllerName, ctrlActionDesc.ControllerName);
            Add(values, Name_ActionName, ctrlActionDesc.ActionName);
            Add(values, Name_ActionControllerNamespace, ctrlActionDesc.ControllerTypeInfo.Namespace);
            Add(values, Name_Template, ctrlActionDesc.AttributeRouteInfo.Template);
        }

        /// <summary>
        /// Adds value into the temporary cache of values, these are then all writen to the IDiagnosticContext when
        /// OnActionExecuting calls SetDiagFromValues.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual IDictionary<string, object> Add(
            IDictionary<string, object> values,
            string key,
            object value)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (values.ContainsKey(key))
            {
                _logger.LogDebug("Replacing {key} from {existing} to {new} ", key, values[key], value);
                values[key] = value;
            }
            else
                values.Add(key, value);

            return values;
        }

        /// <summary>
        /// Can be used to create or modify the cache of items before they are writen to the IDiagnosticContext
        /// </summary>
        /// <param name="values"></param>
        protected virtual void LogConcatenatedValues(IDictionary<string, object> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            LogConcatenateTemplateWithHost(values);
        }

        /// <summary>
        /// Built in compoud writer which combines the host and the MVC template.
        /// </summary>
        /// <param name="values"></param>
        protected virtual void LogConcatenateTemplateWithHost(IDictionary<string, object> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            Concatenate(
                values,
                Name_HostValue,
                Name_Template,
                Name_TemplateWithHostJoiner,
                Name_TemplateWithHost);
        }

        /// <summary>
        /// Takes the value of 2 previously logged values and joins them into one.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="name1"></param>
        /// <param name="name2"></param>
        /// <param name="joinWith"></param>
        /// <param name="newName"></param>
        protected virtual void Concatenate(
            IDictionary<string, object> values,
            string name1,
            string name2,
            string joinWith,
            string newName)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (name1 == null)
                throw new ArgumentNullException(nameof(name1));

            if (name2 == null)
                throw new ArgumentNullException(nameof(name2));

            if (joinWith == null)
                throw new ArgumentNullException(nameof(joinWith));

            if (newName == null)
                throw new ArgumentNullException(nameof(newName));

            if (values.ContainsKey(name1) && values.ContainsKey(name2))
                Add(values,
                    newName,
                    values[name1] + joinWith + values[name2]);
            else
                _logger.LogDebug("Cannot concatenate {name1} and {name2} as one or more do not exist. ", name1, name2);

        }

        /// <summary>
        /// Writes the values supplied to the IDiagnosticContext for this request.
        /// </summary>
        /// <param name="values"></param>
        protected virtual void WriteToDiagnosticsContext(IDictionary<string, object> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            foreach (var value in values)
            {
                if (value.Key != null && value.Value != null)
                {
                    _logger.LogDebug("Logging to IDiagnosticContext: {name} {value}", value.Key, value.Value);
                    _diag.Set(value.Key, value.Value);
                }
                else
                    _logger.LogDebug(
                        "Not Logging to IDiagnosticContext as key and/or value are null, {name} {value}",
                        value.Key ?? string.Empty,
                        value.Value ?? string.Empty);
            }
        }

        /// <summary>
        /// Creates a default dictionary for value storage.
        /// </summary>
        /// <returns></returns>
        protected virtual IDictionary<string, object> InitializeValueStore()
        {
            return new Dictionary<string, object>();
        }

        /// <summary>
        /// Logs values from properties in ActionExecutingContext and child objects.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="values"></param>
        protected virtual void LogRawValues(
            ActionExecutingContext context, 
            IDictionary<string, object> values)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (context.HttpContext != null)
                LogHttpContextRequest(values, context.HttpContext);
            else
                _logger.LogDebug("OnActionExecuting parameter context.HttpContext of type HttpContext is null, skipping Mvc logging LogHttpContextRequest.");

            if (context.ModelState.IsValid)
            {
                LogControllerExecutingAction(values, context);

                if (context.ActionDescriptor is ControllerActionDescriptor ctrlActionDesc)
                    LogControllerActionDescriptor(values, ctrlActionDesc);
                else
                    _logger.LogDebug("OnActionExecuting parameter context.ActionDescriptor of type ControllerActionDescriptor is null, skipping Mvc logging LogControllerActionDescriptor.");
            }
            else
                _logger.LogDebug("OnActionExecuting parameter context.ModelState.IsValid is false skipping some Mvc logging.");
        }

        /// <summary>
        /// Logs Raw values and concatenated values.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="values"></param>
        protected virtual void Log(
            ActionExecutingContext context, 
            IDictionary<string, object> values)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (values == null)
                throw new ArgumentNullException(nameof(values));

            LogRawValues(context, values);
            LogConcatenatedValues(values);
        }

        /// <summary>
        /// Overrides the ActionFilterAttribute. Calls the built in log writers, compound and then finishes by writing to
        /// IDiagnosticContext by calling SetDiagFromValues.
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
            {
                _logger.LogDebug("OnActionExecuting parameter context of type ActionExecutingContext is null, skipping Mvc logging.");
                return;
            }

            var values = InitializeValueStore();
            Log(context, values);
            WriteToDiagnosticsContext(values);
        }
    }
}

