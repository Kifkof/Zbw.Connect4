﻿using Shared.Domain;

namespace Visualizer.Domain.VisualizerAggregate
{
    public class Visualizer(VisualizerId id, IEventRegistry<VisualizerEvent> eventRegistry, TimeProvider timeProvider) : AggregateRoot<VisualizerEvent>(eventRegistry), IVisualizer
    {
        public Visualizer(
            VisualizerId id, 
            string? name, 
            string? externalId, 
            VisualizerStatus? status, 
            DateTimeOffset? deletedAt, 
            IEventRegistry<VisualizerEvent> eventRegistry, 
            TimeProvider timeProvider) 
            : this(id, eventRegistry, timeProvider)
        {
            this.Name = name;
            this.ExternalId = externalId;
            this.Status = status ?? VisualizerStatus.Unknown;
            this.DeletedAt = deletedAt;
        }

        public VisualizerId Id { get; } = id;

        public string? Name { get; private set; }

        public string? ExternalId { get; private set; }

        public VisualizerStatus Status { get; private set; } = VisualizerStatus.Unknown;

        public DateTimeOffset? DeletedAt { get; private set; }

        private bool IsDeleted => this.DeletedAt != null;

        public async Task<Guid> CreateVisualizer(string name, string externalId)
        {
            if (this.IsDeleted)
                throw new VisualizerDeletedException();

            await this.RaiseEventAsync(new VisualizerCreatedEvent
            {
                VisualizerId = this.Id
            });

            await this.ChangeNameAsync(name);
            await this.ChangeExternalIdAsync(externalId);
            await this.ChangeStatusAsync(VisualizerStatus.Idle);

            return this.Id.Id;
        }

        public async Task ChangeNameAsync(string name)
        {
            if (this.IsDeleted)
                throw new VisualizerDeletedException();

            await this.RaiseEventAsync(new VisualizerNameChangedEvent
            {
                VisualizerId = this.Id,
                Name = name
            });
        }

        public async Task ChangeStatusAsync(VisualizerStatus status)
        {
            if (this.Status == status)
                return;

            if (this.IsDeleted)
                throw new VisualizerDeletedException();

            await this.RaiseEventAsync(new VisualizerStatusChangedEvent
            {
                VisualizerId = this.Id,
                Status = status
            });
        }

        public async Task ChangeExternalIdAsync(string externalId)
        {
            if (this.IsDeleted)
                throw new VisualizerDeletedException();

            await this.RaiseEventAsync(new VisualizerExternalIdChangedEvent()
            {
                VisualizerId = this.Id,
                ExternalId = externalId
            });
        }

        public async Task DeleteAsync()
        {
            if (this.IsDeleted)
                throw new VisualizerDeletedException();

            await this.RaiseEventAsync(new VisualizerDeletedEvent()
            {
                VisualizerId = this.Id,
                DeletedAt = timeProvider.GetUtcNow(),
            });
        }

        public override void Apply(VisualizerEvent @event)
        {
            switch (@event)
            {
                case VisualizerCreatedEvent:
                    break;
                case VisualizerDeletedEvent visualizerDeletedEvent:
                    this.Apply(visualizerDeletedEvent);
                    break;
                case VisualizerExternalIdChangedEvent visualizerExternalIdChangedEvent:
                    this.Apply(visualizerExternalIdChangedEvent);
                    break;
                case VisualizerNameChangedEvent visualizerNameChangedEvent:
                    this.Apply(visualizerNameChangedEvent);
                    break;
                case VisualizerStatusChangedEvent visualizerStatusChangedEvent:
                    this.Apply(visualizerStatusChangedEvent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@event));
            }
        }

        public void Apply(VisualizerDeletedEvent @event)
        {
            this.DeletedAt = @event.DeletedAt;
        }

        public void Apply(VisualizerExternalIdChangedEvent @event)
        {
            this.ExternalId = @event.ExternalId;
        }

        public void Apply(VisualizerNameChangedEvent @event)
        {
            this.Name = @event.Name;
        }

        public void Apply(VisualizerStatusChangedEvent @event)
        {
            this.Status = @event.Status;
        }
    }
}