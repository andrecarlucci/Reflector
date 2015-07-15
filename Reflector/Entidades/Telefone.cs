using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflector.Entidades
{
    public class Telefone
    {
        public int Id { get; set; }
        public TipoTelefone TipoTelefone { get; set; }
        public string Numero { get; set; }
    }
    public enum TipoTelefone
    {
        Residencial = 1,
        Celular = 2,
        Comercial = 3
        
    }
}
