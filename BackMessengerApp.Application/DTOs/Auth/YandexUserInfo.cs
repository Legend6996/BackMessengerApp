using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BackMessengerApp.Application.DTOs.Auth
{
    public class YandexUserInfo : IOAuthUserInfo
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("emails")]
        public List<string> Emails { get; set; }

        [JsonPropertyName("default_email")]
        public string Email { get; set; }

        [JsonPropertyName("default_phone")]
        public DefaultPhone DefaultPhone { get; set; }

        [JsonPropertyName("real_name")]
        public string Name { get; set; }

        [JsonPropertyName("is_avatar_empty")]
        public bool IsAvatarEmpty { get; set; }

        [JsonPropertyName("birthday")]
        public string Birthday { get; set; }

        [JsonPropertyName("default_avatar_id")]
        public string DefaultAvatarId { get; set; }

        [JsonPropertyName("login")]
        public string Login { get; set; }

        [JsonPropertyName("old_social_login")]
        public string OldSocialLogin { get; set; }

        [JsonPropertyName("sex")]
        public string Sex { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }

        [JsonPropertyName("psuid")]
        public string Psuid { get; set; }
    }

    public class DefaultPhone
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("number")]
        public string Number { get; set; }
    }
}
