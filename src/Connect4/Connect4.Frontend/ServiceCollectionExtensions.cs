﻿using Connect4.Frontend.Chat;
using Connect4.Frontend.Components;
using Connect4.Frontend.Components.Pages;
using Connect4.Frontend.Game;
using Connect4.Frontend.Game.Games;
using Connect4.Frontend.Game.Lobbies;
using Connect4.Frontend.Shared;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using MudBlazor;
using MudBlazor.Services;

namespace Connect4.Frontend
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFrontend(this IServiceCollection services, IConfigurationSection azureAdConfiguration)
        {
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApp(azureAdConfiguration);
            services.AddRazorPages().AddMicrosoftIdentityUI();
            services.AddAuthorization(options =>
            {
                // By default, all incoming requests will be authorized according to the default policy
                options.FallbackPolicy = options.DefaultPolicy;
            });
            services.AddCascadingAuthenticationState();

            services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddMicrosoftIdentityConsentHandler();

            services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
                config.SnackbarConfiguration.VisibleStateDuration = 3500;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            });

            services.AddMediatR(config => config
                .RegisterServicesFromAssemblyContaining(typeof(ServiceCollectionExtensions)));

            services.AddSingleton<VisualizerChangedEventHandler>();
            services.AddSingleton<GameLobbiesChangedEventHandler>();
            services.AddSingleton<GameChangedEventHandler>();
            services.AddSingleton<PlayerClientChangedEventHandler>();
            services.AddSingleton<IChat, Chat.Chat>();
            services.AddSingleton<LoadingDelayer>();

            return services;
        }

        public static WebApplication UseFrontend(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseForwardedHeaders();

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAntiforgery();

            app.UseRewriter(
                new RewriteOptions().Add(
                    context =>
                    {
                        if (context.HttpContext.Request.Path == "/MicrosoftIdentity/Account/SignOut")
                        {
                            context.HttpContext.Response.Redirect("/");
                        }
                    }
                )
            );

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();
            app.MapControllers();

            return app;
        }
    }
}
