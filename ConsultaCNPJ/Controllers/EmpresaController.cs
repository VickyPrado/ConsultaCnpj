using ConsultaCNPJ.Models;
using ConsultaCNPJ.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Newtonsoft.Json;
using System.Globalization;

namespace ConsultaCNPJ.Controllers
{
    public class EmpresaController : Controller
    {
        private readonly ConsultaCnpjService _consultaService;
        private static List<Empresa> _empresasSalvas = new List<Empresa>();

        public EmpresaController(ConsultaCnpjService consultaService)
        {
            _consultaService = consultaService;
        }

        public IActionResult Consultar()
        {
            return View(_empresasSalvas);
        }

        [HttpPost]
        public async Task<IActionResult> Consultar(string cnpjs)
        {
            if (string.IsNullOrEmpty(cnpjs))
            {
                ViewBag.Erro = "Digite pelo menos um CNPJ";
                return View(_empresasSalvas);
            }

            var cnpjList = cnpjs.Split(',').Select(c => c.Trim().Replace(".", "").Replace("/", "").Replace("-", "")).ToList();
            var empresasExistentes = _empresasSalvas.Where(e => cnpjList.Contains(e.Cnpj?.Replace(".", "").Replace("/", "").Replace("-", "") ?? "")).ToList();
            var cnpjNaoSalvos = cnpjList.Except(empresasExistentes.Select(e => e.Cnpj?.Replace(".", "").Replace("/", "").Replace("-", "") ?? "")).ToList();

            var empresasValidas = new List<Empresa>(empresasExistentes);

            if (cnpjNaoSalvos.Any())
            {
                var tarefas = cnpjNaoSalvos.Select(cnpj => _consultaService.ConsultarCnpjAsync(cnpj)).ToList();
                var empresas = await Task.WhenAll(tarefas);
                empresasValidas.AddRange(empresas.Where(e => e != null && e.Status != "ERROR").Select(e => e!));
            }

            if (!empresasValidas.Any())
            {
                ViewBag.Erro = "Nenhuma empresa válida encontrada na API";
                return View(_empresasSalvas);
            }

            return View("Resultado", empresasValidas);
        }

        [HttpPost]
        public IActionResult Salvar(string empresas, string formato)
        {
            if (string.IsNullOrEmpty(empresas))
            {
                TempData["Erro"] = "Nenhuma empresa para salvar";
                return View("Resultado", new List<Empresa>());
            }

            List<Empresa>? empresasList;
            try
            {
                empresasList = JsonConvert.DeserializeObject<List<Empresa>>(empresas);
            }
            catch (JsonException ex)
            {
                TempData["Erro"] = "Erro ao processar os dados das empresas: " + ex.Message;
                return View("Resultado", new List<Empresa>());
            }

            if (empresasList == null || !empresasList.Any())
            {
                TempData["Erro"] = "Nenhuma empresa válida para salvar";
                return View("Resultado", new List<Empresa>());
            }

            if (string.IsNullOrEmpty(formato))
            {
                TempData["Erro"] = "Formato de salvamento não especificado";
                return View("Resultado", empresasList);
            }

            switch (formato.ToLower())
            {
                case "site":
                    bool todasJaSalvas = true;
                    int empresasAdicionadas = 0;

                    foreach (var empresa in empresasList)
                    {
                        if (!_empresasSalvas.Any(e => e.Cnpj == empresa.Cnpj))
                        {
                            _empresasSalvas.Add(empresa);
                            empresasAdicionadas++;
                            todasJaSalvas = false;
                        }
                    }

                    if (todasJaSalvas)
                    {
                        TempData["Mensagem"] = "Todas as empresas já estão cadastradas no site!";
                    }
                    else if (empresasAdicionadas > 0)
                    {
                        TempData["Mensagem"] = "Empresa" + (empresasAdicionadas > 1 ? "s" : "") + " salva" + (empresasAdicionadas > 1 ? "s" : "") + " com sucesso!";
                    }
                    return RedirectToAction("Consultar");

                case "txt":
                    if (empresasList.Count == 1)
                    {
                        var empresa = empresasList[0];
                        string capitalSocialFormatado = GetCapitalSocialFormatado(empresa);
                        var txtContent = BuildTxtContent(empresa, capitalSocialFormatado);
                        var txtBytes = Encoding.UTF8.GetBytes(txtContent.ToString());
                        return File(txtBytes, "text/plain", "Dados_CNPJ.txt");
                    }
                    else
                    {
                        var txtContent = new StringBuilder();
                        foreach (var empresa in empresasList)
                        {
                            string capitalSocialFormatado = GetCapitalSocialFormatado(empresa);
                            txtContent.Append(BuildTxtContent(empresa, capitalSocialFormatado));
                            txtContent.AppendLine(new string('-', 50));
                        }
                        var txtBytes = Encoding.UTF8.GetBytes(txtContent.ToString());
                        return File(txtBytes, "text/plain", "Dados_CNPJs.txt");
                    }

                case "pdf":
                    using (var memoryStream = new MemoryStream())
                    {
                        var writer = new PdfWriter(memoryStream);
                        var pdf = new PdfDocument(writer);
                        var document = new iText.Layout.Document(pdf);

                        if (empresasList.Count == 1)
                        {
                            var empresa = empresasList[0];
                            string capitalSocialFormatado = GetCapitalSocialFormatado(empresa);
                            BuildPdfContent(document, empresa, capitalSocialFormatado);
                            document.Close();
                            return File(memoryStream.ToArray(), "application/pdf", "Dados_CNPJ.pdf");
                        }
                        else
                        {
                            foreach (var empresa in empresasList)
                            {
                                string capitalSocialFormatado = GetCapitalSocialFormatado(empresa);
                                BuildPdfContent(document, empresa, capitalSocialFormatado);
                                document.Add(new Paragraph(new string('-', 50)));
                            }
                            document.Close();
                            return File(memoryStream.ToArray(), "application/pdf", "Dados_CNPJs.pdf");
                        }
                    }

                default:
                    TempData["Erro"] = "Formato inválido: " + formato;
                    return View("Resultado", empresasList);
            }
        }

