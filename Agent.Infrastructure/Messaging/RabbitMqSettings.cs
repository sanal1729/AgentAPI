// <copyright file="RabbitMqSettings.cs" company="Agent">
// © Agent 2025
// </copyright>

public class RabbitMqSettings
    {
        public const string SectionName = "RabbitMqSettings";

        public string HostName { get; set; } = "localhost";

        public int Port { get; set; } = 5672;

        public string UserName { get; set; } = "admin";

        public string Password { get; set; } = "admin";
    }