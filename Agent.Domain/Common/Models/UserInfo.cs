// <copyright file="UserInfo.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Domain.Common.Models
{
    public class UserInfo(string phone, string country) : ValueObject
    {
        public string PhoneNumber { get; private set; } = phone;

        public string Country { get; private set; } = country;

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.PhoneNumber;
            yield return this.Country;
        }
    }
}
