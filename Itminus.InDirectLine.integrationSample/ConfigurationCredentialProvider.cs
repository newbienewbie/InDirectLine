// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EchoBot .NET Template version v0.4.6

using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;

namespace Itminus.InDirectLine.IntegrationSample
{
    public class ConfigurationCredentialProvider : SimpleCredentialProvider
    {
        public ConfigurationCredentialProvider(IConfiguration configuration)
            : base(configuration["MicrosoftAppId"], configuration["MicrosoftAppPassword"])
        {
        }
    }
}
