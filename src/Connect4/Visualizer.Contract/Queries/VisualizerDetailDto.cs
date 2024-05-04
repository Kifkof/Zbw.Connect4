﻿namespace Visualizer.Contract.Queries
{
    public class VisualizerDetailDto
    {
        public required VisualizerId Id { get; init; }
        public string? Name { get; init; }
        public string? ExternalId { get; init; }
        public required VisualizerStatus Status { get; init; }
    }
}