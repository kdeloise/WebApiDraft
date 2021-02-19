using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.WEB.Logger
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //managing request
            //creating log object
            var log = new LogModel
            {
                Path = context.Request.Path,
                Method = context.Request.Method,
                QueryString = context.Request.QueryString.ToString()
            };
            //checking if request body is required
            if (context.Request.Method == "POST")
            {
                context.Request.EnableBuffering();
                var body = await new StreamReader(context.Request.Body)
                    .ReadToEndAsync();
                context.Request.Body.Position = 0;
                log.RequestBody = body;
            }

            log.RequestedOn = DateTime.Now;

            //managing response
            //saving original stream
            var originalBodyStream = context.Response.Body;
            //creating new memory stream to read response safely
            using (var responseBodyStream = new MemoryStream())
            {
                context.Response.Body = responseBodyStream;

                await _next.Invoke(context);

                //reading response
                responseBodyStream.Position = 0;
                var response = await new StreamReader(responseBodyStream)
                    .ReadToEndAsync();
                responseBodyStream.Position = 0;

                //applying response to log object
                log.Response = response;
                log.ResponseCode = context.Response.StatusCode.ToString();
                log.RespondedOn = DateTime.Now;

                Store(log);

                //returning result of a memory stream to original stream
                //so client can read it as normal
                await responseBodyStream.CopyToAsync(originalBodyStream);
            }
        }

        public void Store(LogModel log)
        {
            string[] lines =
            {
                $"Path: {log.Path}",
                $"QueryString: {log.QueryString}",
                $"Method: {log.Method}",
                $"RequestBody: {log.RequestBody}",
                $"Requested at: {log.RequestedOn}",
                $"Response: {log.Response}",
                $"ResponseCode: {log.ResponseCode}",
                $"Responded at: {log.RespondedOn}",
                Environment.NewLine
            };

            File.AppendAllLines(@"Logger\Logs\log.txt", lines);
        }
    }
}
