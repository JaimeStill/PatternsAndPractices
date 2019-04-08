using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Qxyz.Core.Sockets
{
    public class SocketGroupProvider
    {
        public List<SocketGroup> SocketGroups { get; set; }

        public SocketGroupProvider()
        {
            SocketGroups = new List<SocketGroup>();
        }

        public Task AddToSocketGroup(string connectionId, string group) => Task.Run(() =>
        {
            var check = SocketGroups.FirstOrDefault(x => x.Name == group);

            if (check == null)
            {
                SocketGroups.Add(new SocketGroup
                {
                    Name = group,
                    Connections = new List<string> { connectionId }
                });
            }
            else
            {
                if (!check.Connections.Contains(connectionId))
                {
                    check.Connections.Add(connectionId);
                }
            }
        });

        public Task RemoveFromSocketGroup(string group, string connectionId) => Task.Run(() =>
        {
            var check = SocketGroups.FirstOrDefault(x => x.Name == group);

            if (check != null)
            {
                if (check.Connections.Contains(connectionId))
                {
                    check.Connections.Remove(connectionId);

                    if (check.Connections.Count < 1)
                    {
                        SocketGroups.Remove(check);
                    }
                }
            }
        });
    }
}