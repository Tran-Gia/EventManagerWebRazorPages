using EventManagerWebRazorPage.Models;
using Microsoft.AspNetCore.SignalR;

namespace EventManagerWebRazorPage.Hubs
{
    public class ConfirmStatusHub : Hub
    {
        public async Task UserUpdateConfirmStatus(string userRegisId, ConfirmStatus confirmStatus)
        {
            await Clients.All.SendAsync("ReceiveUserUpdateConfirmStatus", userRegisId, confirmStatus);
        }
    }
}
