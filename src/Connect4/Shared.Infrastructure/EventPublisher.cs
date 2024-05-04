﻿using MediatR;
using Shared.Domain;

namespace Shared.Infrastructure
{
    internal class EventPublisher(IMediator mediator) : IEventPublisher
    {
        public async Task PublishEvents(IReadOnlyList<DomainEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                await mediator.Publish(domainEvent);
            }
        }
    }
}