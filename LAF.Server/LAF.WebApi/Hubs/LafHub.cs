using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace LAF.WebApi.Hubs
{
    public class LafHub : Hub
    {
        private readonly ILogger<LafHub> _logger;
        private static readonly ConcurrentDictionary<string, string> _connectedClients = new();

        public LafHub(ILogger<LafHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            _connectedClients.TryAdd(connectionId, connectionId);
            _logger.LogInformation("Client connected: {ConnectionId}", connectionId);
            // Send hello message to all other clients
            await Clients.Others.SendAsync("ClientJoined", $"Client {connectionId} has joined");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            _connectedClients.TryRemove(connectionId, out _);

            if (exception != null)
            {
                _logger.LogError(exception, "Client disconnected with error: {ConnectionId}", connectionId);
            }
            else
            {
                _logger.LogInformation("Client disconnected: {ConnectionId}", connectionId);
            }

            // Notify others that a client has left
            await Clients.Others.SendAsync("ClientLeft", $"Client {connectionId} has left");

            await base.OnDisconnectedAsync(exception);
        }

        public int GetConnectedClientsCount()
        {
            return _connectedClients.Count;
        }

    }
}