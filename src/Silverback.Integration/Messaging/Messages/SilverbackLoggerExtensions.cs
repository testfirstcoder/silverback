﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Silverback.Diagnostics;
using Silverback.Util;

namespace Silverback.Messaging.Messages
{
    /// <summary>
    ///     Adds some methods to the <see cref="ISilverbackLogger" /> used to consistently enrich the log entry
    ///     with the information about the message(s) being consumed.
    /// </summary>
    public static class SilverbackLoggerExtensions
    {
        /// <summary>
        ///     Writes the standard <i>"Processing inbound message"</i> or
        ///     <i>"Processing the batch of # inbound messages"</i> log message.
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="envelopes">
        ///     The collection of <see cref="IRawBrokerEnvelope" /> containing the messages being processed.
        /// </param>
        public static void LogProcessing(
            this ISilverbackLogger logger,
            IReadOnlyCollection<IRawBrokerEnvelope> envelopes)
        {
            Check.NotEmpty(envelopes, nameof(envelopes));

            var message = envelopes.Count > 1
                ? $"Processing the batch of {envelopes.Count} inbound messages."
                : "Processing inbound message.";

            LogInformationWithMessageInfo(logger, IntegrationEventIds.ProcessingInboundMessage, message, envelopes);
        }

        /// <summary>
        ///     Writes the standard <i>"Error occurred processing the inbound message"</i> or
        ///     <i>"Error occurred processing the batch of # inbound messages"</i> log message.
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="envelopes">
        ///     The collection of <see cref="IRawBrokerEnvelope" /> containing the messages being processed.
        /// </param>
        /// <param name="exception">
        ///     The exception to log.
        /// </param>
        public static void LogProcessingError(
            this ISilverbackLogger logger,
            IReadOnlyCollection<IRawBrokerEnvelope> envelopes,
            Exception exception)
        {
            Check.NotEmpty(envelopes, nameof(envelopes));

            var message = envelopes.Count > 1
                ? $"Error occurred processing the batch of {envelopes.Count} inbound messages."
                : "Error occurred processing the inbound message.";

            LogWarningWithMessageInfo(
                logger,
                IntegrationEventIds.ErrorProcessingInboundMessage,
                exception,
                message,
                envelopes);
        }

        /// <summary>
        ///     Writes a trace log message, enriching it with the information related to the provided message.
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelope">
        ///     The <see cref="IRawBrokerEnvelope" /> containing the message related to the this log.
        /// </param>
        public static void LogTraceWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            string logMessage,
            IRawBrokerEnvelope envelope) =>
            LogWithMessageInfo(logger, LogLevel.Trace, eventId, null, logMessage, new[] { envelope });

