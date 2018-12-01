﻿using System;
using Silverback.Messaging.ErrorHandling;
using Silverback.Messaging.Messages;

namespace Silverback.Messaging.Broker
{
    public interface IConsumer
    {
        event EventHandler<IMessage> Received;
        event EventHandler<ErrorHandlerEventArgs> Error;
    }
}