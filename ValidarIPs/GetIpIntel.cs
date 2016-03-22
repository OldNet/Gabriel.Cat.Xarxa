using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.Xarxa
{
    /// <summary>
    /// Servicio free de www.getipintel.net se hacen como maximo 495 queris por dia y 15 por minuto :) si no tiene por defecto es false
    /// </summary>
    public class GetIpIntelServicioFreeValidacionIP : ServicioValidacionIP
    {
        /*
         * Free Service
         How many queries can I make?

          We rate limit 15 requests / minute to prevent abuse.
         * There's also a burst parameter set to ensure smoothing of traffic.
         * If you hit any of these limits, the web server will return a 429 error. 
         * Please do not exceed 500 queries per day. 
         * The limits may change based on abuse and/or server load which will be posted on twitter and at least one week in advance. 
         * If you need guaranteed resources and/or more queries, please contact me. In most cases, the cost is significantly less than other paid services.
         * 
         * Contacto
         * https://github.com/blackdotsh
         */
        /// <summary>
        /// Fuerza a inspecionar la ip a fondo puede tardar 5 segundos el servicio
        /// </summary>
        
        public bool SinServicioFreeDisponible { get; set; }
        public bool ServicioAFondo { get; set; }
        const int MAXQUERISPERDAY=33*MAXQUERIPERMINUTE;//es para no pasarme de las queris :) 495
        const int MAXQUERIPERMINUTE = 15;
        protected  System.Timers.Timer tmpRenovarQuerisMinute;
        protected  System.Timers.Timer tmpRenovarQuerisPerDay;
        int numeroDeSolicitudesRestantes;
        int numeroSolicitudesTotalesHechas;//se renueva cada dia
        public GetIpIntelServicioFreeValidacionIP()
        {
            SinServicioFreeDisponible = false;
            ServicioAFondo = true;
            numeroDeSolicitudesRestantes = MAXQUERIPERMINUTE;
            numeroSolicitudesTotalesHechas = 0;
            tmpRenovarQuerisMinute = new System.Timers.Timer();
            tmpRenovarQuerisMinute.Interval = 60 * 1000;
            tmpRenovarQuerisMinute.Elapsed += (s, arg) => {
                if (numeroSolicitudesTotalesHechas != MAXQUERISPERDAY)
                    numeroDeSolicitudesRestantes = MAXQUERIPERMINUTE;
                else
                    tmpRenovarQuerisMinute.Enabled = false;
            };
            tmpRenovarQuerisPerDay = new System.Timers.Timer();
            tmpRenovarQuerisPerDay.Interval = 24*60*60* 1000;
            tmpRenovarQuerisPerDay.Elapsed += (s, arg) => {
                tmpRenovarQuerisMinute.Enabled = true;
                numeroDeSolicitudesRestantes = MAXQUERIPERMINUTE;
                numeroSolicitudesTotalesHechas = 0; };
            tmpRenovarQuerisMinute.Start();
            tmpRenovarQuerisPerDay.Start();
        }
        
        public override bool ValidaIp(string ipAComprobar)
        {
            bool valido = SinServicioFreeDisponible;
            if (EstaElServicioOperativo())
            {
                numeroSolicitudesTotalesHechas++;
                numeroDeSolicitudesRestantes--;
                //esta web mira proxy,vpn,red TOR y bad ip detection
                const char NOUSAPROXYETC = '1';
                string servicioHaFondo = ServicioAFondo ? "&flags=f" : "";
                string pathWebConIp = "http://check.getipintel.net/check.php?ip=" + ipAComprobar + servicioHaFondo;
                LanzarMensaje("Se usara la web '{0}' para validar la ip", pathWebConIp);
                System.Net.Http.HttpClient cliente = new System.Net.Http.HttpClient();
                Task<System.Net.Http.HttpResponseMessage> respuesta = cliente.GetAsync(pathWebConIp);
                Task<string> datosRespuesta;
                string respuestaString;
                respuesta.Wait();
                datosRespuesta = respuesta.Result.Content.ReadAsStringAsync();//metodo para mirarlo online :)
                datosRespuesta.Wait();
                respuestaString = datosRespuesta.Result;
                LanzarMensaje("La respuesta de la web '{0}' para la ip {1}", respuestaString, ipAComprobar);
                valido= respuestaString.Contains(NOUSAPROXYETC);

            }
            return valido;
        }
        public override bool EstaElServicioOperativo()
        {
            return numeroDeSolicitudesRestantes!=0&&numeroSolicitudesTotalesHechas<MAXQUERISPERDAY;
        }

        public override TimeSpan TiempoUsoServicio()
        {
            return new TimeSpan(24, 0, 0);
        }
    }

    /// <summary>
    /// Si el servicio de www.getipintel.net se paga diria que no tiene limites :)
    /// </summary>
    public class GetIpIntelServicioValidacionIP:GetIpIntelServicioFreeValidacionIP
    {
        public GetIpIntelServicioValidacionIP()
            : base()
        {
            tmpRenovarQuerisMinute.Enabled = false;
            tmpRenovarQuerisPerDay.Enabled = false;
        }
        public override bool EstaElServicioOperativo()
        {
            return true;
        }
    }

}
