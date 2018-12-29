﻿// Copyright (c) 2018 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using Microsoft.Extensions.Logging;
using Silverback.Messaging.ErrorHandling;
using Silverback.Messaging.Messages;

namespace Silverback.Messaging.Broker
{
    public abstract class Consumer : EndpointConnectedObject, IConsumer
    {
        private readonly ILogger<Consumer> _logger;

        protected Consumer(IBroker broker, IEndpoint endpoint, ILogger<Consumer> logger)
           : base(broker, endpoint)
        {
            _logger = logger;
        }

        public event EventHandler<IMessage> Received;
        public event EventHandler<ErrorHandlerEventArgs> Error;

        /// <summary>Handles the received message.</summary>
        /// <param name="buffer">The byte array containing the serialized message.</param>
        /// <param name="retryCount">The retry count represent the amount of retries of the very same message (same Kafka
        /// offset).</param>
        protected MessageHandlerResult HandleMessage(byte[] buffer, int retryCount)
        {
            if (Received == null)
                throw new InvalidOperationException("A message was received but no handler is configured, please attach to the Received event.");

            IMessage message = null;

            try
            {
                message = DeserializeMessage(buffer);

                if (retryCount > 0)
                    message = IncrementFailedAttempts(message, retryCount);

                _logger.LogTrace("Message received.", message, Endpoint);

                RaiseReceivedEvent(message);

                return MessageHandlerResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error occurred processing the message.", message, Endpoint);

                var errorArgs = new ErrorHandlerEventArgs(ex, IncrementFailedAttempts(message));
                Error?.Invoke(this, errorArgs);

                return MessageHandlerResult.Error(errorArgs.Action);
            }
        }

        private IMessage DeserializeMessage(byte[] buffer) => Endpoint.Serializer.Deserialize(buffer);

        private static FailedMessage IncrementFailedAttempts(IMessage message, int increment = 1)
        {
            if (message is FailedMessage failedMessage)
            {
                failedMessage.FailedAttempts += increment;
                return failedMessage;
            }

            return new FailedMessage(message, increment);
        }

        private void RaiseReceivedEvent(IMessage message)
        {
            if (message is FailedMessage failedMessage)
            {
                Received.Invoke(this, failedMessage.Message);
            }
            else
            {
                Received.Invoke(this, message);
            }
        }
    }

    public abstract class Consumer<TBroker, TEndpoint> : Consumer
        where TBroker : class, IBroker
        where TEndpoint : class, IEndpoint
    {
        protected Consumer(IBroker broker, IEndpoint endpoint, ILogger<Consumer> logger) 
            : base(broker, endpoint, logger)
        {
        }

        protected new TBroker Broker => (TBroker)base.Broker;

        protected new TEndpoint Endpoint => (TEndpoint)base.Endpoint;
    }
}