        private string GetCapitalSocialFormatado(Empresa empresa)
        {
            if (!string.IsNullOrEmpty(empresa.CapitalSocial) && decimal.TryParse(empresa.CapitalSocial, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal capitalSocial))
            {
                var cultureBR = new CultureInfo("pt-BR");
                return "R$ " + capitalSocial.ToString("N2", cultureBR);
            }
            return "N/A";
        }

        private StringBuilder BuildTxtContent(Empresa empresa, string capitalSocialFormatado)
        {
            var txtContent = new StringBuilder();
            txtContent.AppendLine("CNPJ: " + empresa.Cnpj);
            txtContent.AppendLine("Nome: " + empresa.Nome);
            txtContent.AppendLine("Abertura: " + empresa.Abertura);
            txtContent.AppendLine("Endereço: " + empresa.Logradouro + ", " + empresa.Numero + " - " + empresa.Bairro + ", " + empresa.Municipio + "/" + empresa.Uf + " - " + empresa.Cep);
            txtContent.AppendLine("Telefone: " + empresa.Telefone);
            txtContent.AppendLine("Email: " + empresa.Email);
            txtContent.AppendLine("Capital Social: " + capitalSocialFormatado);
            txtContent.AppendLine("Simples Nacional: " + empresa.SimplesNacional);
            txtContent.AppendLine("MEI: " + empresa.Mei);
            txtContent.AppendLine("Situação Cadastral: " + empresa.SituacaoCadastral);
            txtContent.AppendLine("Data da Situação Cadastral: " + empresa.DataSituaçãoCadastral);
            txtContent.AppendLine("Natureza Jurídica: " + empresa.NaturezaJurídica);
            txtContent.AppendLine("Atividade Econômica Primária: " + (empresa.AtividadePrincipal?.FirstOrDefault()?.Text ?? "N/A") + " (" + (empresa.AtividadePrincipal?.FirstOrDefault()?.Code ?? "N/A") + ")");
            txtContent.AppendLine("Atividades Econômicas Secundárias:");
            if (empresa.AtividadesSecundarias != null && empresa.AtividadesSecundarias.Any())
            {
                foreach (var atividade in empresa.AtividadesSecundarias)
                {
                    txtContent.AppendLine("- " + atividade.Text + " (" + atividade.Code + ")");
                }
            }
            else
            {
                txtContent.AppendLine("- Nenhuma atividade secundária");
            }
            txtContent.AppendLine("Quadro de Sócios:");
            if (empresa.Qsa != null && empresa.Qsa.Any())
            {
                foreach (var socio in empresa.Qsa)
                {
                    txtContent.AppendLine("- " + socio.Nome + " - " + socio.Qual);
                }
            }
            else
            {
                txtContent.AppendLine("- Nenhum sócio registrado");
            }
            return txtContent;
        }

        private void BuildPdfContent(iText.Layout.Document document, Empresa empresa, string capitalSocialFormatado)
        {
            document.Add(new Paragraph("CNPJ: " + empresa.Cnpj));
            document.Add(new Paragraph("Nome: " + empresa.Nome));
            document.Add(new Paragraph("Abertura: " + empresa.Abertura));
            document.Add(new Paragraph("Endereço: " + empresa.Logradouro + ", " + empresa.Numero + " - " + empresa.Bairro + ", " + empresa.Municipio + "/" + empresa.Uf + " - " + empresa.Cep));
            document.Add(new Paragraph("Telefone: " + empresa.Telefone));
            document.Add(new Paragraph("Email: " + empresa.Email));
            document.Add(new Paragraph("Capital Social: " + capitalSocialFormatado));
            document.Add(new Paragraph("Simples Nacional: " + empresa.SimplesNacional));
            document.Add(new Paragraph("MEI: " + empresa.Mei));
            document.Add(new Paragraph("Situação Cadastral: " + empresa.SituacaoCadastral));
            document.Add(new Paragraph("Data da Situação Cadastral: " + empresa.DataSituaçãoCadastral));
            document.Add(new Paragraph("Natureza Jurídica: " + empresa.NaturezaJurídica));
            document.Add(new Paragraph("Atividade Econômica Primária: " + (empresa.AtividadePrincipal?.FirstOrDefault()?.Text ?? "N/A") + " (" + (empresa.AtividadePrincipal?.FirstOrDefault()?.Code ?? "N/A") + ")"));
            document.Add(new Paragraph("Atividades Econômicas Secundárias:"));
            if (empresa.AtividadesSecundarias != null && empresa.AtividadesSecundarias.Any())
            {
                foreach (var atividade in empresa.AtividadesSecundarias)
                {
                    document.Add(new Paragraph("- " + atividade.Text + " (" + atividade.Code + ")"));
                }
            }
            else
            {
                document.Add(new Paragraph("- Nenhuma atividade secundária"));
            }
            document.Add(new Paragraph("Quadro de Sócios:"));
            if (empresa.Qsa != null && empresa.Qsa.Any())
            {
                foreach (var socio in empresa.Qsa)
                {
                    document.Add(new Paragraph("- " + socio.Nome + " - " + socio.Qual));
                }
            }
            else
            {
                document.Add(new Paragraph("- Nenhum sócio registrado"));
            }
        }

        public IActionResult EmpresasCadastradas()
        {
            return View(_empresasSalvas);
        }
    }
}