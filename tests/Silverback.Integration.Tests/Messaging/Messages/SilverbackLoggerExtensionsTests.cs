﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Silverback.Messaging.Messages;
using Silverback.Tests.Integration.TestTypes;
using Xunit;

namespace Silverback.Tests.Integration.Messaging.Messages
{
    public class SilverbackLoggerExtensionsTests
    {
        private static readonly IRawInboundEnvelope InboundEnvelope = new RawInboundEnvelope(
            Array.Empty<byte>(),
            new MessageHeaderCollection
            {
                new MessageHeader(DefaultMessageHeaders.FailedAttempts, 1),
                new MessageHeader(DefaultMessageHeaders.MessageType, "Something.Xy"),
                new MessageHeader(DefaultMessageHeaders.MessageId, "1234")
            },
            new TestConsumerEndpoint("Test"),
            "TestActual",
            null,
            new Dictionary<string, string>
            {
                ["offset-in"] = "9"
            });

        private static readonly IRawInboundEnvelope[] InboundBatch =
        {
            new RawInboundEnvelope(
                Array.Empty<byte>(),
                new MessageHeaderCollection
                {
                    new MessageHeader(DefaultMessageHeaders.FailedAttempts, 1),
                    new MessageHeader(DefaultMessageHeaders.MessageType, "Something.Xy"),
                    new MessageHeader(DefaultMessageHeaders.MessageId, "1234"),
                    new MessageHeader(DefaultMessageHeaders.BatchId, "3"),
                    new MessageHeader(DefaultMessageHeaders.BatchSize, "10")
                },
                new TestConsumerEndpoint("Test"),
                "TestActual",
                null,
                new Dictionary<string, string>
                {
                    ["offset-in"] = "9"
                }),
            new RawInboundEnvelope(
                Array.Empty<byte>(),
                new MessageHeaderCollection
                {
                    new MessageHeader(DefaultMessageHeaders.FailedAttempts, 1),
                    new MessageHeader(DefaultMessageHeaders.MessageType, "Something.Xy"),
                    new MessageHeader(DefaultMessageHeaders.MessageId, "5678"),
                    new MessageHeader(DefaultMessageHeaders.BatchId, "3"),
                    new MessageHeader(DefaultMessageHeaders.BatchSize, "10")
                },
                new TestConsumerEndpoint("Test"),
                "TestActual",
                null,
                new Dictionary<string, string>
                {
                    ["offset-in"] = "10"
                })
        };

        private static readonly IRawOutboundEnvelope OutboundEnvelope = new RawOutboundEnvelope(
            Array.Empty<byte>(),
            new MessageHeaderCollection
            {
                new MessageHeader(DefaultMessageHeaders.MessageType, "Something.Xy"),
                new MessageHeader(DefaultMessageHeaders.MessageId, "1234")
            },
            new TestProducerEndpoint("Test"),
            null,
            new Dictionary<string, string>
            {
                ["offset-out"] = "9"
            });

        private static readonly IRawOutboundEnvelope[] OutboundBatch =
        {
            new RawOutboundEnvelope(
                Array.Empty<byte>(),
                new MessageHeaderCollection
                {
                    new MessageHeader(DefaultMessageHeaders.MessageType, "Something.Xy"),
                    new MessageHeader(DefaultMessageHeaders.MessageId, "1234")
                },
                new TestProducerEndpoint("Test"),
                null,
                new Dictionary<string, string>
                {
                    ["offset-out"] = "9"
                }),
            new RawOutboundEnvelope(
                Array.Empty<byte>(),
                new MessageHeaderCollection
                {
                    new MessageHeader(DefaultMessageHeaders.MessageType, "Something.Xy"),
                    new MessageHeader(DefaultMessageHeaders.MessageId, "5678")
                },
                new TestProducerEndpoint("Test"),
                null,
                new Dictionary<string, string>
                {
                    ["offset-out"] = "10"
                })
        };

