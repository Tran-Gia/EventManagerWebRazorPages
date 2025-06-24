namespace EventManagerWebRazorPage.Services.Other
{
    /// <summary>
    /// Returns the result of a service, whether it's successful or not, and if not, provides messages indicating why it failed for users and/or developers.
    /// </summary>
    /// <remarks>
    /// Provides error message to be sent to View through parameter message, internal error messages for debugging through parameter internalMessage, and the service's result through parameter result
    /// </remarks>

    public class ServiceResult
    {
        private bool _result;
        private string? _message;
        private string? _internalErrorMessage;
        private int _totalServices = 1;
        private int _failedServices = 0;
        /// <summary>
        /// Returns the result of a service, whether it's successful or not, and if not, provides messages indicating why it failed to the user and internal error messages for developer.
        /// </summary>
        /// /// <remarks>
        /// Result is default to true if left undefined.
        /// </remarks>
        public ServiceResult(string message= "", string internalErrorMessage = "", bool result = true)
        {
            _message = message;
            _result = result;
            _internalErrorMessage = internalErrorMessage;
            if(!result)
            {
                _failedServices++;
            }
        }
        /// <summary>
        /// Returns the result of a service, whether it's successful or not, and if not, provides messages indicating why it failed to the user.
        /// </summary>
        /// <remarks>
        /// This method requires only a message and an optional value to result. Result is true if not defined.
        /// </remarks>
        public ServiceResult(string message, bool result = true)
        {
            _message = message;
            _result = result;
            _internalErrorMessage = string.Empty;
            if (!result)
            {
                _failedServices++;
            }
        }
        /// <summary>
        /// Returns the result of a service. "True" means the service has been completed successfully.
        /// </summary>
        public bool Result { get { return _result; } }
        /// <summary>
        /// Returns the error message if the service failed. Does not contain sensitive information.
        /// </summary>
        public string Message { get 
            {
                if (String.IsNullOrEmpty(_message))
                    _message = "No error message found";
                return _message; 
            }
        }
        /// <summary>
        /// Returns the error message if the service failed. May contain sensitive information.
        /// </summary>
        public string InternalErrorMessage 
        { 
            get 
            { 
                if(String.IsNullOrEmpty(_internalErrorMessage))
                    _internalErrorMessage = "No internal error message found";
                return _internalErrorMessage;
            } 
        }
        /// <summary>
        /// Combines another ServiceResult into this ServiceResult to return.
        /// </summary>
        public ServiceResult Append(ServiceResult serviceResult)
        {
            if(!serviceResult.Result)
            {
                _result = false;
                _message += "\n" + serviceResult.Message;
                _internalErrorMessage += "\n" + serviceResult.InternalErrorMessage;
            }
            _totalServices+= serviceResult._totalServices;
            _failedServices += serviceResult._failedServices;
            return this;
        }
        /// <summary>
        /// Combines multiple ServiceResult into one single ServiceResult to return.
        /// </summary>
        public ServiceResult Append(IEnumerable<ServiceResult> serviceResults)
        {
            foreach(var serviceResult in serviceResults)
            {
                Append(serviceResult);
            }
            return this;
        }
        /// <summary>
        /// Update values for a single ServiceResult. Will replace all existing values from it. Do not use for multi-layered ServiceResult.
        /// </summary>
        /// <remarks>
        /// This method will add an internal message stating it has replaced previous values if it's forced to be used from a multi-layered ServiceResult. Calling this method again completely removes the warning above.
        /// </remarks>
        public ServiceResult UpdateSingle(string message = "", string internalErrorMessage = "", bool result = true)
        {
            _message = message;
            _internalErrorMessage = internalErrorMessage;
            _result = result;
            if (result)
            {
                _failedServices = 0;
            }
            else
            {
                _failedServices = 1;
            }
            if (_totalServices > 1)
            {
                _internalErrorMessage = $"WARNING: This ServiceResult has been replaced. It previously has {_totalServices} total services." + "\n" + _internalErrorMessage;
            }
            _totalServices = 1;
            return this;
        }

        public override string ToString()
        {
            return $"Summary: total {_totalServices} services, {_totalServices - _failedServices} passed, {_failedServices} failed.";
        }
    }
    public enum LoggingLevel
    {
        Debug = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
        Fatal = 4
    }
}
