using System;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Middle.Model;

namespace Middle.Controllers
{
    [ApiController]
    [Route("ws")]
    public class WebSocketController : ControllerBase
    {

        private readonly wsConnectionsModel _wsConnectionsModel;
        private readonly ILogger<WebSocketController> _logger;

        public WebSocketController(ILogger<WebSocketController> logger, IOptions<wsConnectionsModel> wsConnectionsModel)
        {
            _wsConnectionsModel = wsConnectionsModel.Value;
            _logger = logger;
        }

        [HttpGet]
        public async Task GetWs(AppOrigin appOrigin)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                _wsConnectionsModel.initializeDictionary();

                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                //To send respective for 1 to 2 and 2 to 1 i need control origin connection to determine witch one are. I use same sample above, but control too using enum for type of application origin connection.
                _wsConnectionsModel.AddToDcConnections(webSocket, appOrigin);

                await SendReceiveToAll(webSocket, appOrigin);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task SendReceiveToAll(WebSocket webSocket, AppOrigin appOrigin)
        {

            var buffer = new byte[1024 * 4];

            //Define socket to await message
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            var msgReceived = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);

            //I retrieve only active connections, and use with enum AppOrigin to send only 1 to 2 and 2 to 1 using below rule.
            Dictionary<WebSocket, AppOrigin> connections = _wsConnectionsModel.DcWsConnections;

            //Use to get only open connections, and the opositive of requested connection.
            var validConnections = connections.Where(x => x.Key.State == WebSocketState.Open);

            //Optional code....
            //If false, when someone sended a message, all anothers active connections will receive the message, independent if are windows or web.
            //if true, only opposite application will receive the msg.
            //If will be used, better move to appsettings and call a _configuration to read .json key.
            bool only1to2and2to1 = true;
            if (only1to2and2to1)
            {
                validConnections = connections.Where(x => x.Value != appOrigin);
            }
            
            while (!receiveResult.CloseStatus.HasValue)
            {
                msgReceived = msgReceived.Replace("\"", "");
                var jsonString = JsonSerializer.Serialize<jsonModel>(new jsonModel { text = msgReceived });
                buffer = Encoding.UTF8.GetBytes(jsonString);


                foreach (var (webSkt, conns) in validConnections.Where(x => x.Key.State == WebSocketState.Open))
                {
                    await webSkt.SendAsync(
                    new ArraySegment<byte>(buffer, 0, buffer.Length),
                    receiveResult.MessageType,
                    receiveResult.EndOfMessage,
                    CancellationToken.None);
                }

                //reset await for Receive on original socket.
                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);

                //Just if i want see msg on server.
                msgReceived = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);

            }

            //await to close...
            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }
    }
}

