using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Globalization;

namespace Reflector
{
    public static class Parser
    {
        #region Editor de Relatório
        public string ImprimirModeloRelatorio<T>(T obj, int idRelatorio)
        {

            //var textoPadrao = serviceTextoPadraoRelatorio.GetTextosPadroes().ToList();
            //var textoParaImpressao = repositorio.GetAll().Where(r => r.Id == idRelatorio).ToList();
            //Relatorio relatorioImpressao = repositorio.Get(r => r.Id == idRelatorio);
            //string agatml = DefinirMargensTamanhoDocumento(relatorioImpressao);
            //string html = relatorioImpressao.Description;
            string html = "";
            Regex rgx = new Regex("\\n|\\r");
            html = rgx.Replace(html, "");

            string pattern = "(</[pd]>)";
            dynamic objetoImpressao = obj;
            html = DefinirFormatacoes(objetoImpressao, html);
            if (html.IndexOf("CONDICIONAL(") > 0)
            {
                html = VerificarSessao(objetoImpressao, html);
            }
            string entidade = objetoImpressao.GetType().Name;
            List<string> listaEntidades = new List<string>();
            ListarDetalhe(objetoImpressao, ref html, ref listaEntidades);

            string[] documentoHtml = Regex.Split(html, pattern);
            Dictionary<int, string> dicionarioDocumento = new Dictionary<int, string>();
            for (var i = 0; i < documentoHtml.Length; i++)
            {
                if (!string.IsNullOrEmpty(documentoHtml[i]))
                    dicionarioDocumento.Add(i, documentoHtml[i]);
            }
            InserirValores(objetoImpressao, ref dicionarioDocumento, ref listaEntidades);

            html = String.Join("", dicionarioDocumento.Select(r => r.Value).ToArray());
            html = VerificarSessao(objetoImpressao, html);
            string substituicao = "";

            rgx = new Regex(@"</d>|<p></p>|<br />");
            html = rgx.Replace(html, substituicao);
            rgx = new Regex(@"&lt;@Detalhe_Fim\(.*?\)&gt;|");
            html = rgx.Replace(html, substituicao);
            //   rgx = new Regex(@"<[^\/>][^>].*><\/[^>]+>|<p></p>");
            //   html = rgx.Replace(html, substituicao);


            rgx = new Regex(@"<[^\/>][^>]*><\/[^>]+>");
            while (rgx.IsMatch(html))
            {
                html = rgx.Replace(html, substituicao);
            }
            return html + "<div style='height:100%; width: 100%; display:inline-block;  margin: 0px;  position: relative;background:url(http://i.piccy.info/i7/c7a432fe0beb98a3a66f5b423b430423/1-5-1789/1066503/lol.png);background-size:100% 100%;' class='line'></div>"; ;
        }

        private string DefinirMargensTamanhoDocumento(dynamic relatorioImpressao)
        {
            string divDocumento = "<div style='height:" + relatorioImpressao.Height +
                                             " width:" + relatorioImpressao.Width +
                                             " margin-top:" + relatorioImpressao.TopMargin +
                                             " margin-bottom:" + relatorioImpressao.BottomMargin +
                                             " margin-left:" + relatorioImpressao.LeftMargin +
                                             " margin-right:" + relatorioImpressao.RightMargin + "'>";


            return divDocumento;
        }

        private string VerificarSessao<T>(T obj, string html) where T : class
        {
            var regexTags = new Regex("&lt;@Sessao_Inicio.*?&lt;@Sessao_Fim&gt;");
            foreach (Match tag in regexTags.Matches(html))
            {
                string sessao = ListarSessoes(obj, tag.ToString());
                html = html.Replace(tag.ToString(), sessao);
            }
            return html;
        }
        public static string ListarSessoes<T>(T obj, string sessao) where T : class
        {
            string[] sessaoDividida = Regex.Split(sessao, @"(\[.*?\])(.*)(\{.*?\})");
            int startIndex = sessaoDividida[1].IndexOf("(");
            int endIndex = sessaoDividida[1].IndexOf(")");
            string funcao = sessaoDividida[1].Substring(1, startIndex - 1);
            string valor = sessaoDividida[1].Substring(startIndex + 1, endIndex - startIndex - 1);

            if (funcao.Equals("CONDICIONAL"))
            {
                string resultado = ListarCondicionais(obj, sessao);
                if (resultado.ToLower().Equals("true"))
                {
                    sessao = sessaoDividida[3].Substring(sessaoDividida[3].IndexOf("{") + 1, sessaoDividida[3].IndexOf("}") - sessaoDividida[3].IndexOf("{") - 1);
                }
                else
                {
                    sessao = string.Empty;
                }
            }


            var rgx = new Regex("{|}");
            return rgx.Replace(sessao, "");


        }
        private static string DefinirFormatacoes<T>(T obj, string html) where T : class
        {
            var rgx = new Regex(@"CURRENT_DATE\(\)");
            foreach (Match item in rgx.Matches(html))
            {
                html = rgx.Replace(html, DateTime.Now.ToString());
            }
            rgx = new Regex(@"FORMAT_(.*?)\((.*?)\-(.*?)\)");
            string[] splitTags = rgx.Split(html);
            if (splitTags.Length > 1)
                html = DefinirFormatacao(splitTags[1], ref splitTags[3], splitTags[2]);

            return html;
        }

