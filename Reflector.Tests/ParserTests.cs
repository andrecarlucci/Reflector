using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Reflector.Entidades;
using Xunit;

namespace Reflector.Tests {
    public class ParserTests {
        private Pessoa _pessoa; 

        [Fact]
        public void Deve_gerar_template_de_teste() {
            var template = File.ReadAllText("files/template1.txt", Encoding.UTF8);
            var expected = File.ReadAllText("files/output1.txt", Encoding.UTF8);
            var relatorio = Parser.ImprimirModeloRelatorio(_pessoa, template);
            Assert.Equal(expected, relatorio);
        }

        [Fact]
        public void Deve_gerar_declaracao_de_imovel() {
            var template = File.ReadAllText("files/template2.txt", Encoding.UTF8);
            var expected = File.ReadAllText("files/output2.txt", Encoding.UTF8);
            var relatorio = Parser.ImprimirModeloRelatorio(_pessoa, template);
            Assert.Equal(expected, relatorio);
        }

        public ParserTests() {
            var telefones = new List<Telefone> {
                new Telefone {Id = 1, Numero = "554833333333", TipoTelefone = TipoTelefone.Residencial },
                new Telefone { Id = 2, Numero = "554833334444", TipoTelefone = TipoTelefone.Comercial },
                new Telefone { Id = 3, Numero = "554899998888", TipoTelefone = TipoTelefone.Celular }
            };
            var enderecos = new List<Endereco> { 
                new Endereco { Id = 1, Numero = 22, Logradouro = "Rua Sem Saida", TipoEndereco = TipoEndereco.Residencial, TipoLogradouro = TipoLogradouro.Rua, TipoResidencia = TipoResidencia.Casa }, 
                new Endereco { Id = 2, Numero = 33, Logradouro = "Rua Com Saida", TipoEndereco = TipoEndereco.Comercial, TipoLogradouro = TipoLogradouro.Rua, TipoResidencia = TipoResidencia.Sala } 
            };
            _pessoa = new Pessoa {
                Id = 1,
                Nome = "Joe",
                DataNascimento = DateTime.Parse("20/10/1986"),
                Enderecos = enderecos,
                Telefones = telefones,
                Documentos = new List<Documento> {
                    new Documento {
                        Id = 1, Numero = "999.999.999-97", 
                        TipoDocumento = TipoDocumento.RG, 
                        Emissao = DateTime.Now, 
                        IsDocumentoIdentidade = true
                    }
                }
            };
        }
    }
}
