﻿using MediatR;
using Shared.Contract;

namespace Shared.Infrastructure
{
    internal class EventPublisher(IMediator mediator) : IEventPublisher
    {
        public async Task PublishEvents(IReadOnlyList<INotification> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                await mediator.Publish(domainEvent).ConfigureAwait(false);
            }
        }
    }
}
