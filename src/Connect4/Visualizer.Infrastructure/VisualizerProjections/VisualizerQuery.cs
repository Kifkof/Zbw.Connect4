﻿using MongoDB.Driver;
using Visualizer.Contract;
using Visualizer.Domain.VisualizerProjections;

namespace Visualizer.Infrastructure.VisualizerProjections
{
    internal class VisualizerQuery(IVisualizerCollectionProvider collectionProvider) : IVisualizerQuery
    {
        public Task<VisualizerView> GetByIdAsync(VisualizerId id, CancellationToken cancellationToken = default)
        {
            var visualizerDetail = collectionProvider.VisualizerDetailCollection
                .AsQueryable()
                .First(v => v.VisualizerId == id.Id);

            return Task.FromResult(Map(visualizerDetail));
        }

        public Task<VisualizerView> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
        {
            var visualizerDetail = collectionProvider.VisualizerDetailCollection
                .AsQueryable()
                .Where(v => !v.IsDeleted)
                .First(v => v.ExternalId == externalId);

            return Task.FromResult(Map(visualizerDetail));
        }

        public Task<IReadOnlyList<VisualizerView>> GetAllVisualizersAsync(CancellationToken cancellationToken = default)
        {
            var visualizerDetails = collectionProvider.VisualizerDetailCollection
                .AsQueryable()
                .Where(v => !v.IsDeleted)
                .ToList();

            return Task.FromResult<IReadOnlyList<VisualizerView>>(visualizerDetails.Select(Map).ToList());
        }

        private static VisualizerView Map(VisualizerViewDbo visualizerView)
        {
            return new VisualizerView
            {
                Id = new VisualizerId(visualizerView.VisualizerId),
                Name = visualizerView.Name,
                ExternalId = visualizerView.ExternalId,
                Status = VisualizerStatus.GetById(visualizerView.StatusId)
            };
        }
    }
}