        private static string ListarCondicionais<T>(T obj, string sessao) where T : class
        {
            bool? result = null;

            if (Regex.IsMatch(sessao, "!MAIORIGUAL|!MAIOR|!MENORIGUAL|!MENOR|!IGUAL|!DIFERENTE"))
            {
                Dictionary<int, string> dic = new Dictionary<int, string>();
                string[] condicao = Regex.Split(sessao, @"CONDICIONAL\((.*?)\)");
                dic.Add(1, condicao[1]);

                List<string> listaEntidades = new List<string>();
                string ent = string.Empty;

                InserirValores(obj, ref dic, ref listaEntidades);
                foreach (var condicaoSubstituida in dic)
                {
                    ent = condicaoSubstituida.Value;
                }
                sessao = sessao.Replace(condicao[1], ent);
                string[] sessaoDividida = Regex.Split(sessao, @"&lt;@Sessao_Inicio&gt;\[CONDICIONAL\((.*?)\)\]|(.*?)@RETORNO_SESSAO\{.*?\}|(.*?)&lt;@Sessao_Fim&gt;");
                string pattern = @"(!AND|!OR|!NOT|!MAIORIGUAL|!MAIOR|!MENORIGUAL|!MENOR|!IGUAL|!DIFERENTE)";

                string[] operadoresAritimeticos = Regex.Split(sessaoDividida[1], pattern);
                for (var x = 0; x < operadoresAritimeticos.Length; x++)
                {
                    if (!operadoresAritimeticos[x].StartsWith("!"))
                    {
                        if (operadoresAritimeticos[x].IndexOf("FORMAT") > 0)
                        {
                            int startIndex = operadoresAritimeticos[x].IndexOf("(");
                            int endIndex = operadoresAritimeticos[x].IndexOf(")");
                            string funcao = operadoresAritimeticos[x].Substring(operadoresAritimeticos[x].IndexOf("F"), startIndex - operadoresAritimeticos[x].IndexOf("F"));
                            string valor = operadoresAritimeticos[x].Contains("CURRENT_DATE") ? DateTime.Now.ToShortDateString() : operadoresAritimeticos[x].Substring(startIndex + 1, endIndex - startIndex - 1);
                            string fomarto = operadoresAritimeticos[x].Substring(operadoresAritimeticos[x].IndexOf('{'), operadoresAritimeticos[x].IndexOf('}') - operadoresAritimeticos[x].IndexOf('{') + 1);
                            operadoresAritimeticos[x] = DefinirFormatacao(funcao, ref fomarto, valor);
                        }
                    }
                }

                StringBuilder sb = new StringBuilder();
                for (var x = 0; x < operadoresAritimeticos.Length; x++)
                {
                    if (Regex.IsMatch(operadoresAritimeticos[x], "!MAIORIGUAL|!MAIOR|!MENORIGUAL|!MENOR|!IGUAL|!DIFERENTE"))
                    {
                        FazerOperacoes(operadoresAritimeticos[x], operadoresAritimeticos[x - 1], operadoresAritimeticos[x + 1], ref result);
                        sb.Append(result.ToString());
                    }
                    else if (Regex.IsMatch(operadoresAritimeticos[x], "!NOT|!AND|!OR"))
                    {
                        sb.Append(operadoresAritimeticos[x]);
                    }
                }
                ListarCondicionais(obj, sb.ToString());
            }
            else
            {

                string[] operadoresLogicos = Regex.Split(sessao, "(!AND)|(!OR)|(!NOT)");
                for (var x = 0; x < operadoresLogicos.Length; x++)
                {
                    if (Regex.IsMatch(operadoresLogicos[x], "!AND"))
                    {
                        FazerOperacoes(operadoresLogicos[x], operadoresLogicos[x - 1], operadoresLogicos[x + 1], ref result);
                    }
                }
            }
            return result.ToString();
        }

        public static void ListarDetalhe<T>(T obj, ref string html, ref List<string> listaDetalhe) where T : class
        {

            string[] documentoDetalhe = new string[4];
            var regexTags = new Regex(@"(&lt;@Detalhe_Inicio)(.*?)(&lt;@Detalhe_Fim\(.*?\)&gt;)", RegexOptions.Singleline);
            string[] documentoHtml = regexTags.Split(html);
            string pattern = "(@Detalhe_Inicio)(.*?)(@Detalhe_Fim)";
            string[] documentoHtmle = Regex.Split(html, pattern);
            if (documentoHtml.Length > 1)
            {
                documentoHtml = documentoHtml.Where(r => !string.IsNullOrEmpty(r)).ToArray();
                for (var x = 0; x < documentoHtml.Length - 1; x += 4)
                {
                    Array.Copy(documentoHtml, x, documentoDetalhe, 0, 4);
                    string detalhe = string.Join("", documentoDetalhe);
                    listaDetalhe.Add(FuncaoDetalhe(obj, detalhe));
                }

            }
            if (listaDetalhe.Count() > 0)
            {
                html = string.Join("", listaDetalhe);
                html = html + documentoHtml[documentoHtml.Length - 1];
            }
        }

