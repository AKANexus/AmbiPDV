using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using RestSharp;
using YandehCarga.Yandeh;

namespace YandehCarga
{
    public static class YandehAPI
    {
        public static async Task<(bool, string)> EnviaEstoque(string GTIN, string Descrição, decimal Preço)
        {
            var clientOptions = new RestClientOptions("https://hml-integration.yandeh.com.br/product");
            clientOptions.Timeout = -1;
            var client = new RestClient(clientOptions);

            var request = new RestRequest();
            request.AddHeader("Authorization", "56E1314DDAF04A0CB39E2618F213ECB6");
            request.AddHeader("Content-Type", "application/json");
            //var body = $"{{\"product_type\":\"simple\",\"sku\":\"{GTIN}\",\"name\":\"{Descrição}\",\"description\":\"{Descrição}\",\"price_info\":{{\"price\":{Preço.ToString("N4", CultureInfo.InvariantCulture)}}},\"visibility\":\"T\",\"status\":\"A\"}}";
            //request.AddParameter("application/json", body, ParameterType.RequestBody);

            EstoqueBody body = new()
            {
                product_type = "simple",
                sku = GTIN,
                name = Descrição,
                description = Descrição,
                price_info = new Price_Info()
                {
                    price = (float)Preço
                },
                visibility = "T",
                status = "A"
            };
            request.AddJsonBody(body);
            var response = await client.ExecutePostAsync<DefaultResponse>(request);
            if (response.IsSuccessful && response.Data is not null)
            {
                return (true, response.Data.message);
            }
            else
            {
                return (false, response.Data?.message ?? "Erro inesperado");

            }
        }

        public static async Task<(bool, string)> EnviaSellin(SelloutBody bodyObject)
        {
            var clientOptions = new RestClientOptions("https://hml-integration.yandeh.com.br/sellin");
            clientOptions.Timeout = -1;
            var client = new RestClient(clientOptions);

            var request = new RestRequest();
            request.AddHeader("Authorization", "56E1314DDAF04A0CB39E2618F213ECB6");
            request.AddHeader("Content-Type", "application/json");

            request.AddJsonBody(bodyObject);


            var response = await client.ExecutePostAsync<DefaultResponse>(request);
            if (response.IsSuccessful && response.Data is not null)
            {
                return (true, response.Data.message);
            }
            else
            {
                return (false, response.Data?.message ?? "Erro inesperado");

            }
        }

        public static async Task<(bool, string)> EnviaSellout(SelloutBody bodyObject)
        {
            var clientOptions = new RestClientOptions("https://hml-integration.yandeh.com.br/sellout");
            clientOptions.Timeout = -1;
            var client = new RestClient(clientOptions);

            var request = new RestRequest();
            request.AddHeader("Authorization", "56E1314DDAF04A0CB39E2618F213ECB6");
            request.AddHeader("Content-Type", "application/json");
            
            request.AddJsonBody(bodyObject);

            var json = JsonSerializer.Serialize(bodyObject);

            var response = await client.ExecutePostAsync<DefaultResponse>(request);
            if (response.IsSuccessful && response.Data is not null)
            {
                return (true, response.Data.message);
            }
            else
            {
                return (false, response.Data?.message ?? "Erro inesperado");

            }
        }

        public static async Task<(bool, string)> CadastraAPIKey(string cnpj)
        {
            var clientOptions = new RestClientOptions("https://hml-integration.yandeh.com.br/clients");
            clientOptions.Timeout = -1;
            var client = new RestClient(clientOptions);

            var request = new RestRequest();
            request.AddHeader("Authorization", "56E1314DDAF04A0CB39E2618F213ECB6");
            request.AddHeader("Content-Type", "application/json");
            AuthorizationBodyJson body = new()
            {
                name = cnpj,
                allowedDomains = new(),
                allowedIPs = new()
            };
            body.allowedDomains.Add("*");
            body.allowedIPs.Add("*");
            //var body = $"{{\"name\":\"{cnpj}\",\"allowedIPs\":[\"*\"],\"allowedDomains\":[\"*\"]}}";
            //request.AddParameter("application/json", body, ParameterType.RequestBody);
            request.AddJsonBody(body);
            var response = await client.ExecutePostAsync<Authorization>(request);
            if (response.IsSuccessful && response.Data is not null)
            {
                return (true, response.Data.authkey);
            }
            else
            {
                return (false, "Falha ao obter a authKey");
            }
        }
    }
}
