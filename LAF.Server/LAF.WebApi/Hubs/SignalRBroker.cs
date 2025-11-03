using LAF.Dtos;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LAF.WebApi.Hubs
{
    public interface ISignalRBroker
    {
        Task SendToAll<T>(string eventName, SignalRBrokerMessages<T> obj) where T : class;
    }

    public class SignalRBroker(IHubContext<LafHub> hubContext) : ISignalRBroker
    {
        private readonly IHubContext<LafHub> hubContext = hubContext;

        public async Task SendToAll<T>(string eventName, SignalRBrokerMessages<T> obj) where T : class
        {
            await hubContext.Clients.All.SendAsync(eventName, obj);
        }

    }

    public record SignalRBrokerMessages<T>(int sender, T Payload) where T : class;
    public class SignalRBrokerMessages
    {
        public const string PositionCellEditing = nameof(PositionCellEditing);
        public const string PositionChanged = nameof(PositionChanged);
        public const string NewTrade = nameof(NewTrade);
        public const string CashflowCreated = nameof(CashflowCreated);
        public const string CashflowDeleted = nameof(CashflowDeleted);
        public const string RepoCircleUpdated = nameof(RepoCircleUpdated);
    }
}
