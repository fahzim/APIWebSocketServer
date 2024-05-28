using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;

namespace Middle.Model
{


    public class wsConnectionsModel
    {
        /// <summary>
        /// Dictionary for better use of rules in connections.
        /// </summary>
        public Dictionary<WebSocket, AppOrigin> DcWsConnections { get; set; }

        public void initializeDictionary()
        {
            if (DcWsConnections is null)
            {
                DcWsConnections = new Dictionary<WebSocket, AppOrigin>();
            }
        }

        /// <summary>
        /// Add new value to dictionary
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="appOrigin"></param>
        public void AddToDcConnections(WebSocket ws, AppOrigin appOrigin)
        {
            DcWsConnections.Add(ws , appOrigin);
        }
    }

    
}
;