# Consulta CNPJ
Projeto desenvolvido em C# com objetivo de permitir que o usuário consulte informações de empresas a partir de CNPJs válidos, utilizando a API da ReceitaWS, exibindo os dados retornados e oferecendo opções para salvá-los em diferentes formatos.


## Funcionalidades

1. **Consulta de CNPJs**:
   - O usuário pode inserir até 3 CNPJs por vez, separados por vírgula (ex.: `14.380.200/0001-21, 33.000.167/0001-01`).
   - A aplicação utiliza a API da ReceitaWS para consultar os dados das empresas e exibe os resultados na página `Resultado`.

2. **Limitação de Consultas**:
   - Devido ao limite da API da ReceitaWS (3 consultas por minuto na versão gratuita), a aplicação restringe a entrada a no máximo 3 CNPJs por vez.
   - Validação no front-end (JavaScript) desativa o botão "Consultar" se mais de 3 CNPJs forem inseridos.
   - Validação no back-end (C#) também impede consultas excedentes.

3. **Exibição de Dados**:
   - Os dados retornados pela API (nome, endereço, capital social, situação cadastral, atividades econômicas, quadro de sócios, etc.) são exibidos de forma organizada na página `Resultado`.

4. **Salvamento de Dados**:
   - **Salvar no Site**: Os dados são armazenados em memória (lista estática) e exibidos na seção "Empresas Cadastradas" na página inicial.
   - **Salvar como TXT**: Exporta os dados em um arquivo de texto (`Dados_CNPJ.txt` ou `Dados_CNPJs.txt` para múltiplas empresas).
   - **Salvar como PDF**: Exporta os dados em um arquivo PDF (`Dados_CNPJ.pdf` ou `Dados_CNPJs.pdf`) utilizando a biblioteca **iText7**.

5. **Extras Implementados**:
   - Uso de **métodos assíncronos** para consultas à API.
   - **Paralelismo** com `Task.WhenAll` para consultar múltiplos CNPJs simultaneamente.
   - Interface com validações em JavaScript e CSS personalizado.


## Estrutura do Projeto

- **Controllers**:
  - `EmpresaController`: Gerencia as ações de consulta, exibição e salvamento dos dados.
- **Models**:
  - `Empresa`: Representa os dados retornados pela API da ReceitaWS.
  - `Atividade`: Modelo para atividades econômicas (primária e secundárias).
  - `Socio`: Modelo para o quadro de sócios.
- **Services**:
  - `ConsultaCnpjService`: Realiza chamadas assíncronas à API da ReceitaWS.
- **Views**:
  - `Consultar.cshtml`: Tela inicial para inserir CNPJs e exibir empresas salvas.
  - `Resultado.cshtml`: Exibe os dados consultados e opções de salvamento.
  - `Layout.cshtml`: Layout base com header, footer e estilos CSS.
- **Dependências**:
  - `HttpClient` para chamadas HTTP.
  - Pacotes NuGet: `Newtonsoft.Json`, `itext7`.


## Funcionamento da API da ReceitaWS

A aplicação utiliza a API da ReceitaWS para consultar dados de empresas com base em CNPJs. Detalhes importantes:

- **Endpoint**: `https://receitaws.com.br/v1/cnpj/{cnpj}`
- **Limite de Consultas**:
  - Versão gratuita: **3 consultas por minuto**.
  - Após atingir o limite, é necessário aguardar **60 segundos** para novas consultas, ou a API retornará um erro.
- **Resposta**:
  - Retorna dados em formato JSON com informações detalhadas da empresa.


## Tecnologias Utilizadas

- **Framework**: ASP.NET Core MVC
- **Versão do .NET**: 9.0.201
- **Linguagem**: C#
- **IDE**: Visual Studio 2022
- **Bibliotecas**:
  - **Newtonsoft.Json**: Para serialização e desserialização de JSON.
  - **iText7**: Para geração de arquivos PDF.
- **Front-end**: HTML, CSS, JavaScript
- **API Externa**: ReceitaWS (`https://receitaws.com.br/v1/cnpj/{cnpj}`)