        /// <summary>
        ///     Writes a trace log message, enriching it with the information related to the provided message(s).
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelopes">
        ///     The collection of <see cref="IRawBrokerEnvelope" /> containing the message(s) related to the this
        ///     log.
        /// </param>
        public static void LogTraceWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            string logMessage,
            IReadOnlyCollection<IRawBrokerEnvelope> envelopes) =>
            LogWithMessageInfo(logger, LogLevel.Trace, eventId, null, logMessage, envelopes);

        /// <summary>
        ///     Writes a debug log message, enriching it with the information related to the provided message.
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelope">
        ///     The <see cref="IRawBrokerEnvelope" /> containing the message related to the this log.
        /// </param>
        public static void LogDebugWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            string logMessage,
            IRawBrokerEnvelope envelope) =>
            LogWithMessageInfo(logger, LogLevel.Debug, eventId, null, logMessage, new[] { envelope });

        /// <summary>
        ///     Writes a debug log message, enriching it with the information related to the provided message(s).
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelopes">
        ///     The collection of <see cref="IRawBrokerEnvelope" /> containing the message(s) related to the this
        ///     log.
        /// </param>
        public static void LogDebugWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            string logMessage,
            IReadOnlyCollection<IRawBrokerEnvelope> envelopes) =>
            LogWithMessageInfo(logger, LogLevel.Debug, eventId, null, logMessage, envelopes);

        /// <summary>
        ///     Writes an information log message, enriching it with the information related to the provided
        ///     message.
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelope">
        ///     The <see cref="IRawBrokerEnvelope" /> containing the message related to the this log.
        /// </param>
        public static void LogInformationWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            string logMessage,
            IRawBrokerEnvelope envelope) =>
            LogWithMessageInfo(logger, LogLevel.Information, eventId, null, logMessage, new[] { envelope });

        /// <summary>
        ///     Writes an information log message, enriching it with the information related to the provided
        ///     message(s).
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelopes">
        ///     The collection of <see cref="IRawBrokerEnvelope" /> containing the message(s) related to the this
        ///     log.
        /// </param>
        public static void LogInformationWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            string logMessage,
            IReadOnlyCollection<IRawBrokerEnvelope> envelopes) =>
            LogWithMessageInfo(logger, LogLevel.Information, eventId, null, logMessage, envelopes);

        /// <summary>
        ///     Writes a warning log message, enriching it with the information related to the provided message.
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelope">
        ///     The <see cref="IRawBrokerEnvelope" /> containing the message related to the this log.
        /// </param>
        public static void LogWarningWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            string logMessage,
            IRawBrokerEnvelope envelope) =>
            LogWithMessageInfo(logger, LogLevel.Warning, eventId, null, logMessage, new[] { envelope });

        /// <summary>
        ///     Writes a warning log message, enriching it with the information related to the provided message(s).
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelopes">
        ///     The collection of <see cref="IRawBrokerEnvelope" /> containing the message(s) related to the this
        ///     log.
        /// </param>
        public static void LogWarningWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            string logMessage,
            IReadOnlyCollection<IRawBrokerEnvelope> envelopes) =>
            LogWithMessageInfo(logger, LogLevel.Warning, eventId, null, logMessage, envelopes);

        /// <summary>
        ///     Writes a warning log message, enriching it with the information related to the provided message.
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="exception">
        ///     The exception to log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelope">
        ///     The <see cref="IRawBrokerEnvelope" /> containing the message related to the this log.
        /// </param>
        public static void LogWarningWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            Exception exception,
            string logMessage,
            IRawBrokerEnvelope envelope) =>
            LogWithMessageInfo(logger, LogLevel.Warning, eventId, exception, logMessage, new[] { envelope });

        /// <summary>
        ///     Writes a warning log message, enriching it with the information related to the provided message(s).
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="exception">
        ///     The exception to log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelopes">
        ///     The collection of <see cref="IRawBrokerEnvelope" /> containing the message(s) related to the this
        ///     log.
        /// </param>
        public static void LogWarningWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            Exception exception,
            string logMessage,
            IReadOnlyCollection<IRawBrokerEnvelope> envelopes) =>
            LogWithMessageInfo(logger, LogLevel.Warning, eventId, exception, logMessage, envelopes);

        /// <summary>
        ///     Writes an error log message, enriching it with the information related to the provided message.
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelope">
        ///     The <see cref="IRawBrokerEnvelope" /> containing the message related to the this log.
        /// </param>
        public static void LogErrorWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            string logMessage,
            IRawBrokerEnvelope envelope) =>
            LogWithMessageInfo(logger, LogLevel.Error, eventId, null, logMessage, new[] { envelope });

        /// <summary>
        ///     Writes an error log message, enriching it with the information related to the provided message(s).
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelopes">
        ///     The collection of <see cref="IRawBrokerEnvelope" /> containing the message(s) related to the this
        ///     log.
        /// </param>
        public static void LogErrorWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            string logMessage,
            IReadOnlyCollection<IRawBrokerEnvelope> envelopes) =>
            LogWithMessageInfo(logger, LogLevel.Error, eventId, null, logMessage, envelopes);

        /// <summary>
        ///     Writes an error log message, enriching it with the information related to the provided message.
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="exception">
        ///     The exception to log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelope">
        ///     The <see cref="IRawBrokerEnvelope" /> containing the message related to the this log.
        /// </param>
        public static void LogErrorWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            Exception exception,
            string logMessage,
            IRawBrokerEnvelope envelope) =>
            LogWithMessageInfo(logger, LogLevel.Error, eventId, exception, logMessage, new[] { envelope });

        /// <summary>
        ///     Writes an error log message, enriching it with the information related to the provided message(s).
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="exception">
        ///     The exception to log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelopes">
        ///     The collection of <see cref="IRawBrokerEnvelope" /> containing the message(s) related to the this
        ///     log.
        /// </param>
        public static void LogErrorWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            Exception exception,
            string logMessage,
            IReadOnlyCollection<IRawBrokerEnvelope> envelopes) =>
            LogWithMessageInfo(logger, LogLevel.Error, eventId, exception, logMessage, envelopes);

        /// <summary>
        ///     Writes a critical log message, enriching it with the information related to the provided message.
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelope">
        ///     The <see cref="IRawBrokerEnvelope" /> containing the message related to the this log.
        /// </param>
        public static void LogCriticalWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            string logMessage,
            IRawBrokerEnvelope envelope) =>
            LogWithMessageInfo(logger, LogLevel.Critical, eventId, null, logMessage, new[] { envelope });

        /// <summary>
        ///     Writes an critical log message, enriching it with the information related to the provided message(s).
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelopes">
        ///     The collection of <see cref="IRawBrokerEnvelope" /> containing the message(s) related to the this
        ///     log.
        /// </param>
        public static void LogCriticalWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            string logMessage,
            IReadOnlyCollection<IRawBrokerEnvelope> envelopes) =>
            LogWithMessageInfo(logger, LogLevel.Critical, eventId, null, logMessage, envelopes);

        /// <summary>
        ///     Writes an critical log message, enriching it with the information related to the provided message.
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="exception">
        ///     The exception to log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelope">
        ///     The <see cref="IRawBrokerEnvelope" /> containing the message related to the this log.
        /// </param>
        public static void LogCriticalWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            Exception exception,
            string logMessage,
            IRawBrokerEnvelope envelope) =>
            LogWithMessageInfo(logger, LogLevel.Critical, eventId, exception, logMessage, new[] { envelope });

        /// <summary>
        ///     Writes a critical log message, enriching it with the information related to the provided message(s).
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="exception">
        ///     The exception to log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelopes">
        ///     The collection of <see cref="IRawBrokerEnvelope" /> containing the message(s) related to the this
        ///     log.
        /// </param>
        public static void LogCriticalWithMessageInfo(
            this ISilverbackLogger logger,
            EventId eventId,
            Exception exception,
            string logMessage,
            IReadOnlyCollection<IRawBrokerEnvelope> envelopes) =>
            LogWithMessageInfo(logger, LogLevel.Critical, eventId, exception, logMessage, envelopes);

        /// <summary>
        ///     Writes a log message at the specified log level, enriching it with the information related to the
        ///     provided message(s).
        /// </summary>
        /// <param name="logger">
        ///     The <see cref="ISilverbackLogger" /> to write to.
        /// </param>
        /// <param name="logLevel">
        ///     Entry will be written on this level.
        /// </param>
        /// <param name="eventId">
        ///     The event id associated with the log.
        /// </param>
        /// <param name="exception">
        ///     The exception to log.
        /// </param>
        /// <param name="logMessage">
        ///     The log message.
        /// </param>
        /// <param name="envelopes">
        ///     The collection of <see cref="IRawBrokerEnvelope" /> containing the message(s) related to the this
        ///     log.
        /// </param>
        public static void LogWithMessageInfo(
            this ISilverbackLogger logger,
            LogLevel logLevel,
            EventId eventId,
            Exception? exception,
            string logMessage,
            IReadOnlyCollection<IRawBrokerEnvelope> envelopes)
        {
            Check.NotNull(logger, nameof(logger));

            if (!logger.IsEnabled(logLevel))
                return;

            Check.NotEmpty(logMessage, nameof(logMessage));
            Check.NotEmpty(envelopes, nameof(envelopes));

            var arguments = GetLogArguments(envelopes, ref logMessage);

            if (exception != null)
                logger.Log(logLevel, eventId, exception, logMessage, arguments);
            else
                logger.Log(logLevel, eventId, logMessage, arguments);
        }

        private static object?[] GetLogArguments(
            IReadOnlyCollection<IRawBrokerEnvelope> envelopes,
            ref string logMessage)
        {
            var firstEnvelope = envelopes.FirstOrDefault();

            if (firstEnvelope == null)
                return Array.Empty<object>();

            if (firstEnvelope is IRawInboundEnvelope inboundEnvelope)
            {
                return envelopes.Count == 1 && !firstEnvelope.Headers.Contains(DefaultMessageHeaders.BatchId)
                    ? GetInboundLogArguments(inboundEnvelope, ref logMessage)
                    : GetInboundBatchLogArguments(inboundEnvelope, ref logMessage);
            }
            else
            {
                return envelopes.Count == 1
                    ? GetOutboundLogArguments(firstEnvelope, ref logMessage)
                    : GetOutboundBatchLogArguments(firstEnvelope, ref logMessage);
            }
        }

        private static object?[] GetInboundLogArguments(IRawInboundEnvelope envelope, ref string logMessage)
        {
            logMessage += LogTemplates.GetInboundMessageLogTemplate(envelope.Endpoint);

            var arguments = new List<object?>
            {
                envelope.ActualEndpointName,
                envelope.Headers.GetValueOrDefault<int>(DefaultMessageHeaders.FailedAttempts),
                envelope.Headers.GetValue(DefaultMessageHeaders.MessageType),
                envelope.Headers.GetValue(DefaultMessageHeaders.MessageId)
            };

            foreach (string key in LogTemplates.GetInboundMessageArguments(envelope.Endpoint))
            {
                arguments.Add(envelope.AdditionalLogData.TryGetValue(key, out string value) ? value : null);
            }

            return arguments.ToArray();
        }

        private static object?[] GetInboundBatchLogArguments(IRawInboundEnvelope envelope, ref string logMessage)
        {
            logMessage += LogTemplates.GetInboundBatchLogTemplate(envelope.Endpoint);

            return new object?[]
            {
                envelope.ActualEndpointName,
                envelope.Headers.GetValueOrDefault<int>(DefaultMessageHeaders.FailedAttempts),
                envelope.Headers.GetValue(DefaultMessageHeaders.BatchId),
                envelope.Headers.GetValue(DefaultMessageHeaders.BatchSize)
            };
        }

        private static object?[] GetOutboundLogArguments(IRawBrokerEnvelope envelope, ref string logMessage)
        {
            logMessage += LogTemplates.GetOutboundMessageLogTemplate(envelope.Endpoint);

            var arguments = new List<object?>
            {
                envelope.Endpoint?.Name,
                envelope.Headers.GetValue(DefaultMessageHeaders.MessageType),
                envelope.Headers.GetValue(DefaultMessageHeaders.MessageId)
            };

            foreach (string key in LogTemplates.GetOutboundMessageArguments(envelope.Endpoint))
            {
                arguments.Add(envelope.AdditionalLogData.TryGetValue(key, out string value) ? value : null);
            }

            return arguments.ToArray();
        }

        private static object?[] GetOutboundBatchLogArguments(IRawBrokerEnvelope envelope, ref string logMessage)
        {
            logMessage += LogTemplates.GetOutboundBatchLogTemplate(envelope.Endpoint);

            return new object?[]
            {
                envelope.Endpoint?.Name
            };
        }
    }
}