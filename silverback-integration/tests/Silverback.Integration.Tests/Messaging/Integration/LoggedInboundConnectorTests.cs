﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Silverback.Messaging;
using Silverback.Messaging.Connectors;
using Silverback.Messaging.Connectors.Repositories;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Publishing;
using Silverback.Messaging.Serialization;
using Silverback.Messaging.Subscribers;
using Silverback.Tests.TestTypes;
using Silverback.Tests.TestTypes.Domain;

namespace Silverback.Tests.Messaging.Integration
{
    [TestFixture]
    public class LoggedInboundConnectorTests
    {
        private IInboundLog _inboundLog;
        private IServiceCollection _services;
        private TestSubscriber _testSubscriber;
        private IInboundConnector _connector;
        private TestBroker _broker;

        [SetUp]
        public void Setup()
        {
            _services = new ServiceCollection();

            _testSubscriber = new TestSubscriber();
            _services.AddSingleton<ISubscriber>(_testSubscriber);

            _services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
            _services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
            _services.AddSingleton<IPublisher, Publisher>();

            _broker = new TestBroker(new JsonMessageSerializer());
            _inboundLog = new InMemoryInboundLog();

            _connector = new LoggedInboundConnector(_broker, _services.BuildServiceProvider(), _inboundLog, new NullLogger<LoggedInboundConnector>());
        }

        [Test]
        public void Bind_PushMessages_MessagesReceived()
        {
            _connector.Bind(TestEndpoint.Default);
            _broker.Connect();

            var consumer = (TestConsumer)_broker.GetConsumer(TestEndpoint.Default);
            consumer.TestPush(new TestEventOne { Id = Guid.NewGuid() });
            consumer.TestPush(new TestEventTwo { Id = Guid.NewGuid() });

            Assert.That(_testSubscriber.ReceivedMessages.Count, Is.EqualTo(2));
        }

        [Test]
        public void Bind_PushMessages_EachIsConsumedOnce()
        {
            var e1 = new TestEventOne { Content = "Test", Id = Guid.NewGuid() };
            var e2 = new TestEventTwo { Content = "Test", Id = Guid.NewGuid() };

            _connector.Bind(TestEndpoint.Default);
            _broker.Connect();

            var consumer = (TestConsumer)_broker.GetConsumer(TestEndpoint.Default);
            consumer.TestPush(e1);
            consumer.TestPush(e2);
            consumer.TestPush(e1);
            consumer.TestPush(e2);
            consumer.TestPush(e1);

            Assert.That(_testSubscriber.ReceivedMessages.Count, Is.EqualTo(2));
        }

        [Test]
        public void AddToBind_PushMessages_WrittenToLog()
        {
            var e1 = new TestEventOne { Content = "Test", Id = Guid.NewGuid() };
            var e2 = new TestEventTwo { Content = "Test", Id = Guid.NewGuid() };

            _connector.Bind(TestEndpoint.Default);
            _broker.Connect();

            var consumer = (TestConsumer)_broker.GetConsumer(TestEndpoint.Default);
            consumer.TestPush(e1);
            consumer.TestPush(e2);
            consumer.TestPush(e1);
            consumer.TestPush(e2);
            consumer.TestPush(e1);


            Assert.That(_inboundLog.Length, Is.EqualTo(2));
        }
    }
}