// <copyright file="UserController.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [Route("user")]

    public class UserController : ApiController
    {
        [HttpGet("list")]

        public async Task<IActionResult> GetUsers()
        {
            var l = await Task.Run(() => new List<string>(new string[] { "1", "A", "1", "1i", "1", "1123", "1", "1", "1", "1", "1" }));

            // Get the current time
            DateTimeOffset now = DateTimeOffset.Now;

            // Convert to Unix time in seconds
            long epochTime = now.ToUnixTimeSeconds();

            // Print the epoch time
            Console.WriteLine("Epoch time: {0}", epochTime);

            return this.Ok(l);
        }
    }
}