        private static string FuncaoDetalhe(object obj, string detalhe)
        {
            bool temTabela = Regex.IsMatch(detalhe, @"table");
            bool temLista = Regex.IsMatch(detalhe, @"<ul");
            string[] splitTags = new string[4];
            string estiloCSS = "";
            string tipoDetalhe = "";
            string resultado = string.Empty;
            bool teste = false;
            if (temTabela)
            {
                splitTags = Regex.Split(detalhe, @"<table(.*?)><tbody>(<tr><th>.*?</th></tr>)(.*?)</tbody></table>");
                if (Regex.IsMatch(splitTags[1], @"style|border"))
                {
                    estiloCSS = splitTags[1];
                    splitTags[1] = string.Empty;

                }

                var cabecalhoTable = splitTags[2];
                splitTags[2] = string.Empty;
                splitTags[0] = splitTags[0].Substring(0, splitTags[0].IndexOf("<p>&lt;@Detalhe_Inicio")) + "!sub!" + splitTags[0].Substring(splitTags[0].IndexOf("<p>&lt;@Detalhe_Inicio"));
                detalhe = string.Join("", splitTags);
                detalhe = DuplicarLinhasDetalhe(obj, detalhe, ref tipoDetalhe, ref teste, ref resultado);
                detalhe = Regex.Replace(detalhe, "!sub!", "<table" + estiloCSS + "><tbody>" + cabecalhoTable) + "</tbody></table>";
            }
            else if (temLista)
            {
                splitTags = Regex.Split(detalhe, @"<ul(.*?)>(.*?)</ul>");
                if (Regex.IsMatch(splitTags[1], @"style"))
                {
                    estiloCSS = splitTags[1];
                    splitTags[1] = string.Empty;
                }
                detalhe = String.Join("", splitTags);
                // splitTags = DuplicarLinhasDetalhe(obj, detalhe, ref tipoDetalhe);
                splitTags[1] = "<table" + estiloCSS + ">" + splitTags[1];
                splitTags[3] = "</table>";
            }
            else
            {


                detalhe = DuplicarLinhasDetalhe(obj, detalhe, ref tipoDetalhe, ref teste, ref resultado);
            }

            //  detalhe = string.Join("", splitTags);

            string patterno = "(" + tipoDetalhe + ")&gt;</p>";
            return detalhe = detalhe.Replace(patterno, string.Empty);

        }
        private static string DuplicarLinhasDetalhe(object obj, string detalhe, ref string tipoDetalhe, ref bool isFinalizou, ref string resultado)
        {
            int numeroRepeticoes = 0;
            if (isFinalizou)
            {
                return string.IsNullOrEmpty(resultado) ? detalhe : resultado;
            }
            string[] documentoDetalhe = new string[20];
            string[] documentoFinal = new string[20];


            var regexTags = new Regex(@"(&lt;@Detalhe_Inicio)(.*?)(&lt;@Detalhe_Fim\(.*?\)&gt;)", RegexOptions.Singleline);
            string[] documentoHtml = regexTags.Split(detalhe);

            //string pattern = @"(&lt;@Detalhe_Inicio)(.*?)(&lt;@Detalhe_Fim.*?&gt;)";
            //string[] documentoHtmle = Regex.Split(detalhe, pattern);
            foreach (PropertyInfo info in obj.GetType().GetProperties())
            {
                tipoDetalhe = documentoHtml[2].Substring(1, documentoHtml[2].IndexOf(')') - 1);
                dynamic o = info.GetValue(obj, null);
                if (o != null && o.GetType().IsGenericType)
                {
                    bool isLista = false;
                    if (info.PropertyType.GenericTypeArguments[0].Name.Equals(tipoDetalhe))
                    {
                        if (!isFinalizou)
                        {
                            isLista = true;
                            numeroRepeticoes = o.Count;
                            if (numeroRepeticoes > 1)
                            {
                                Array.Copy(documentoHtml, 0, documentoDetalhe, 0, 4);
                                for (var x = 1; x <= numeroRepeticoes; x++) { documentoDetalhe[x] = documentoDetalhe[2] + "</d>"; }
                                documentoDetalhe = documentoDetalhe.Where(r => !string.IsNullOrEmpty(r)).ToArray();
                                Array.Copy(documentoDetalhe, 0, documentoFinal, 0, documentoDetalhe.Length);
                                Array.Copy(documentoHtml, 4, documentoFinal, documentoDetalhe.Length, documentoHtml.Length - 4);
                                documentoFinal = documentoFinal.Where(r => !string.IsNullOrEmpty(r)).ToArray();
                                resultado = string.Join("", documentoFinal);
                                isFinalizou = true;
                            }
                        }

                    }
                    if (!isLista)
                        foreach (var objeto in o)
                        {
                            foreach (PropertyInfo infoGeneric in objeto.GetType().GetProperties())
                            {
                                if (objeto != null && objeto.GetType().IsClass && objeto.GetType() != typeof(string))
                                {
                                    //  entidade = objeto.GetType().Name;
                                    DuplicarLinhasDetalhe(objeto, detalhe, ref tipoDetalhe, ref isFinalizou, ref resultado);
                                }

                            }
                        }

                }

            }

            return string.IsNullOrEmpty(resultado) ? detalhe : resultado; ;
        }

