using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DemandasComunidade.Tests
{
    public class MockBrasilApiHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Dados que a Brasil API retornaria na vida real
            var fakeResponse = new
            {
                cep = "01001000",
                state = "SP",
                city = "São Paulo",
                neighborhood = "Sé",
                street = "Praça da Sé"
            };

            var json = JsonSerializer.Serialize(fakeResponse);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}