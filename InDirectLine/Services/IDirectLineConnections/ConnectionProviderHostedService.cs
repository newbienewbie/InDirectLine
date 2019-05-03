using Itminus.InDirectLine.InDirectLine.Services.IDirectLineConnections;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Itminus.InDirectLine.InDirectLine.Services.IDirectLineConnections
{
    public class DirectLineConnectionHostedService : IDirectLineConnectionsProvider, IHostedService, IDisposable
    {
        public DirectLineConnectionHostedService(IDirectLineConnectionManager directLineConnectionManager)
        {
            this._connections = new ConcurrentDictionary<string, ConnectionInfo>();
            this.timer = new System.Timers.Timer(CleanInterval);
            this._connectionManager = directLineConnectionManager;
        }

        private readonly IDirectLineConnectionManager _connectionManager;
        private ConcurrentDictionary<string, ConnectionInfo> _connections;
        private int CleanInterval { get; } = 2000; // 2000ms;
        public static readonly int Expires=3600;   // 3600s 
        private System.Timers.Timer timer;

        public Task<IDirectLineConnection> GetConnectionAsync(string conversationId)
        {
            var exists = _connections.TryGetValue(conversationId, out var connection);
            if (!exists || !connection.Valid )
                return Task.FromResult<IDirectLineConnection>(null); 
            return Task.FromResult(connection?.Connection);
        }

        public Task<bool> RegisterConnectionAsync(string conversationId, IDirectLineConnection connection)
        {
            var exists = _connections.TryGetValue(conversationId, out var exConnection);
            if (!exists || !exConnection.Valid)
            {
                var result=_connections.TryAdd(conversationId,new ConnectionInfo(connection,DateTime.Now));
                return Task.FromResult(result);
            }
            return Task.FromResult(false);
        }

        private async Task SendMessageHandler(string conversationId, ArraySegment<byte> message)
        {
            var connection = await this.GetConnectionAsync(conversationId);
            await connection.SendAsync(message);
        }

        // todo : return Task<RegisterResult>
        private async Task RegisterConnectionHandler(string conversationId, IDirectLineConnection connection)
        {
            await this.RegisterConnectionHandler(conversationId,connection);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._connectionManager.OnSend += SendMessageHandler;
            this._connectionManager.OnRegister += RegisterConnectionHandler;
            this.timer.Elapsed += async (source , e) => {
                var invalids = this._connections.Where(c => !c.Value.Valid && c.Value.Connection.Avaiable);
                Parallel.ForEach(invalids, async kvp => {
                    var connInstance = kvp.Value.Connection;
                    await connInstance.CloseAsync("closed since it's invalid");
                    this._connections.Remove(kvp.Key,out var connectionRemoved);
                });
            };
            this.timer.Enabled = true;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._connectionManager.OnSend -= SendMessageHandler;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this.timer?.Dispose();
        }

        private class ConnectionInfo
        {
            public ConnectionInfo(IDirectLineConnection connection, DateTime lastVisitedAt)
            {
                Connection = connection;
                LastVisitedAt = lastVisitedAt;
                WillExpiredAt = LastVisitedAt.AddSeconds(Expires);
            }

            public enum ConnectionStatus { Avaible, NotAvaible }
            public IDirectLineConnection Connection { get; set; }
            public DateTime LastVisitedAt { get; set; }
            public DateTime WillExpiredAt { get; set; } 
            public ConnectionStatus Status { get; set; } = ConnectionStatus.Avaible;
            public bool Valid {
                get {
                    return Status != ConnectionStatus.NotAvaible
                       && DateTime.Now < WillExpiredAt;
                }
            }
        }

    }



}