        private readonly LoggerSubstitute<SilverbackLoggerExtensionsTests>
            _logger = new LoggerSubstitute<SilverbackLoggerExtensionsTests>();

        public SilverbackLoggerExtensionsTests()
        {
            LogTemplates.ConfigureAdditionalData<TestConsumerEndpoint>("offset-in");
            LogTemplates.ConfigureAdditionalData<TestProducerEndpoint>("offset-out");
        }

        [Fact]
        public void LogProcessing_SingleInboundEnvelope_InformationLogged()
        {
            _logger.LogProcessing(new[] { InboundEnvelope });

            const string expectedMessage =
                "Processing inbound message. | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "messageType: Something.Xy, " +
                "messageId: 1234, " +
                "offset-in: 9";

            _logger.Received(LogLevel.Information, null, expectedMessage);
        }

        [Fact]
        public void LogProcessing_MultipleInboundEnvelopes_InformationLogged()
        {
            _logger.LogProcessing(InboundBatch);

            const string expectedMessage =
                "Processing the batch of 2 inbound messages. | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "batchId: 3, " +
                "batchSize: 10";

            _logger.Received(LogLevel.Information, null, expectedMessage);
        }

        [Fact]
        public void LogProcessingError_SingleInboundEnvelope_WarningLogged()
        {
            _logger.LogProcessingError(new[] { InboundEnvelope }, new InvalidOperationException());

            const string expectedMessage =
                "Error occurred processing the inbound message. | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "messageType: Something.Xy, " +
                "messageId: 1234, " +
                "offset-in: 9";

            _logger.Received(LogLevel.Warning, typeof(InvalidOperationException), expectedMessage);
        }

        [Fact]
        public void LogProcessingError_MultipleInboundEnvelopes_WarningLogged()
        {
            _logger.LogProcessingError(InboundBatch, new InvalidOperationException());

            const string expectedMessage =
                "Error occurred processing the batch of 2 inbound messages. | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "batchId: 3, " +
                "batchSize: 10";

            _logger.Received(LogLevel.Warning, typeof(InvalidOperationException), expectedMessage);
        }

        [Fact]
        public void LogTraceWithMessageInfo_InboundEnvelope_TraceLogged()
        {
            _logger.LogTraceWithMessageInfo(
                new EventId(42, "test"),
                "Log message",
                InboundEnvelope);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "messageType: Something.Xy, " +
                "messageId: 1234, " +
                "offset-in: 9";

            _logger.Received(LogLevel.Trace, null, expectedMessage);
        }

        [Fact]
        public void LogTraceWithMessageInfo_InboundEnvelopesCollection_TraceLogged()
        {
            _logger.LogTraceWithMessageInfo(
                new EventId(42, "test"),
                "Log message",
                InboundBatch);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "batchId: 3, " +
                "batchSize: 10";

            _logger.Received(LogLevel.Trace, null, expectedMessage);
        }

        [Fact]
        public void LogDebugWithMessageInfo_InboundEnvelope_DebugLogged()
        {
            _logger.LogDebugWithMessageInfo(
                new EventId(42, "test"),
                "Log message",
                InboundEnvelope);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "messageType: Something.Xy, " +
                "messageId: 1234, " +
                "offset-in: 9";

            _logger.Received(LogLevel.Debug, null, expectedMessage);
        }

        [Fact]
        public void LogDebugWithMessageInfo_InboundEnvelopesCollection_DebugLogged()
        {
            _logger.LogDebugWithMessageInfo(
                new EventId(42, "test"),
                "Log message",
                InboundBatch);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "batchId: 3, " +
                "batchSize: 10";

            _logger.Received(LogLevel.Debug, null, expectedMessage);
        }

        [Fact]
        public void LogInformationWithMessageInfo_InboundEnvelope_InformationLogged()
        {
            _logger.LogInformationWithMessageInfo(
                new EventId(42, "test"),
                "Log message",
                InboundEnvelope);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "messageType: Something.Xy, " +
                "messageId: 1234, " +
                "offset-in: 9";

            _logger.Received(LogLevel.Information, null, expectedMessage);
        }