        public static void InserirValores<T>(T obj, ref Dictionary<int, string> dic, ref List<string> entidadesAdicionadas) where T : class
        {

            try
            {
                Type tipo = obj.GetType();
                PropertyInfo[] propriedades = tipo.GetProperties();
                string sexo = "";
                string campo = "";
                List<string> valoresEmLista = new List<string>();
                if (!entidadesAdicionadas.Contains(tipo.Name) && !tipo.Name.Equals("Usuario"))
                {

                    foreach (PropertyInfo info in propriedades)
                    {

                        if (info.CanRead)
                        {
                            entidadesAdicionadas.Add(tipo.Name);

                            dynamic o = obj.GetType() != typeof(string) && obj.GetType() != typeof(int) ? info.GetValue(obj, null) : null;

                            if (o != null && !o.GetType().IsGenericType && (o.GetType() == typeof(string) || !o.GetType().IsClass))
                            {
                                {
                                    campo = String.Format("&lt;#{0}.{1}#&gt;", tipo.Name, info.Name);
                                }

                                sexo = info.Name.ToLower().Equals("sexo") ? o.ToString().ToLower() : string.Empty;
                                string valor = o.ToString();
                                var listaTags = dic.Select(r => r);
                                foreach (var linha in listaTags)
                                {
                                    if (linha.Value.IndexOf(campo) != -1)
                                    {
                                        dic[linha.Key] = linha.Value.Replace(campo, valor);
                                        break;
                                    }
                                }


                            }
                            else if (o != null && o.GetType().IsGenericType && !o.GetType().IsAbstract)
                            {

                                Type propertyType = info.PropertyType.GenericTypeArguments[0];
                                var instanciaObj = Activator.CreateInstance(propertyType);
                                Type instanciaObjTipo = instanciaObj.GetType();
                                if (!o.GetType().IsNotPublic && o.Count > 0)
                                {
                                    foreach (var objeto in o)
                                    {
                                        foreach (PropertyInfo infoGeneric in objeto.GetType().GetProperties())
                                        {
                                            if (objeto != null && objeto.GetType().IsClass && objeto.GetType() != typeof(string) && o.Count < 2)
                                            {
                                                InserirValores(objeto, ref dic, ref entidadesAdicionadas);
                                            }
                                            else
                                            {
                                                campo = String.Format("&lt;#{0}.{1}#&gt;", info.PropertyType.GenericTypeArguments[0].Name, infoGeneric.Name);
                                                string valor = (objeto.GetType().GetProperty(infoGeneric.Name).GetValue(objeto, null) != null ? objeto.GetType().GetProperty(infoGeneric.Name).GetValue(objeto, null).ToString() : null);
                                                var listaFormatada = dic.Where(r => r.Value.IndexOf(campo) != -1).Select(r => r).ToList();

                                                foreach (var linhaDetalhe in listaFormatada)
                                                {
                                                    if (listaFormatada.Count != o.Count)
                                                    {
                                                        var replaceDicionario = listaFormatada.Where(r => !r.Value.StartsWith("<d>")).Select(r => r).ToList();
                                                        foreach (var linha in replaceDicionario)
                                                        {
                                                            dic[linha.Key] = linha.Value.Replace(campo, valor);
                                                        }

                                                    }
                                                    if (linhaDetalhe.Value.IndexOf(campo) != -1)
                                                    {
                                                        dic[linhaDetalhe.Key] = linhaDetalhe.Value.Replace(campo, valor);
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (o.Count > 0)
                                {
                                    InserirValores(o, ref dic, ref entidadesAdicionadas);
                                }

                            }
                            else if (o != null && o.GetType().IsClass)
                            {
                                foreach (PropertyInfo infoGeneric in o.GetType().GetProperties())
                                {
                                    if (infoGeneric.GetType().IsClass && infoGeneric.GetType() != typeof(string))
                                    {
                                        InserirValores(o, ref dic, ref entidadesAdicionadas);
                                    }


                                    if (o != null)
                                    {
                                        campo = String.Format("&lt;#{0}.{1}#&gt;", infoGeneric.DeclaringType.Name, infoGeneric.Name);


                                        string valor = (o.GetType().GetProperty(infoGeneric.Name).GetValue(o, null) != null ? o.GetType().GetProperty(infoGeneric.Name).GetValue(o, null).ToString() : null);
                                        var listaTags = dic.Select(r => r);
                                        foreach (var linha in listaTags)
                                        {
                                            if (linha.Value.IndexOf(campo) != -1)
                                            {
                                                dic[linha.Key] = linha.Value.Replace(campo, valor);
                                                break;
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(sexo))
                {
                    SubstituicaoGenero(ref dic, sexo);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
        private static void SubstituicaoGenero(ref Dictionary<int, string> dic, string sexo)
        {
            if (sexo.Equals("feminino"))
            {
                var dicionario = dic;
                foreach (var linha in dicionario)
                {
                    var rgx = new Regex("([aeiou]+\b)(?!:)");

                    var tagGenero = linha.Value.Substring(linha.Value.IndexOf("<#o_a(") + 6, linha.Value.IndexOf(")a_o#>") - 6);
                    if (!rgx.IsMatch(tagGenero))
                    {
                        dic[linha.Key] = linha.Value.Replace(tagGenero.Substring(linha.Value.IndexOf(tagGenero) - 6, tagGenero.Length + 11), tagGenero.Replace(tagGenero.Substring(tagGenero.Length - 1), "a"));
                    }
                    else
                    {
                        dic[linha.Key] = linha.Value.Replace(tagGenero.Substring(linha.Value.IndexOf(tagGenero) - 6, tagGenero.Length + 11), tagGenero + "a");
                    }

                }
            }
            else
            {
                var dicionario = dic;
                foreach (var linha in dicionario)
                {
                    var rgx = new Regex("([aeiou]+\b)(?!:)");

                    var tagGenero = linha.Value.Substring(linha.Value.IndexOf("<#o_a(") + 6, linha.Value.IndexOf(")a_o#>") - 1);
                    if (!rgx.IsMatch(tagGenero))
                    {
                        dic[linha.Key] = linha.Value.Replace(tagGenero.Substring(linha.Value.IndexOf(tagGenero) - 6, tagGenero.Length + 11), tagGenero.Replace(tagGenero.Substring(tagGenero.Length - 1), "a"));
                    }
                    else
                    {
                        dic[linha.Key] = linha.Value.Replace(tagGenero.Substring(linha.Value.IndexOf(tagGenero) - 6, tagGenero.Length + 11), tagGenero + "a");
                    }

                }
            }
        }


        #endregion

        public static string DefinirFormatacao(string funcao, ref string formato, string valor)
        {
            switch (funcao)
            {
                case "INTEIRO":
                    {
                        formato = valor.ToString().PadLeft(formato.Substring(1, formato.Length - 2).Length, '0');
                        break;
                    }
                case "CURRENCY":
                    {
                        formato = String.Format("{0:c}", Double.Parse(valor));
                        break;
                    }
                case "DATE":
                    {
                        FormatarData(ref formato, valor);
                        break;
                    }
                case "EXTENSO":
                    {
                        FormatExtenso(ref formato, valor);
                        break;
                    }
            }
            return formato;

        }

        private static void FormatExtenso(ref string formato, string valor)
        {
            string tipo = formato.Substring(1, formato.Length - 2);
            switch (tipo)
            {
                case "booleano":
                    {
                        formato = valor.Equals("1") | valor.Equals("true") ? "sim" : valor.Equals("0") | valor.Equals("false") ? "não" : formato;
                        break;
                    }
                case "BOOLEANO":
                    {
                        formato = valor.Equals("1") | valor.Equals("true") ? "Sim" : valor.Equals("0") | valor.Equals("false") ? "Não" : formato;
                        break;
                    }
                case "inteiro":
                    {
                        formato = toExtenso(Int32.Parse(valor), TabelaFormatacao.INTEIRO).ToLower();
                        break;
                    }
                case "INTEIRO":
                    {
                        formato = toExtenso(Int32.Parse(valor), TabelaFormatacao.INTEIRO);
                        string reposicao = formato.Substring(0, 1).ToUpper();
                        formato = reposicao + formato.Substring(1, formato.Length - 1);
                        break;
                    }
                case "currency":
                    {
                        formato = toExtenso(decimal.Parse(valor), TabelaFormatacao.CURR);
                        break;
                    }
                case "CURRENCY":
                    {
                        formato = toExtenso(decimal.Parse(valor), TabelaFormatacao.CURR);
                        string reposicao = formato.Substring(0, 1).ToUpper();
                        formato = reposicao + formato.Substring(1, formato.Length - 1);
                        break;
                    }
            }
        }

        private static void FormatarData(ref string formato, string valor)
        {
            var pattern = new Regex("[A-Za-z]?[A-Za-z]?[A-Za-z]?[A-Za-z]?[A-Za-z]");
            DateTime data = DateTime.Parse(valor);
            CultureInfo culture = new CultureInfo("pt-BR");
            DateTimeFormatInfo dtfi = culture.DateTimeFormat;
            int dia = data.Day;
            int mes = data.Month;
            int ano = data.Year;
            string diaFormatado = string.Empty;
            string mesFormatado = string.Empty;
            string anoFormatado = string.Empty;
            string diasemana = culture.TextInfo.ToTitleCase(dtfi.GetDayName(data.DayOfWeek));
            foreach (Match item in pattern.Matches(formato))
            {
                string unidade = item.ToString();
                TabelaFormatacao tipoFormatacao;
                Enum.TryParse(unidade, out tipoFormatacao);
                // Verificação necessária apenas para Meses, pois anos e dias são valores numéricos 
                if (unidade.ToLower().StartsWith("m") && unidade.Count() == 5)
                {
                    mesFormatado = unidade.StartsWith("M") ? culture.TextInfo.ToTitleCase(dtfi.GetMonthName(data.Month)).Substring(0, 3) : unidade.StartsWith("m") ? culture.TextInfo.ToTitleCase(dtfi.GetMonthName(data.Month)).ToLower().Substring(0, 3) : string.Empty;
                }
                if (unidade.ToLower().StartsWith("m") && unidade.Count() == 4)
                {
                    mesFormatado = unidade.StartsWith("M") ? culture.TextInfo.ToTitleCase(dtfi.GetMonthName(data.Month)) : unidade.StartsWith("m") ? culture.TextInfo.ToTitleCase(dtfi.GetMonthName(data.Month)).ToLower() : string.Empty;
                }
                else if (unidade.Count() == 3)
                {
                    diaFormatado = toExtenso(dia, tipoFormatacao);
                    mesFormatado = toExtenso(mes, tipoFormatacao);
                    anoFormatado = toExtenso(ano, tipoFormatacao);
                }
                else
                {
                    if (unidade.ToLower().StartsWith("d"))
                    {
                        diaFormatado = Format(dia, tipoFormatacao);
                    }
                    else if (unidade.ToLower().StartsWith("m"))
                    {
                        mesFormatado = Format(mes, tipoFormatacao);
                    }
                    else if (unidade.ToLower().StartsWith("a"))
                    {
                        anoFormatado = Format(ano, tipoFormatacao);
                    }
                }
                formato = formato.Replace("[" + unidade + "]", unidade.ToLower().StartsWith("d") ? diaFormatado : unidade.ToLower().StartsWith("m") ? mesFormatado : unidade.ToLower().StartsWith("a") ? anoFormatado : formato);
            }


        }
        public static void FazerOperacoes(string operador, string primeiroArgumento, string segundoArgumento, ref bool? resultado)
        {
            // Variáveis iniciadas pois todos os valores que vem são do tipo string do modelo html
            int primeiroArgumentoInteiro = -1;
            int segundoArgumentoInteiro = -1;
            bool? primeiroArgumentoBooleano = null;
            bool? segundoArgumentoBooleano = null;
            string primeiroArgumentoString = string.Empty;
            string segundoArgumentoString = string.Empty;
            // Nestes regex ele testa o valor e faz o parse dos argumentos
            if (Regex.IsMatch(primeiroArgumento, "(T|t)rue|(F|f)alse") && Regex.IsMatch(segundoArgumento, "(T|t)rue|(F|f)alse"))
            {
                primeiroArgumentoBooleano = bool.Parse(primeiroArgumento);
                segundoArgumentoBooleano = bool.Parse(segundoArgumento);
            }
            if (Regex.IsMatch(primeiroArgumento, "[A-Za-z]") && Regex.IsMatch(segundoArgumento, "[A-Za-z]"))
            {
                primeiroArgumentoString = primeiroArgumento.Substring(primeiroArgumento.IndexOf("{") + 1, primeiroArgumento.IndexOf("}") - primeiroArgumento.IndexOf("{") - 1); ;
                segundoArgumentoString = segundoArgumento.Substring(segundoArgumento.IndexOf("{") + 1, segundoArgumento.IndexOf("}") - segundoArgumento.IndexOf("{") - 1);
            }
            if (Regex.IsMatch(primeiroArgumento, "[0-9]") && Regex.IsMatch(segundoArgumento, "[0-9]"))
            {
                primeiroArgumentoInteiro = Int32.Parse(primeiroArgumento.Substring(primeiroArgumento.IndexOf("{") + 1, primeiroArgumento.IndexOf("}") - primeiroArgumento.IndexOf("{") - 1));
                segundoArgumentoInteiro = Int32.Parse(segundoArgumento.Substring(segundoArgumento.IndexOf("{") + 1, segundoArgumento.IndexOf("}") - segundoArgumento.IndexOf("{") - 1));
            }
            // switch para realizar as funções condicionais
            switch (operador)
            {
                case "!MAIORIGUAL":
                    {
                        if (primeiroArgumentoInteiro > -1)
                        {
                            resultado = primeiroArgumentoInteiro >= segundoArgumentoInteiro;
                        }
                        break;
                    }
                case "!MENORIGUAL":
                    {
                        if (primeiroArgumentoInteiro > -1)
                        {
                            resultado = primeiroArgumentoInteiro <= segundoArgumentoInteiro;
                        }
                        break;
                    }
                case "!MAIOR":
                    {
                        if (primeiroArgumentoInteiro > -1)
                        {
                            resultado = primeiroArgumentoInteiro > segundoArgumentoInteiro;
                        }
                        break;
                    }
                case "!MENOR":
                    {
                        if (primeiroArgumentoInteiro > -1)
                        {
                            resultado = primeiroArgumentoInteiro < segundoArgumentoInteiro;
                        }
                        break;
                    }
                case "!IGUAL":
                    {
                        if (primeiroArgumentoInteiro > -1)
                        {
                            resultado = primeiroArgumentoInteiro == segundoArgumentoInteiro;
                        }
                        else if (!string.IsNullOrEmpty(primeiroArgumentoString))
                        {
                            if (segundoArgumentoString.Equals("!NULL"))
                            {
                                resultado = string.IsNullOrEmpty(primeiroArgumentoString);
                            }
                            else
                            {
                                resultado = primeiroArgumentoString.Equals(segundoArgumentoString);
                            }
                        }
                        else
                        {
                            resultado = false;
                        }
                        break;
                    }
                case "!DIFERENTE":
                    {
                        if (primeiroArgumentoInteiro > -1)
                        {
                            resultado = primeiroArgumentoInteiro != segundoArgumentoInteiro;
                        }
                        else if (!string.IsNullOrEmpty(primeiroArgumentoString))
                        {
                            if (segundoArgumentoString.Equals("!NULL"))
                            {
                                resultado = !string.IsNullOrEmpty(primeiroArgumentoString);
                            }
                            else
                            {
                                resultado = !primeiroArgumentoString.Equals(segundoArgumentoString);
                            }
                        }
                        else
                        {
                            resultado = false;
                        }
                        break;
                    }
                case "!AND":
                    {
                        if (primeiroArgumentoBooleano != null && segundoArgumentoBooleano != null)
                        {
                            resultado = ((bool)primeiroArgumentoBooleano) && ((bool)segundoArgumentoBooleano);
                        }
                        break;
                    }
                case "!OR":
                    {

                        resultado = ((bool)primeiroArgumentoBooleano) && ((bool)segundoArgumentoBooleano);

                        break;
                    }
                case "!NOT":
                    {
                        if (primeiroArgumentoInteiro > -1)
                        {
                            resultado = primeiroArgumentoInteiro != segundoArgumentoInteiro;
                        }
                        else
                        {
                            resultado = primeiroArgumentoBooleano != segundoArgumentoBooleano;
                        }
                        break;
                    }
            }
        }


       


        public static string toExtenso(decimal valor, TabelaFormatacao tipo)
        {
            if (valor <= 0 | valor >= 1000000000000000)
                return "Valor não suportado pelo sistema.";
            else
            {
                string strValor = valor.ToString("000000000000000.00");
                string valorExtenso = string.Empty;

                for (int i = 0; i <= 15; i += 3)
                {
                    valorExtenso += EscreveParte(Convert.ToDecimal(strValor.Substring(i, 3)));
                    if (i == 0 & valorExtenso != string.Empty)
                    {
                        if (Convert.ToInt32(strValor.Substring(0, 3)) == 1)
                            valorExtenso += " trilhão" + ((Convert.ToDecimal(strValor.Substring(3, 12)) > 0) ? " e " : string.Empty);
                        else if (Convert.ToInt32(strValor.Substring(0, 3)) > 1)
                            valorExtenso += " trilhões" + ((Convert.ToDecimal(strValor.Substring(3, 12)) > 0) ? " e " : string.Empty);
                    }
                    else if (i == 3 & valorExtenso != string.Empty)
                    {
                        if (Convert.ToInt32(strValor.Substring(3, 3)) == 1)
                            valorExtenso += " bilhão" + ((Convert.ToDecimal(strValor.Substring(6, 9)) > 0) ? " e " : string.Empty);
                        else if (Convert.ToInt32(strValor.Substring(3, 3)) > 1)
                            valorExtenso += " bilhões" + ((Convert.ToDecimal(strValor.Substring(6, 9)) > 0) ? " e " : string.Empty);
                    }
                    else if (i == 6 & valorExtenso != string.Empty)
                    {
                        if (Convert.ToInt32(strValor.Substring(6, 3)) == 1)
                            valorExtenso += " milhão" + ((Convert.ToDecimal(strValor.Substring(9, 6)) > 0) ? " e " : string.Empty);
                        else if (Convert.ToInt32(strValor.Substring(6, 3)) > 1)
                            valorExtenso += " milhões" + ((Convert.ToDecimal(strValor.Substring(9, 6)) > 0) ? " e " : string.Empty);
                    }
                    else if (i == 9 & valorExtenso != string.Empty)
                        if (Convert.ToInt32(strValor.Substring(9, 3)) > 0)
                            valorExtenso += " mil" + ((Convert.ToDecimal(strValor.Substring(12, 3)) > 0) ? " e " : string.Empty);

                    if (i == 12)
                    {
                        if (valorExtenso.Length > 8)
                            if (valorExtenso.Substring(valorExtenso.Length - 6, 6) == "bilhão" | valorExtenso.Substring(valorExtenso.Length - 6, 6) == "milhão")
                                valorExtenso += " de";
                            else
                                if (valorExtenso.Substring(valorExtenso.Length - 7, 7) == "bilhões" | valorExtenso.Substring(valorExtenso.Length - 7, 7) == "milhões" | valorExtenso.Substring(valorExtenso.Length - 8, 7) == "trilhões")
                                    valorExtenso += " de";
                                else
                                    if (valorExtenso.Substring(valorExtenso.Length - 8, 8) == "trilhões")
                                        valorExtenso += " de";
                        if (GetDescriptionFromEnumValue(tipo).ToLower() == "currency")
                        {
                            if (Convert.ToInt64(strValor.Substring(0, 15)) == 1)
                                valorExtenso += " real";
                            else if (Convert.ToInt64(strValor.Substring(0, 15)) > 1)
                                valorExtenso += " reais";
                        }
                        if (Convert.ToInt32(strValor.Substring(16, 2)) > 0 && valorExtenso != string.Empty)
                            valorExtenso += " e ";
                    }
                    if (GetDescriptionFromEnumValue(tipo).ToLower() == "currency")
                    {
                        if (i == 15)
                            if (Convert.ToInt32(strValor.Substring(16, 2)) == 1)
                                valorExtenso += " centavo";
                            else if (Convert.ToInt32(strValor.Substring(16, 2)) > 1)
                                valorExtenso += " centavos";
                    }
                }
                if (GetDescriptionFromEnumValue(tipo).Equals("M") || GetDescriptionFromEnumValue(tipo).Equals("Currency"))
                {
                    valorExtenso = valorExtenso.Substring(0, 1).ToUpper() + valorExtenso.Substring(1, valorExtenso.Length - 1);
                }

                return valorExtenso;
            }
        }

        static string EscreveParte(decimal valor)
        {
            if (valor <= 0)
                return string.Empty;
            else
            {
                string montagem = string.Empty;
                if (valor > 0 & valor < 1)
                {
                    valor *= 100;
                }
                string strValor = valor.ToString("000");
                int a = Convert.ToInt32(strValor.Substring(0, 1));
                int b = Convert.ToInt32(strValor.Substring(1, 1));
                int c = Convert.ToInt32(strValor.Substring(2, 1));

                if (a == 1) montagem += (b + c == 0) ? "cem" : "cento";
                else if (a == 2) montagem += "duzentos";
                else if (a == 3) montagem += "trezentos";
                else if (a == 4) montagem += "quatrocentos";
                else if (a == 5) montagem += "quinhentos";
                else if (a == 6) montagem += "seiscentos";
                else if (a == 7) montagem += "setecentos";
                else if (a == 8) montagem += "oitocentos";
                else if (a == 9) montagem += "novecentos";

                if (b == 1)
                {
                    if (c == 0) montagem += ((a > 0) ? " e " : string.Empty) + "dez";
                    else if (c == 1) montagem += ((a > 0) ? " e " : string.Empty) + "onze";
                    else if (c == 2) montagem += ((a > 0) ? " e " : string.Empty) + "doze";
                    else if (c == 3) montagem += ((a > 0) ? " e " : string.Empty) + "treze";
                    else if (c == 4) montagem += ((a > 0) ? " e " : string.Empty) + "quatorze";
                    else if (c == 5) montagem += ((a > 0) ? " e " : string.Empty) + "quinze";
                    else if (c == 6) montagem += ((a > 0) ? " e " : string.Empty) + "dezesseis";
                    else if (c == 7) montagem += ((a > 0) ? " e " : string.Empty) + "dezessete";
                    else if (c == 8) montagem += ((a > 0) ? " e " : string.Empty) + "dezoito";
                    else if (c == 9) montagem += ((a > 0) ? " e " : string.Empty) + "dezenove";
                }
                else if (b == 2) montagem += ((a > 0) ? " E " : string.Empty) + "vinte";
                else if (b == 3) montagem += ((a > 0) ? " E " : string.Empty) + "trinta";
                else if (b == 4) montagem += ((a > 0) ? " E " : string.Empty) + "quarenta";
                else if (b == 5) montagem += ((a > 0) ? " E " : string.Empty) + "cinquenta";
                else if (b == 6) montagem += ((a > 0) ? " E " : string.Empty) + "sessenta";
                else if (b == 7) montagem += ((a > 0) ? " E " : string.Empty) + "setenta";
                else if (b == 8) montagem += ((a > 0) ? " E " : string.Empty) + "oitenta";
                else if (b == 9) montagem += ((a > 0) ? " E " : string.Empty) + "noventa";

                if (strValor.Substring(1, 1) != "1" & c != 0 & montagem != string.Empty) montagem += " e ";

                if (strValor.Substring(1, 1) != "1")
                    if (c == 1) montagem += "um";
                    else if (c == 2) montagem += "dois";
                    else if (c == 3) montagem += "três";
                    else if (c == 4) montagem += "quatro";
                    else if (c == 5) montagem += "cinco";
                    else if (c == 6) montagem += "seis";
                    else if (c == 7) montagem += "sete";
                    else if (c == 8) montagem += "oito";
                    else if (c == 9) montagem += "nove";

                return montagem;
            }
        }
        public static string Format(decimal valor, TabelaFormatacao tipo)
        {
            string stringFormatada = String.Format(GetDescriptionFromEnumValue(tipo), valor);

            return stringFormatada;
        }
        public static string GetDescriptionFromEnumValue(Enum value)
        {
            DescriptionAttribute attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .SingleOrDefault() as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static T GetEnumValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException();
            FieldInfo[] fields = type.GetFields();
            var field = fields
                            .SelectMany(f => f.GetCustomAttributes(
                                typeof(DescriptionAttribute), false), (
                                    f, a) => new { Field = f, Att = a })
                            .Where(a => ((DescriptionAttribute)a.Att)
                                .Description == description).SingleOrDefault();
            return field == null ? default(T) : (T)field.Field.GetRawConstantValue();
        }
    }
    // Enums de formatação. As description são usadas como tipo de formatação na própria função de Format() do c#
    public enum TabelaFormatacao
    {
        [Description("{0:0}")]
        D,
        [Description("{0:00}")]
        DD,
        [Description("EXTENSO")]
        DDD,
        [Description("Extenso")]
        ddd,
        [Description("{0:0}")]
        M,
        [Description("{0:00}")]
        MM,
        [Description("M")]
        MMM,
        [Description("m")]
        mmm,
        [Description("M")]
        MMMM,
        [Description("m")]
        mmmm,
        [Description("Decada")]
        AA,
        [Description("{0:0}")]
        AAAA,
        [Description("{0:0}")]
        AAAAA,
        [Description("Extenso")]
        aaaaa,
        [Description("M")]
        INTEIRO,
        [Description("m")]
        inteiro,
        [Description("M")]
        BOOLEANO,
        [Description("m")]
        booleano,
        [Description("Currency")]
        CURR,
        [Description("currency")]
        curr

    }

}
