using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflector.Entidades
{
    public class Pessoa
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public DateTime DataNascimento { get; set; }
        public List<Endereco> Enderecos { get; set; }
        public List<Documento> Documentos { get; set; }
        public List<Telefone> Telefones { get; set; }
    }
}
