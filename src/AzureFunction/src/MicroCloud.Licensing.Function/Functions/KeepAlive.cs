// using System;
// using System.Threading.Tasks;
// using Microsoft.Azure.Functions.Worker;
// using Microsoft.Extensions.Logging;
 
// namespace MicroCloud.Licensing.Function.Functions
// {
//     public class KeepAlive
//     {
//         [Function("KeepAlive")]
//         public Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger log)
//         {
//             log.LogInformation("KeepAlive timer trigger executed at: {Time}", DateTime.UtcNow);
//             return Task.CompletedTask;
//         }
//     }
// }


