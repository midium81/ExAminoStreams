using DevExpress.Pdf.Native.BouncyCastle.Utilities.Encoders;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace ExAminoStreams.API
{
    public class FlowAPI
    {
        private string _baseUri = "https://api.statsperform.io/external/dvr/{0}";

        public async Task<Streams> GetStreams(string fixtureId)
        {
            var payload = await GetPayload(fixtureId);
            if (!string.IsNullOrEmpty(payload))
                return new Streams() { AvailableStreams = JsonSerializer.Deserialize<List<StreamData>>(payload) };

            return new();
        }

        private async Task<string> GetPayload(string fixtureId)
        {
            using (var client = new HttpClient()) // new HttpRetryMessageHandler(new HttpClientHandler())))
            {
                var endpoint = string.Format(_baseUri, fixtureId);
                var token = GetSignedJWT();

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = null;
                try
                {
                    response = await client.GetAsync(endpoint);
                    if(response.IsSuccessStatusCode)
                    {
                        var jsonData = await response.Content.ReadAsStringAsync();
                        return jsonData;
                    }
                    else
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.HttpVersionNotSupported)
                            Manage505Response(await response.Content.ReadAsStringAsync());
                        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                            Manage404Response(await response.Content.ReadAsStringAsync());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Unhandled Error");
                }

                return string.Empty;
            }
        }

        private void Manage404Response(string message)
        {
            var error = JsonSerializer.Deserialize<FixtureIdError>(message);
            if (error is null) return;
            MessageBox.Show(error.Message, "FixtureId Error");
        }

        private void Manage505Response(string message)
        {
            var error = JsonSerializer.Deserialize<APIError>(message);
            if (error is null) return;
            MessageBox.Show(error.Error, "API Error");
        }

        private string GetSignedJWT()
        {

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("C2sHA3kdAsFaIzD2e7SBzMOq15g6Bvu3ZJioP5e76Swq6XzD"));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var header = new JwtHeader(signingCredentials);
            var payload = new JwtPayload
            {
            };

            var secToken = new JwtSecurityToken(header, payload);
            return new JwtSecurityTokenHandler().WriteToken(secToken);
        }
    }
}