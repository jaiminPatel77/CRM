namespace Crm.Application.Common.Interfaces;

public interface IApplicationHubService
{
    Task BroadcastMessage(string message);
    Task SendToUser(string userId, string message);
}
