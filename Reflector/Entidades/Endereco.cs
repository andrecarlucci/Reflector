using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflector.Entidades
{
    public class Endereco
    {
        public int Id { get; set; }
        public string Logradouro { get; set; }
        public TipoLogradouro TipoLogradouro { get; set; }
        public int Numero { get; set; }
        public TipoResidencia TipoResidencia { get; set; }
        public TipoEndereco TipoEndereco { get; set; }
        public string Complemento { get; set; }
    }
    public enum TipoLogradouro
    {
        Avenida = 1,
        Rua = 2,
        Travessa = 3,
        Rodovia = 4,
        Estrada = 5
        
    }
    public enum TipoResidencia
    {
        Casa = 1,
        Apartamento = 2,
        KitNet = 3,
        Sala = 4
    }
    public enum TipoEndereco
    {
        Residencial = 1,
        Comercial = 2,
        Estadia = 3
    }
}
