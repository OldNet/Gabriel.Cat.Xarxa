using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.Xarxa
{
    public abstract class ServicioValidacionIP:IComparable,IComparable<ServicioValidacionIP>
    {
        public static bool ShowDebbugMessages=true;
        /// <summary>
        /// Valida la Ip que no use Proxy u otro medio no directo (para prevenir ataques masivos)
        /// </summary>
        /// <param name="conexionNueva"></param>
        /// <returns></returns>
     public  abstract bool ValidaIp(string direccionIP);
     public virtual bool EstaElServicioOperativo()
     {
         return true;
     }
     public virtual TimeSpan TiempoRenovarServicio()
     {
         return TimeSpan.MinValue;
     }
     public virtual TimeSpan TiempoUsoServicio() {
         return TimeSpan.MaxValue;
     }

     #region Miembros de IComparable

     public int CompareTo(object obj)
     {
         return CompareTo(obj as ServicioValidacionIP);
     }

     #endregion

     #region Miembros de IComparable<ServicioValidacionIP>

     public int CompareTo(ServicioValidacionIP other)
     {
         int compareTo;

         if (other != null)
         {
             compareTo = EstaElServicioOperativo().CompareTo(other.EstaElServicioOperativo());
             if (compareTo==0)
             {
                 compareTo = TiempoUsoServicio().CompareTo(other.TiempoUsoServicio());
                 if (compareTo == 0)
                 {
                     compareTo = TiempoRenovarServicio().CompareTo(other.TiempoRenovarServicio());
                 }
             }
         }
         else {
             compareTo = -1;
         }
         return compareTo;
     }

     #endregion
    }

}
