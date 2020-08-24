// Copyright 2020, Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Http;
using Google.Apis.Json;
using Google.Apis.Requests;
using Google.Apis.Util;
using Newtonsoft.Json;

namespace FirebaseAdmin.IntegrationTests.Auth
{
    internal class AuthIntegrationUtils
    {
        internal static readonly NewtonsoftJsonSerializer JsonParser =
            NewtonsoftJsonSerializer.Instance;

        private const string EmailLinkSignInUrl =
            "https://www.googleapis.com/identitytoolkit/v3/relyingparty/emailLinkSignin";

        private const string ResetPasswordUrl =
            "https://www.googleapis.com/identitytoolkit/v3/relyingparty/resetPassword";

        private const string VerifyCustomTokenUrl =
            "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyCustomToken";

        internal static async Task<string> SignInWithCustomTokenAsync(
            string customToken, string tenantId = null)
        {
            var rb = new RequestBuilder()
            {
                Method = HttpConsts.Post,
                BaseUri = new Uri(VerifyCustomTokenUrl),
            };
            rb.AddParameter(RequestParameterType.Query, "key", IntegrationTestUtils.GetApiKey());

            var request = rb.CreateRequest();
            var payload = JsonParser.Serialize(new SignInRequest
            {
                CustomToken = customToken,
                TenantId = tenantId,
                ReturnSecureToken = true,
            });
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await SendAndDeserialize<SignInResponse>(request);
            return response.IdToken;
        }

        internal static async Task<string> ResetPasswordAsync(ResetPasswordRequest data)
        {
            var rb = new RequestBuilder()
            {
                Method = HttpConsts.Post,
                BaseUri = new Uri(ResetPasswordUrl),
            };
            rb.AddParameter(RequestParameterType.Query, "key", IntegrationTestUtils.GetApiKey());

            var payload = JsonParser.Serialize(data);
            var request = rb.CreateRequest();
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await SendAndDeserialize<Dictionary<string, object>>(request);
            return (string)response["email"];
        }

        internal static async Task<string> SignInWithEmailLinkAsync(
            string email, string oobCode, string tenantId = null)
        {
            var rb = new RequestBuilder()
            {
                Method = HttpConsts.Post,
                BaseUri = new Uri(EmailLinkSignInUrl),
            };
            rb.AddParameter(RequestParameterType.Query, "key", IntegrationTestUtils.GetApiKey());

            var data = new Dictionary<string, object>()
            {
                { "email", email },
                { "oobCode", oobCode },
            };
            if (tenantId != null)
            {
                data["tenantId"] = tenantId;
            }

            var payload = JsonParser.Serialize(data);
            var request = rb.CreateRequest();
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await SendAndDeserialize<Dictionary<string, object>>(request);
            return (string)response["idToken"];
        }

        private static async Task<T> SendAndDeserialize<T>(HttpRequestMessage request)
        {
            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonParser.Deserialize<T>(json);
            }
        }

        internal class SignInRequest
        {
            [JsonProperty("token")]
            public string CustomToken { get; set; }

            [JsonProperty("tenantId")]
            public string TenantId { get; set; }

            [JsonProperty("returnSecureToken")]
            public bool ReturnSecureToken { get; set; }
        }

        internal class SignInResponse
        {
            [JsonProperty("idToken")]
            public string IdToken { get; set; }
        }
    }

    internal class ResetPasswordRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("oldPassword")]
        public string OldPassword { get; set; }

        [JsonProperty("newPassword")]
        public string NewPassword { get; set; }

        [JsonProperty("oobCode")]
        public string OobCode { get; set; }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }
    }
}