using System;
using System.Linq;
using System.Threading.Tasks;
using Qxyz.Core.Sockets;
using Microsoft.AspNetCore.SignalR;

namespace Qxyz.Web.Hubs
{
    public class GroupHub : Hub
    {
        private SocketGroupProvider groups;

        public GroupHub(SocketGroupProvider groups)
        {
            this.groups = groups;
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var connections = groups
                .SocketGroups
                .Where(x => x.Connections.Contains(Context.ConnectionId))
                .Select(x => x.Name)
                .ToList();

            foreach (var c in connections)
            {
                await groups.RemoveFromSocketGroup(Context.ConnectionId, c);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, c);
                await Clients.GroupExcept(c, Context.ConnectionId).SendAsync("groupAlert", $"{Context.UserIdentifier} has left {c}");
            }

            await base.OnDisconnectedAsync(ex);
        }

        public async Task triggerJoinGroup(string group)
        {
            await groups.AddToSocketGroup(Context.ConnectionId, group);
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await Clients.GroupExcept(group, Context.ConnectionId).SendAsync("groupAlert", $"{Context.User.Identity.Name} has joined {group}");
        }

        public async Task triggerLeaveGroup(string group)
        {
            await groups.RemoveFromSocketGroup(Context.ConnectionId, group);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
            await Clients.GroupExcept(group, Context.ConnectionId).SendAsync("groupAlert", $"{Context.User.Identity.Name} has left {group}");
        }

        public async Task triggerGroupMessage(string group)
        {
            await Clients.Group(group).SendAsync("groupMessage");
        }
    }
}