        [Fact]
        public void LogInformationWithMessageInfo_InboundEnvelopesCollection_InformationLogged()
        {
            _logger.LogInformationWithMessageInfo(
                new EventId(42, "test"),
                "Log message",
                InboundBatch);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "batchId: 3, " +
                "batchSize: 10";

            _logger.Received(LogLevel.Information, null, expectedMessage);
        }

        [Fact]
        public void LogWarningWithMessageInfo_InboundEnvelope_WarningLogged()
        {
            _logger.LogWarningWithMessageInfo(
                new EventId(42, "test"),
                "Log message",
                InboundEnvelope);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "messageType: Something.Xy, " +
                "messageId: 1234, " +
                "offset-in: 9";

            _logger.Received(LogLevel.Warning, null, expectedMessage);
        }

        [Fact]
        public void LogWarningWithMessageInfo_InboundEnvelopeAndException_WarningLogged()
        {
            _logger.LogWarningWithMessageInfo(
                new EventId(42, "test"),
                new InvalidOperationException(),
                "Log message",
                InboundEnvelope);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "messageType: Something.Xy, " +
                "messageId: 1234, " +
                "offset-in: 9";

            _logger.Received(LogLevel.Warning, typeof(InvalidOperationException), expectedMessage);
        }

        [Fact]
        public void LogWarningWithMessageInfo_InboundEnvelopesCollection_WarningLogged()
        {
            _logger.LogWarningWithMessageInfo(
                new EventId(42, "test"),
                "Log message",
                InboundBatch);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "batchId: 3, " +
                "batchSize: 10";

            _logger.Received(LogLevel.Warning, null, expectedMessage);
        }

        [Fact]
        public void LogWarningWithMessageInfo_InboundEnvelopesCollectionAndException_WarningLogged()
        {
            _logger.LogWarningWithMessageInfo(
                new EventId(42, "test"),
                new InvalidOperationException(),
                "Log message",
                InboundBatch);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "batchId: 3, " +
                "batchSize: 10";

            _logger.Received(LogLevel.Warning, typeof(InvalidOperationException), expectedMessage);
        }

        [Fact]
        public void LogErrorWithMessageInfo_InboundEnvelope_ErrorLogged()
        {
            _logger.LogErrorWithMessageInfo(
                new EventId(42, "test"),
                "Log message",
                InboundEnvelope);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "messageType: Something.Xy, " +
                "messageId: 1234, " +
                "offset-in: 9";

            _logger.Received(LogLevel.Error, null, expectedMessage);
        }

        [Fact]
        public void LogErrorWithMessageInfo_InboundEnvelopeAndException_ErrorLogged()
        {
            _logger.LogErrorWithMessageInfo(
                new EventId(42, "test"),
                new InvalidOperationException(),
                "Log message",
                InboundEnvelope);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "messageType: Something.Xy, " +
                "messageId: 1234, " +
                "offset-in: 9";

            _logger.Received(LogLevel.Error, typeof(InvalidOperationException), expectedMessage);
        }

        [Fact]
        public void LogErrorWithMessageInfo_InboundEnvelopesCollection_ErrorLogged()
        {
            _logger.LogErrorWithMessageInfo(
                new EventId(42, "test"),
                "Log message",
                InboundBatch);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "batchId: 3, " +
                "batchSize: 10";

            _logger.Received(LogLevel.Error, null, expectedMessage);
        }

        [Fact]
        public void LogErrorWithMessageInfo_InboundEnvelopesCollectionAndException_ErrorLogged()
        {
            _logger.LogErrorWithMessageInfo(
                new EventId(42, "test"),
                new InvalidOperationException(),
                "Log message",
                InboundBatch);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "batchId: 3, " +
                "batchSize: 10";

            _logger.Received(LogLevel.Error, typeof(InvalidOperationException), expectedMessage);
        }

