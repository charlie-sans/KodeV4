using KodeRunner;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace KodeRunner
{
    [Service]
    public class MyCustomService
    {
        [Route("/hello", "GET")]
        public async Task Hello(HttpContext ctx)
        {
            await ctx.Response.WriteAsync("Hello, World!");
        }

        [WebSocketRoute("/echo")]
        public async Task Echo(WebSocket ws)
        {
            var buffer = new byte[1024];
            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    await ws.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
