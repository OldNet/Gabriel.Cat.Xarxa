using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.Extension;
using System.Timers;

namespace Gabriel.Cat.Xarxa
{
   public class SesionUsuario
    {
        public const double TIEMPOLIMPIEZADEFAULT = 1000 * 60 * 30;//cada 30 minutos si hay algo claro
        /// <summary>
        /// Tiempo en milisegundos
        /// </summary>
        public static double TiempoLimpieza= TIEMPOLIMPIEZADEFAULT;
        public Timer TimerLimpieza { get; private set; }

        bool estaLimpio;
        public SesionUsuario()
        {
            TimerLimpieza = new Timer(TiempoLimpieza);
            TimerLimpieza.Elapsed += (s, e) => LimpiarLista();
            DatosUsuario = new LlistaOrdenada<UsuarioData, IUsuarioData>((s, e1, obj) => { return obj.TratarDatos == SesionDataEnum.Conservar; }, null, null, null, null);//es una tonteria guardar los que van a tirarse...no?
            DatosUsuario.Added += (s, e) => {
                estaLimpio = false;
                if (!TimerLimpieza.Enabled)
                    TimerLimpieza.Start();
            };
        }
        public LlistaOrdenada<UsuarioData,IUsuarioData> DatosUsuario
        { get; private set; }
        public void LimpiarLista()
        {
            if (!estaLimpio)//asi esta operacion tan costosa solo lo hago una vez
            {
                DatosUsuario.Elimina(DatosUsuario.FiltraKeys((obj) => obj.ObjectData.TratarDatos == SesionDataEnum.Liberar));
                estaLimpio = true;
                if (TimerLimpieza.Enabled)
                    TimerLimpieza.Stop();
            }
        }
    }

    public class UsuarioData : IComparable, IComparable<UsuarioData>, IComparable<IUsuarioData>
    {

        IUsuarioData objectData;
        public UsuarioData(IUsuarioData obj)
        {
            objectData = obj;
        }
        public IUsuarioData ObjectData
        { get { return objectData; } }

        public int CompareTo(IUsuarioData other)
        {
            int compareTo;
            if (other != null)
                compareTo = objectData.IdUnico.CompareTo(other.IdUnico);
            else compareTo = -1;
            return compareTo;

        }

        public int CompareTo(object obj)
        {
           return CompareTo(obj as UsuarioData);
        }

        public int CompareTo(UsuarioData other)
        {
            int compareTo;

            if (other != null) compareTo = CompareTo(other.ObjectData);
            else compareTo = -1;
            return compareTo;
        }


    }
    public enum SesionDataEnum
    {
       Liberar,Conservar
    }
    public interface IUsuarioData
    {
        SesionDataEnum TratarDatos { get;}
        IComparable IdUnico { get; }
        object Datos { get;}      
    }
}
