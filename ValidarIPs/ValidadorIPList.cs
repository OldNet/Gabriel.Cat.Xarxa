﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.Xarxa.ValidarIPs
{
    public class ValidadorIPList:ServicioValidacionIP
    {
        public static bool LanzarExcepcionSiNoHayServicioDisponible = false;
        Llista<ServicioValidacionIP> servicios;
        public ValidadorIPList()
        { 
            servicios = new Llista<ServicioValidacionIP>();
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
                if (LanzarExcepcionSiNoHayServicioDisponible)
                    throw new Exception("No hay servicios disponibles");
            }
            else
            {
                servicios.Ordena();
                if (!servicios[0].EstaElServicioOperativo())
                {
                    if (LanzarExcepcionSiNoHayServicioDisponible)
                        throw new Exception("No hay servicios disponibles");
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
