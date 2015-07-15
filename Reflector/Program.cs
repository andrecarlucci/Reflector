using Reflector.Entidades;
using System;
using System.Collections.Generic;
using System.IO;
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Reflector {
    class Program {
        static void Main(string[] args) {
            List<Telefone> telefones = new List<Telefone>(){
                new Telefone {Id = 1, Numero = "554833333333", TipoTelefone = TipoTelefone.Residencial },
                new Telefone { Id = 2, Numero = "554833334444", TipoTelefone = TipoTelefone.Comercial },
                new Telefone { Id = 3, Numero = "554899998888", TipoTelefone = TipoTelefone.Celular }
            };
            List<Endereco> enderecos = new List<Endereco> { 
                new Endereco { Id = 1, Numero = 22, Logradouro = "Rua Sem Saida", TipoEndereco = TipoEndereco.Residencial, TipoLogradouro = TipoLogradouro.Rua, TipoResidencia = TipoResidencia.Casa }, 
                new Endereco { Id = 2, Numero = 33, Logradouro = "Rua Com Saida", TipoEndereco = TipoEndereco.Comercial, TipoLogradouro = TipoLogradouro.Rua, TipoResidencia = TipoResidencia.Sala } 
            };
            Pessoa pessoa = new Pessoa {
                Id = 1,
                Nome = "Ronaldão das Américas - vulgo Banha",
                DataNascimento = DateTime.Parse("20/10/1986"),
                Enderecos = enderecos,
                Telefones = telefones,
                Documentos = new List<Documento>() { new Documento { Id = 1, Numero = "999.999.999-97", TipoDocumento = TipoDocumento.RG, Emissao = DateTime.Now, IsDocumentoIdentidade = true } }
            };
            string html = string.Empty;
            StringBuilder storeContent = new StringBuilder();
            using (StreamReader reader = new StreamReader(@"C:\Textos\texto2.txt")) {
                string line = string.Empty;
                while ((line = reader.ReadLine()) != null) {
                    storeContent.Append(line);
                    html += line;
                }
            }

            html = Parser.ImprimirModeloRelatorio(pessoa, html);
            File.WriteAllText("output.txt", html);
        }
    }

}

