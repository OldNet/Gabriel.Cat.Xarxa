using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.Xarxa
{
    public class ValidadorIPList:ServicioValidacionIP
    {

        Llista<ServicioValidacionIP> servicios;
        public ValidadorIPList()
        { 
            servicios = new Llista<ServicioValidacionIP>();
        }
        public ValidadorIPList(params ServicioValidacionIP[] servicios)
        {
            if (servicios != null)
            {
                for (int i = 0; i < servicios.Length; i++)
                    if (servicios[i] != null)
                        Servicios.Afegir(servicios[i]);
            }
        }
        public Llista<ServicioValidacionIP> Servicios
        { 
            get
            { 
                return servicios;
            }
        }
        public override bool ValidaIp(string direccionIP)
        {
            bool ipValida=true;
            if (HayServicioOperativo())
            {
                ipValida = servicios[0].ValidaIp(direccionIP);
            }
            return ipValida;
        }


        public override bool EstaElServicioOperativo()
        {
            bool estaElServicioOperativo = true;
            if (HayServicioOperativo())
            {
                estaElServicioOperativo = servicios[0].EstaElServicioOperativo();
            }
            return estaElServicioOperativo;
        }
        public override TimeSpan TiempoRenovarServicio()
        {
            TimeSpan tiempoRenovarServicio = base.TiempoRenovarServicio();
            if (HayServicioOperativo())
            {
                tiempoRenovarServicio = servicios[0].TiempoRenovarServicio();
            }
            return tiempoRenovarServicio;
        }
        public override TimeSpan TiempoUsoServicio()
        {
            TimeSpan tiempoUsoServicio = base.TiempoUsoServicio();
            if (HayServicioOperativo())
            {
                tiempoUsoServicio = servicios[0].TiempoUsoServicio();
            }
            return tiempoUsoServicio;
        }
        private bool HayServicioOperativo()
        {
            bool hayServicio = false;

            if (servicios.Count == 0)
            {
                    LanzarMensaje( "No hay servicios disponibles");  
            }
            else
            {
                servicios.Ordena();
                if (!servicios[0].EstaElServicioOperativo())
                {
                    LanzarMensaje("No hay servicios operativos");  
                }
                else
                {
                    hayServicio = true;
                }
            }
            return hayServicio;
        }


    }
}
