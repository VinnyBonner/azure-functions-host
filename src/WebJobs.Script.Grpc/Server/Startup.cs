﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.WebJobs.Script.Grpc.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Azure.WebJobs.Script.Grpc
{
    internal class Startup
    {
        private const int MaxMessageLengthBytes = int.MaxValue;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ExtensionsCompositeEndpointDataSource>();
            services.AddSingleton<ExtensionsCompositeEndpointDataSource.EnsureInitializedMiddleware>();
            services.AddGrpc(options =>
            {
                options.MaxReceiveMessageSize = MaxMessageLengthBytes;
                options.MaxSendMessageSize = MaxMessageLengthBytes;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // This must occur before 'UseRouting'. This ensures extension endpoints are registered before the
            // endpoints are collected by the routing middleware.
            app.UseMiddleware<ExtensionsCompositeEndpointDataSource.EnsureInitializedMiddleware>();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<FunctionRpc.FunctionRpcBase>();
                endpoints.DataSources.Add(
                    endpoints.ServiceProvider.GetRequiredService<ExtensionsCompositeEndpointDataSource>());
            });
        }
    }
}