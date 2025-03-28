using ConsultaCNPJ.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConsultaCNPJ.Services
{
    public class ConsultaCnpjService
    {
        private readonly HttpClient _httpClient;

        public ConsultaCnpjService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Empresa?> ConsultarCnpjAsync(string cnpj) 
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://receitaws.com.br/v1/cnpj/{cnpj}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var empresa = JsonConvert.DeserializeObject<Empresa>(json);
                    return empresa; 
                }
                return new Empresa { Cnpj = cnpj, Status = "ERROR" }; 
            }
            catch (Exception)
            {
                return new Empresa { Cnpj = cnpj, Status = "ERROR" }; 
            }
        }
    }
}