        [Fact]
        public void LogCriticalWithMessageInfo_InboundEnvelope_CriticalLogged()
        {
            _logger.LogCriticalWithMessageInfo(
                new EventId(42, "test"),
                "Log message",
                InboundEnvelope);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "messageType: Something.Xy, " +
                "messageId: 1234, " +
                "offset-in: 9";

            _logger.Received(LogLevel.Critical, null, expectedMessage);
        }

        [Fact]
        public void LogCriticalWithMessageInfo_InboundEnvelopeAndException_CriticalLogged()
        {
            _logger.LogCriticalWithMessageInfo(
                new EventId(42, "test"),
                new InvalidOperationException(),
                "Log message",
                InboundEnvelope);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "messageType: Something.Xy, " +
                "messageId: 1234, " +
                "offset-in: 9";

            _logger.Received(LogLevel.Critical, typeof(InvalidOperationException), expectedMessage);
        }

        [Fact]
        public void LogCriticalWithMessageInfo_InboundEnvelopesCollection_CriticalLogged()
        {
            _logger.LogCriticalWithMessageInfo(
                new EventId(42, "test"),
                "Log message",
                InboundBatch);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "batchId: 3, " +
                "batchSize: 10";

            _logger.Received(LogLevel.Critical, null, expectedMessage);
        }

        [Fact]
        public void LogCriticalWithMessageInfo_InboundEnvelopesCollectionAndException_CriticalLogged()
        {
            _logger.LogCriticalWithMessageInfo(
                new EventId(42, "test"),
                new InvalidOperationException(),
                "Log message",
                InboundBatch);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "batchId: 3, " +
                "batchSize: 10";

            _logger.Received(LogLevel.Critical, typeof(InvalidOperationException), expectedMessage);
        }

        [Fact]
        public void LogCriticalWithMessageInfo_OutboundEnvelope_CriticalLogged()
        {
            _logger.LogCriticalWithMessageInfo(
                new EventId(42, "test"),
                "Log message",
                OutboundEnvelope);

            const string expectedMessage =
                "Log message | " +
                "endpointName: Test, " +
                "messageType: Something.Xy, " +
                "messageId: 1234, " +
                "offset-out: 9";

            _logger.Received(LogLevel.Critical, null, expectedMessage);
        }

        [Fact]
        public void LogCriticalWithMessageInfo_OutboundEnvelopeAndException_CriticalLogged()
        {
            _logger.LogCriticalWithMessageInfo(
                new EventId(42, "test"),
                new InvalidOperationException(),
                "Log message",
                OutboundEnvelope);

            const string expectedMessage =
                "Log message | " +
                "endpointName: Test, " +
                "messageType: Something.Xy, " +
                "messageId: 1234, " +
                "offset-out: 9";

            _logger.Received(LogLevel.Critical, typeof(InvalidOperationException), expectedMessage);
        }

        [Fact]
        public void LogCriticalWithMessageInfo_OutboundEnvelopesCollection_CriticalLogged()
        {
            _logger.LogCriticalWithMessageInfo(
                new EventId(42, "test"),
                "Log message",
                OutboundBatch);

            const string expectedMessage =
                "Log message | " +
                "endpointName: Test";

            _logger.Received(LogLevel.Critical, null, expectedMessage);
        }

        [Fact]
        public void LogCriticalWithMessageInfo_OutboundEnvelopesCollectionAndException_CriticalLogged()
        {
            _logger.LogCriticalWithMessageInfo(
                new EventId(42, "test"),
                new InvalidOperationException(),
                "Log message",
                OutboundBatch);

            const string expectedMessage =
                "Log message | " +
                "endpointName: Test";

            _logger.Received(LogLevel.Critical, typeof(InvalidOperationException), expectedMessage);
        }

        [Fact]
        public void LogWithMessageInfo_SingleInboundEnvelopeWithWarningLevel_WarningLogged()
        {
            _logger.LogWithMessageInfo(
                LogLevel.Warning,
                new EventId(42, "test"),
                null,
                "Log message",
                new[] { InboundEnvelope });

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "messageType: Something.Xy, " +
                "messageId: 1234, " +
                "offset-in: 9";

            _logger.Received(LogLevel.Warning, null, expectedMessage);
        }

