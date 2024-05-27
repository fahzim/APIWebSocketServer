using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;

namespace Middle.Model
{


    public class wsConnectionsModel
    {
        public Dictionary<WebSocket, AppOrigin> DcWsConnections { get; set; }

        public void initializeDictionary()
        {
            if (DcWsConnections is null)
            {
                DcWsConnections = new Dictionary<WebSocket, AppOrigin>();
            }
        }

        /// <summary>
        /// ws = new WebSocket created
        /// appOrigin = enum to determine connection origin system.
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="appOrigin"></param>
        public void AddToDcConnections(WebSocket ws, AppOrigin appOrigin)
        {
            //For each add to Dictionary, i validate and remove all of closed connections of ws.
            //DcWsConnections.Remove((WebSocket)DcWsConnections.Keys.Where(x => x.State == WebSocketState.Closed));
            
            //After i add a new.
            DcWsConnections.Add(ws , appOrigin);
        }
    }

    
}
;