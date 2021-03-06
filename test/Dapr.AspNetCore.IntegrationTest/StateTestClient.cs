// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Dapr.Client
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapr.Client;
    using Grpc.Net.Client;
    using Autogenerated = Dapr.Client.Autogen.Grpc.v1;

    internal class StateTestClient : DaprClientGrpc
    {
        public Dictionary<string, object> State { get; } = new Dictionary<string, object>();
        private static readonly GrpcChannel channel = GrpcChannel.ForAddress("http://localhost");

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprClientGrpc"/> class.
        /// </summary>
        internal StateTestClient() 
            : base(channel, new Autogenerated.Dapr.DaprClient(channel), new HttpClient(), new Uri("http://localhost"), null, default)
        {
        }

        public override Task<TValue> GetStateAsync<TValue>(string storeName, string key, ConsistencyMode? consistencyMode = default, IReadOnlyDictionary<string, string> metadata = default, CancellationToken cancellationToken = default)
        {
            ArgumentVerifier.ThrowIfNullOrEmpty(storeName, nameof(storeName));
            ArgumentVerifier.ThrowIfNullOrEmpty(key, nameof(key));

            if (this.State.TryGetValue(key, out var obj))
            {
                return Task.FromResult((TValue)obj);
            }
            else
            {
                return Task.FromResult(default(TValue));
            }
        }

        public override Task<IReadOnlyList<BulkStateItem>> GetBulkStateAsync(string storeName, IReadOnlyList<string> keys, int? parallelism, IReadOnlyDictionary<string, string> metadata = default, CancellationToken cancellationToken = default)
        {
            ArgumentVerifier.ThrowIfNullOrEmpty(storeName, nameof(storeName));

            var response = new List<BulkStateItem>();

            foreach (var key in keys)
            {
                if (this.State.TryGetValue(key, out var obj))
                {
                    response.Add(new BulkStateItem(key, obj.ToString(), ""));
                }
                else
                {
                    response.Add(new BulkStateItem(key, "", ""));
                }
            }

            return Task.FromResult<IReadOnlyList<BulkStateItem>>(response);
        }

        public override Task<(TValue value, string etag)> GetStateAndETagAsync<TValue>(
            string storeName,
            string key,
            ConsistencyMode? consistencyMode = default,
            IReadOnlyDictionary<string, string> metadata = default,
            CancellationToken cancellationToken = default)
        {
            ArgumentVerifier.ThrowIfNullOrEmpty(storeName, nameof(storeName));
            ArgumentVerifier.ThrowIfNullOrEmpty(key, nameof(key));

            if (this.State.TryGetValue(key, out var obj))
            {
                return Task.FromResult(((TValue)obj, "test_etag"));
            }
            else
            {
                return Task.FromResult((default(TValue), "test_etag"));
            }
        }

        public override Task SaveStateAsync<TValue>(
            string storeName,
            string key,
            TValue value,
            StateOptions stateOptions = default,
            IReadOnlyDictionary<string, string> metadata = default,
            CancellationToken cancellationToken = default)
        {
            ArgumentVerifier.ThrowIfNullOrEmpty(storeName, nameof(storeName));
            ArgumentVerifier.ThrowIfNullOrEmpty(key, nameof(key));

            this.State[key] = value;
            return Task.CompletedTask;
        }

        public override Task DeleteStateAsync(
           string storeName,
           string key,
           StateOptions stateOptions = default,
           IReadOnlyDictionary<string, string> metadata = default,
           CancellationToken cancellationToken = default)
        {
            ArgumentVerifier.ThrowIfNullOrEmpty(storeName, nameof(storeName));
            ArgumentVerifier.ThrowIfNullOrEmpty(key, nameof(key));

            this.State.Remove(key);
            return Task.CompletedTask;
        }
    }
}
