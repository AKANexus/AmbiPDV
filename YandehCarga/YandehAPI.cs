﻿using System;
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
        private static Logger _logger = new Logger("YandehAPI");
#if DEBUG
        private const string UrlApi = "https://hml-integration.yandeh.com.br";
        private const string AuthKey = "56E1314DDAF04A0CB39E2618F213ECB6";
#else
        private const string UrlApi = "https://integration-new.yandeh.com.br";
        private const string AuthKey = "E38B9099379C485D8515B8A4FD4271D3";
#endif

        public static async Task<(bool, string)> EnviaEstoque(EstoqueBody bodyObject)
        {
            _logger.Debug($"EnviaEstoque >> {bodyObject.description}");
            var clientOptions = new RestClientOptions($"{UrlApi}/product");
            clientOptions.Timeout = -1;
            var client = new RestClient(clientOptions);

            var request = new RestRequest();
            request.AddHeader("Authorization", AuthKey);
            request.AddHeader("Content-Type", "application/json");
            //var bodyObject = $"{{\"product_type\":\"simple\",\"sku\":\"{GTIN}\",\"name\":\"{Descrição}\",\"description\":\"{Descrição}\",\"price_info\":{{\"price\":{Preço.ToString("N4", CultureInfo.InvariantCulture)}}},\"visibility\":\"T\",\"status\":\"A\"}}";
            //request.AddParameter("application/json", bodyObject, ParameterType.RequestBody);

            //var json = JsonSerializer.Serialize(bodyObject);


            request.AddJsonBody(bodyObject);
            var response = await client.ExecutePostAsync<DefaultResponse>(request);
            _logger.Debug($"EnviaEstoque successful: {response.IsSuccessful}");
            if (response.IsSuccessful && response.Data is not null)
            {
                _logger.Debug($"EnviaEstoque << {response.Data.message}");
                return (true, response.Data.message);
            }
            else
            {
                _logger.Debug($"EnviaEstoque |<< {response?.Data?.message ?? "null"}");
                return (false, response?.Data?.message ?? "Erro inesperado");

            }
        }

        public static async Task<(bool, string)> EnviaSellin(SellInBody bodyObject)
        {
            var clientOptions = new RestClientOptions($"{UrlApi}/sellin");
            clientOptions.Timeout = -1;
            var client = new RestClient(clientOptions);

            var request = new RestRequest();
            request.AddHeader("Authorization", AuthKey);
            request.AddHeader("Content-Type", "application/json");

            var json = JsonSerializer.Serialize(bodyObject);


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
            _logger.Debug($"EnviaSellout >> {bodyObject.id}");

            var clientOptions = new RestClientOptions($"{UrlApi}/sellout");
            clientOptions.Timeout = -1;
            var client = new RestClient(clientOptions);

            var request = new RestRequest();
            request.AddHeader("Authorization", AuthKey);
            request.AddHeader("Content-Type", "application/json");

            request.AddJsonBody(bodyObject);

            _logger.Debug($"EnviaSellout Json: {JsonSerializer.Serialize(bodyObject)}");

            var response = await client.ExecutePostAsync<DefaultResponse>(request);
            _logger.Debug($"EnviaSellout successful: {response.IsSuccessful}");

            if (response.IsSuccessful && response.Data is not null)
            {
                _logger.Debug($"EnviaSellout << {response.Data.message}");
                return (true, response.Data.message);
            }
            else
            {
                _logger.Debug($"EnviaSellout |<< {response?.Data?.message ?? "null"}");
                return (false, response.Data?.message ?? "Erro inesperado");

            }
        }

        public static async Task<(bool, string)> CadastraAPIKey(string cnpj)
        {
            var clientOptions = new RestClientOptions($"{UrlApi}/clients");
            clientOptions.Timeout = -1;
            var client = new RestClient(clientOptions);

            var request = new RestRequest();
            request.AddHeader("Authorization", AuthKey);
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

            _logger.Debug($"CadastraAPIKey >> {JsonSerializer.Serialize(body)}");

            var response = await client.ExecutePostAsync<Authorization>(request);
            _logger.Debug($"CadastraAPIKey successful: {response.IsSuccessful}");

            if (response.IsSuccessful && response.Data is not null)
            {
                _logger.Debug($"CadastraAPIKey << {response.Data.authkey}");
                return (true, response.Data.authkey);
            }
            else
            {
                _logger.Debug($"CadastraAPIKey |<< Falha ao obter a authKey");
                return (false, "Falha ao obter a authKey");
            }
        }
    }
}
