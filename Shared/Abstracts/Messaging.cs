// Copyright (c) 2021 Axis Group, LLC. All Rights Reserved. Please see the included LICENSE file for license details or contact Axis Group for license information.

using System;
using MediatR;

namespace Shared
{
    // Basic interfaces for commands and events that support MediatR
    public interface IMessage : INotification { Guid MessageId { get; } DateTime UtcTime { get; } }
    public interface ICommand : IMessage { }
    public interface IEvent : IMessage { };
    public interface IEvent<TCommand> : IEvent where TCommand : ICommand { TCommand Command { get; } }


    // Abtract types for integration commands, events, and failures

    public abstract record Command(Guid MessageId, DateTime UtcTime, int Retries)
        : ICommand;

    public abstract record Event<TCommand>(Guid MessageId, DateTime UtcTime, TCommand Command)
        : IEvent<TCommand>
        where TCommand : ICommand;

    public abstract record Event<TCommand, TObject>(Guid MessageId, DateTime UtcTime, TCommand Command, TObject Result)
        : Event<TCommand>(MessageId, UtcTime, Command)
        where TCommand : ICommand;

    public abstract record FailureEvent<TCommand, TError>(Guid MessageId, DateTime UtcTime, TCommand Command, TError Error)
        : Event<TCommand>(MessageId, UtcTime, Command)
        where TCommand : ICommand;

}

