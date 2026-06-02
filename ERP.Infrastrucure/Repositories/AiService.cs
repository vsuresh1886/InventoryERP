using ERP.Application.DTOs;
using ERP.Application.DTOs.Quotation;
using ERP.Application.Interfaces.Repositories;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ERP.Infrastructure.Repositories
{
    public class AiService:IAiService
    {

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public AiService(HttpClient httpClient, IConfiguration config, AppDbContext context)
        {
            _httpClient = httpClient;
            _config = config;
            _context = context;
        }

        public async Task<AiQuotationfilterDto> ParseQuotationQuery(string query)
        {
            var prompt = $@"
                            Convert the following user query into JSON filters.

                            Rules:
                            - Return JSON only
                            - No explanation
                            - Use null for missing fields

                            Example Output:
                            {{
                              ""quotationno"" :"""",
                              ""customer"": ""Murali"",
                              ""status"": ""Approved"",
                              ""fromDate"": ""2026-05-01"",
                              ""toDate"": ""2026-05-31"",
                              ""salesperson"": ""suresh"",
                            }}

                            User Query:
                            {query}
                            ";

            var requestBody = new
            {
                model = "gpt-5.4-mini",
                messages = new[]
                {
                new
                {
                    role = "user",
                    content = prompt
                }
            }
            };

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    _config["OpenAI:ApiKey"]);

            var response =
                await _httpClient.PostAsJsonAsync(
                    "https://api.openai.com/v1/chat/completions",
                    requestBody);

            var json =
                await response.Content.ReadAsStringAsync();

            using var doc =
                JsonDocument.Parse(json);

            var content =
                doc.RootElement
                   .GetProperty("choices")[0]
                   .GetProperty("message")
                   .GetProperty("content")
                   .GetString();
            var aiResult = JsonSerializer.Deserialize<AiQuotationRawDto>(content);

            // convert the customer name / sales man name / status  to id's
            var customerIds = new List<string>();
            var statusIds =0;
            var salespersonIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(aiResult.customer))
            {
                customerIds = await Customerid(aiResult.customer);
            }
                       
            if (!string.IsNullOrWhiteSpace(aiResult.salesperson))
            {
                salespersonIds = await Salesman(aiResult.salesperson);
            }

            if (!string.IsNullOrWhiteSpace(aiResult.status))
            {
                statusIds = await statusid(aiResult.status, "QUOTATION_STATUS");
            }
            // end 

            var finalFilters =
                    new AiQuotationfilterDto
                    {
                        quotationno =  null,

                        customer = customerIds,

                        salesperson = new List<string>(),

                        datefrom =
                            aiResult.fromDate ?? "",

                        dateto =
                            aiResult.toDate ?? "",

                        status =   statusIds 
                    };

            return finalFilters;


        }


        public async Task<List<string>> Customerid(string customername)
        {
            try
            {
                var customerIds = await _context.customers

                        .Where(x =>

                            EF.Functions.ILike(
                                x.company_name,
                                $"%{customername}%"
                            )

                            ||

                            EF.Functions.TrigramsSimilarity(
                                x.company_name,
                                customername
                            ) > 0.3
                     )

                    .Select(x =>
                        x.cust_pk.ToString())

                    .ToListAsync();



                return customerIds;
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<List<string>> Salesman(string salesperson)
        {
            try
            {
                var salesmanIds = await _context.employees

                        .Where(x =>

                            EF.Functions.ILike(
                                x.first_name,
                                $"%{salesperson}%"
                            )

                            ||

                            EF.Functions.TrigramsSimilarity(
                                x.first_name,
                                salesperson
                            ) > 0.3
                     )

                    .Select(x =>
                        x.employee_pk.ToString())

                    .ToListAsync();



                return salesmanIds;
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<int> statusid(string status, string lookuptype)
        {
            try
            {
                var statid = await _context.ddlookups

                        .Where(x => x.lookup_type == lookuptype &&

                            EF.Functions.ILike(
                                x.code,
                                $"%{status}%"
                            )

                            ||

                            EF.Functions.TrigramsSimilarity(
                                x.code,
                                status
                            ) > 0.3
                     )

                    .Select(x =>
                        (int)x.id)

                    .FirstOrDefaultAsync();



                return statid;
            }
            catch
            {
                return 0;
            }
        }


    }
}
