using EventManagerWebRazorPage.Models;
using EventManagerWebRazorPage.ViewModels;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ILogger = Serilog.ILogger;

namespace EventManagerWebRazorPage.Services.Other
{
    public static class Extensions
    {
        public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState)
        {
            foreach(var error in result.Errors)
            {
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }
        public static void ValidateEventDetailViewModel(this ModelStateDictionary modelState, EventDetailViewModel eventDetailViewModel, IValidator<EventDetailViewModel> eventDetailViewModelValidator, bool restrictedMode = false )
        {
            Action<ValidationStrategy<EventDetailViewModel>> selector = options => options.IncludeAllRuleSets();
            if(restrictedMode)
            {
                selector = options => options.IncludeRulesNotInRuleSet();
            }
            ValidationResult result = eventDetailViewModelValidator.Validate(eventDetailViewModel, selector);
            if (!result.IsValid)
            {
                result.AddToModelState(modelState);
            }
        }

        /// <summary>
        /// Send a non-complicated message to Serilog, with options to choose which logging level to send to.
        /// </summary>
        /// <remarks>
        /// Doesn't change any properties from the ServiceResult. If Result is false and overrideLoggingLevel is true, messages will be sent to Error regardless of input loggingLevel. 
        /// </remarks>
        public static ServiceResult SendResultToLog(this ServiceResult serviceResult, ILogger logger, string action, string property, LoggingLevel? loggingLevel = null, bool overrideLoggingLevel = true)
        {
            string output = (serviceResult.Result) ? $"Successfully {action} {property}: {serviceResult}" : $"Failed to {action} {property}: {serviceResult}\r\n" +
                $"Message: {serviceResult.Message}\r\nInternal Message: {serviceResult.InternalErrorMessage}";
            if (!serviceResult.Result && overrideLoggingLevel)
            {
                loggingLevel = LoggingLevel.Error;
            }
            switch (loggingLevel)
            {
                case LoggingLevel.Debug:
                    logger.Debug(output); break;
                case LoggingLevel.Warn:
                    logger.Warning(output); break;
                case LoggingLevel.Error:
                    logger.Error(output); break;
                case LoggingLevel.Fatal:
                    logger.Fatal(output); break;
                default:
                    logger.Information(output); break;
            }
            return serviceResult;
        }
    }
}
