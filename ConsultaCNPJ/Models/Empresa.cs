using Newtonsoft.Json;

namespace ConsultaCNPJ.Models;

public class Empresa
{
    [JsonProperty("status")]
    public string? Status { get; set; }
    [JsonProperty("message")]
    public string? Message { get; set; }
    [JsonProperty("cnpj")]
    public string? Cnpj { get; set; }
    [JsonProperty("tipo")]
    public string? Tipo { get; set; }
    [JsonProperty("abertura")]
    public string? Abertura { get; set; }
    [JsonProperty("nome")]
    public string? Nome { get; set; }
    [JsonProperty("atividade_principal")]
    public List<Atividade>? AtividadePrincipal { get; set; }
    [JsonProperty("atividades_secundarias")]
    public List<Atividade>? AtividadesSecundarias { get; set; }
    [JsonProperty("natureza_juridica")]
    public string? NaturezaJurídica { get; set; }
    [JsonProperty("logradouro")]
    public string? Logradouro { get; set; }
    [JsonProperty("numero")]
    public string? Numero { get; set; }
    [JsonProperty("complemento")]
    public string? Complemento { get; set; }
    [JsonProperty("cep")]
    public string? Cep { get; set; }
    [JsonProperty("bairro")]
    public string? Bairro { get; set; }
    [JsonProperty("municipio")]
    public string? Municipio { get; set; }
    [JsonProperty("uf")]
    public string? Uf { get; set; }
    [JsonProperty("email")]
    public string? Email { get; set; }
    [JsonProperty("telefone")]
    public string? Telefone { get; set; }
    [JsonProperty("capital_social")]
    public string? CapitalSocial { get; set; }
    [JsonProperty("opcao_pelo_simples")]
    public string? SimplesNacional { get; set; }
    [JsonProperty("opcao_pelo_mei")]
    public string? Mei { get; set; }
    [JsonProperty("qsa")]
    public List<Socio>? Qsa { get; set; }
    [JsonProperty("situacao")]
    public string? SituacaoCadastral { get; set; }
    [JsonProperty("data_situacao")]
    public string? DataSituaçãoCadastral { get; set; }
}

public class Atividade
{
    [JsonProperty("code")]
    public string? Code { get; set; }
    [JsonProperty("text")]
    public string? Text { get; set; }
}

public class Socio
{
    [JsonProperty("nome")]
    public string? Nome { get; set; }
    [JsonProperty("qual")]
    public string? Qual { get; set; }
}