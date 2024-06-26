﻿using Game.Contract;
using Game.Contract.Commands;
using Game.Contract.Queries.Dtos;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Web;
using Visualizer.Contract;

namespace Connect4.Frontend.Game.Games
{
    public partial class Board
    {
        [Parameter] public required BoardDto BoardState { get; set; }

        [Parameter] public required PlayerUIClient CurrentPlayer { get; set; }

        [Parameter] public required GameId GameId { get; set; }

        [Parameter] public required VisualizerStatus? VisualizerStatus { get; set; }

        [Inject] private ISender Mediator { get; set; } = null!;

        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

        private bool IsBoardReadOnly { get; set; }

        private bool IsPiecePlacing { get; set; }

        private bool PlayPlaceSound { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await this.EvaluateBoardReadOnlyAsync();
            await base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            this.IsPiecePlacing = false;
            await this.EvaluateBoardReadOnlyAsync();
            await base.OnParametersSetAsync();
        }

        private async Task EvaluateBoardReadOnlyAsync()
        {
            var authenticationState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync();
            var isMyPlayer = this.CurrentPlayer.Player?.Owner.Identifier == authenticationState.User.GetNameIdentifierId();
            var isVisualizerReady = this.VisualizerStatus is { StatusType: StatusType.Ready };
            this.IsBoardReadOnly = !isMyPlayer || this.IsPiecePlacing || !isVisualizerReady;
        }

        private async Task CellClickedAsync(int xCoord)
        {
            this.IsPiecePlacing = true;
            var yCoord = 0;
            for (var row = 0; row < 6; row++)
            {
                if (this.BoardState.BoardState[0][xCoord].SlotState != SlotState.Empty)
                    return;

                if (this.BoardState.BoardState[row][xCoord].SlotState == SlotState.Empty)
                    yCoord = row;
                else
                    break;
            }

            var boardPosition = new BoardPosition(xCoord, yCoord);
            await this.Mediator.Send(new PlaceGamePieceCommand(this.GameId, boardPosition));
            _ = Task.Run(async () =>
            {
                this.PlayPlaceSound = true;
                await this.InvokeAsync(this.StateHasChanged);

                await Task.Delay(1200);

                this.PlayPlaceSound = false;
                await this.InvokeAsync(this.StateHasChanged);
            });
        }
    }
}