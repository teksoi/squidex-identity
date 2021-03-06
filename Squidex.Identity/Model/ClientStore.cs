﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Squidex.ClientLibrary;

namespace Squidex.Identity.Model
{
    public sealed class ClientStore : IClientStore
    {
        private readonly SquidexClientManagerFactory factory;

        public ClientStore(SquidexClientManagerFactory factory)
        {
            this.factory = factory;
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            var clientManager = factory.GetClientManager();

            var apiClient = factory.GetClientManager().CreateContentsClient<ClientEntity, ClientData>("clients");

            var query = new ContentQuery
            {
                Filter = $"data/clientId/iv eq '{clientId}'"
            };

            var clients = await apiClient.GetAsync(query, context: Context.Build());

            var client = clients.Items.FirstOrDefault();

            if (client == null)
            {
                return null;
            }

            var scopes = new HashSet<string>(client.Data.AllowedScopes.OrDefault())
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                IdentityServerConstants.StandardScopes.Email,
                DefaultResources.Permissions.Scope
            };

            return new Client
            {
                AllowAccessTokensViaBrowser = true,
                AllowedCorsOrigins = client.Data.AllowedCorsOrigins.OrDefault(),
                AllowedGrantTypes = client.Data.AllowedGrantTypes.OrDefault(),
                AllowedScopes = scopes,
                AllowOfflineAccess = client.Data.AllowOfflineAccess,
                ClientId = clientId,
                ClientName = client.Data.ClientName,
                ClientSecrets = client.Data.ClientSecrets.ToSecrets(),
                ClientUri = client.Data.ClientUri,
                LogoUri = clientManager.GenerateImageUrl(client.Data.Logo),
                PostLogoutRedirectUris = client.Data.PostLogoutRedirectUris.OrDefault(),
                RedirectUris = client.Data.RedirectUris.OrDefault(),
                RequireConsent = client.Data.RequireConsent
            };
        }
    }
}
