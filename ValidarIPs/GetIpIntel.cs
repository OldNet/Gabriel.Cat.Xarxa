using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.Xarxa
{
    public class GetIpIntelServicioValidacionIP : ServicioValidacionIP
    {

        public override bool ValidaIp(string ipAComprobar)
        {
            //esta web mira proxy,vpn,red TOR y bad ip detection
            const char USAPROXYETC = '1';
            string pathWebConIp = "http://check.getipintel.net/check.php?ip=" + ipAComprobar;
            MostrarProblema("Se usara la web '{0}' para validar la ip", pathWebConIp);
            System.Net.Http.HttpClient cliente = new System.Net.Http.HttpClient();
            Task<System.Net.Http.HttpResponseMessage> respuesta = cliente.GetAsync(pathWebConIp);
            Task<string> datosRespuesta;
            string respuestaString;
            respuesta.Wait();
            datosRespuesta = respuesta.Result.Content.ReadAsStringAsync();//metodo para mirarlo online :)
            datosRespuesta.Wait();
            respuestaString = datosRespuesta.Result;
            MostrarProblema("La respuesta de la web '{0}' para la ip {1}", respuestaString, ipAComprobar);
            return respuestaString.Contains(USAPROXYETC);
        }
    }
}
