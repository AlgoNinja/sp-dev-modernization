﻿using System;

namespace SharePointPnP.Modernization.Framework.Functions
{
    /// <summary>
    /// Exception class thrown when a SharePoint resource (e.g. file) is not available
    /// </summary>
    public class MediaWebpartConfigurationException: Exception
    {
        /// <summary>
        /// Throws a ResourceNotFoundException message
        /// </summary>
        /// <param name="message">Error message</param>
        public MediaWebpartConfigurationException(string message) : base(message) { }

        /// <summary>
        /// Throws a ResourceNotFoundException message
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Inner exception object</param>
        public MediaWebpartConfigurationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
