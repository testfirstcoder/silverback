﻿// Copyright (c) 2018-2019 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using Silverback.Messaging.Messages;

namespace Silverback.Tests.TestTypes.Domain
{
    public interface IIntegrationMessage : IMessage
    {
        Guid Id { get; set; }
    }
}