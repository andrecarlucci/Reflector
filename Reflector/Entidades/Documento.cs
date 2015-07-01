using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflector.Entidades
{
    public class Documento
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public TipoDocumento TipoDocumento { get; set; }
        public DateTime Emissao { get; set; }
        public bool IsDocumentoIdentidade { get; set; }

    }
    public enum TipoDocumento
    {
        RG = 1,
        CPF = 2,
        CNH = 2,
        OAB = 5,
        Passaporte = 6
    }
}
