﻿using Game.Contract.Commands;
using Game.Contract.Events;
using MediatR;
using PlayerClient.Contract;
using PlayerClient.Contract.Queries;
using PlayerClient.Domain;

namespace PlayerClient.Application
{
    internal class GamePiecePlacementRequestedPolicy(IMediator mediator, IPlayerAssignmentQuery playerAssignmentQuery) : INotificationHandler<GamePiecePlacementRequestedEventDto>
    {
        public async Task Handle(GamePiecePlacementRequestedEventDto notification, CancellationToken cancellationToken)
        {
            var players = await playerAssignmentQuery.GetPlayersByGameAsync(notification.GameId);

            var playerToAcknowledgeRequest = players.First(p => p.PlayerId != notification.RequestingPlayer.Id);
            var playerClient = await mediator.Send(
                    new PlayerClientByPlayerQuery
                    {
                        PlayerId = playerToAcknowledgeRequest.PlayerId,
                        PlayerClientType = playerToAcknowledgeRequest.PlayerType
                    }, cancellationToken);

            if (playerClient == null)
            {
                await mediator.Send(new NotAcknowledgeGamePiecePlacementCommand(notification.GameId, notification.RequestingPlayer.Id), cancellationToken);
                return;
            }

            await playerClient.RequestGamePiecePlacementAcknowledgement(notification.Position);
        }
    }
}