        [Fact]
        public void LogWithMessageInfo_SingleInboundEnvelopeAndExceptionWithWarningLevel_WarningLogged()
        {
            _logger.LogWithMessageInfo(
                LogLevel.Warning,
                new EventId(42, "test"),
                new InvalidOperationException(),
                "Log message",
                new[] { InboundEnvelope });

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "messageType: Something.Xy, " +
                "messageId: 1234, " +
                "offset-in: 9";

            _logger.Received(LogLevel.Warning, typeof(InvalidOperationException), expectedMessage);
        }

        [Fact]
        public void LogWithMessageInfo_MultipleInboundEnvelopesWithWarningLevel_WarningLogged()
        {
            _logger.LogWithMessageInfo(
                LogLevel.Warning,
                new EventId(42, "test"),
                null,
                "Log message",
                InboundBatch);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "batchId: 3, " +
                "batchSize: 10";

            _logger.Received(LogLevel.Warning, null, expectedMessage);
        }

        [Fact]
        public void LogWithMessageInfo_MultipleInboundEnvelopesAndExceptionWithWarningLevel_WarningLogged()
        {
            _logger.LogWithMessageInfo(
                LogLevel.Warning,
                new EventId(42, "test"),
                new InvalidOperationException(),
                "Log message",
                InboundBatch);

            const string expectedMessage =
                "Log message | " +
                "endpointName: TestActual, " +
                "failedAttempts: 1, " +
                "batchId: 3, " +
                "batchSize: 10";

            _logger.Received(LogLevel.Warning, typeof(InvalidOperationException), expectedMessage);
        }

        [Fact]
        public void LogWithMessageInfo_SingleOutboundEnvelopeWithWarningLevel_WarningLogged()
        {
            _logger.LogWithMessageInfo(
                LogLevel.Warning,
                new EventId(42, "test"),
                null,
                "Log message",
                new[] { OutboundEnvelope });

            const string expectedMessage =
                "Log message | " +
                "endpointName: Test, " +
                "messageType: Something.Xy, " +
                "messageId: 1234, " +
                "offset-out: 9";

            _logger.Received(LogLevel.Warning, null, expectedMessage);
        }

        [Fact]
        public void LogWithMessageInfo_SingleOutboundEnvelopeAndExceptionWithWarningLevel_WarningLogged()
        {
            _logger.LogWithMessageInfo(
                LogLevel.Warning,
                new EventId(42, "test"),
                new InvalidOperationException(),
                "Log message",
                new[] { OutboundEnvelope });

            const string expectedMessage =
                "Log message | " +
                "endpointName: Test, " +
                "messageType: Something.Xy, " +
                "messageId: 1234, " +
                "offset-out: 9";

            _logger.Received(LogLevel.Warning, typeof(InvalidOperationException), expectedMessage);
        }

        [Fact]
        public void LogWithMessageInfo_MultipleOutboundEnvelopesWithWarningLevel_WarningLogged()
        {
            _logger.LogWithMessageInfo(
                LogLevel.Warning,
                new EventId(42, "test"),
                null,
                "Log message",
                OutboundBatch);

            const string expectedMessage =
                "Log message | " +
                "endpointName: Test";

            _logger.Received(LogLevel.Warning, null, expectedMessage);
        }

        [Fact]
        public void LogWithMessageInfo_MultipleOutboundEnvelopesAndExceptionWithWarningLevel_WarningLogged()
        {
            _logger.LogWithMessageInfo(
                LogLevel.Warning,
                new EventId(42, "test"),
                new InvalidOperationException(),
                "Log message",
                OutboundBatch);

            const string expectedMessage =
                "Log message | " +
                "endpointName: Test";

            _logger.Received(LogLevel.Warning, typeof(InvalidOperationException), expectedMessage);
        }
    }
}