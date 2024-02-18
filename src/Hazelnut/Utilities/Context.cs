﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hazelnut
{
    public partial class Context
    {
        static Context current = new();
        internal static Func<Context> ContextProvider = () => new Context();

        IServiceProvider ApplicationServices;
        Func<IServiceProvider> ScopeServiceProvider;
        public IServiceProvider ServiceProvider => ScopeServiceProvider?.Invoke() ?? ApplicationServices;

        /// <summary>
        /// Occurs when the StartUp.OnInitializedAsync is completed.
        /// </summary>
        public static event AwaitableEventHandler StartedUp;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Task OnStartedUp() => StartedUp.Raise();

        public static Context Current => current ?? throw new InvalidOperationException("Hazelnut.Context is not initialized!");

        public static Context Initialize(IServiceProvider applicationServices, Func<IServiceProvider> scopeServiceProvider)
        {
            return current = new()
            {
                ApplicationServices = applicationServices,
                ScopeServiceProvider = scopeServiceProvider
            };
        }

        /// <summary>
        /// Gets a required service of the specified contract type.
        /// </summary>
        public TService GetService<TService>() => ServiceProvider.GetRequiredService<TService>();

        public IConfiguration Config => GetService<IConfiguration>();

        public TService GetOptionalService<TService>() where TService : class
        {
            var result = ServiceProvider.GetService<TService>();

            if (result == null)
                Debug.WriteLine(typeof(TService).FullName + " service is not configured.");

            return result;
        }

        public IEnumerable<TService> GetServices<TService>() => ServiceProvider.GetServices<TService>();
    }
}