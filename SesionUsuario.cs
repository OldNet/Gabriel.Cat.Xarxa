using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.Extension;
namespace Gabriel.Cat.Xarxa
{
   public class SesionUsuario
    {
        public struct UsuarioData:IComparable,IComparable<UsuarioData>,IComparable<IUsuarioData>
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
                int compareTo;
                try
                {
                    compareTo = CompareTo((UsuarioData)obj);
                }catch { compareTo = -1; }
                return compareTo;
            }

            public int CompareTo(UsuarioData other)
            {
                return CompareTo(other.ObjectData);
            }

           
        }
        LlistaOrdenada<UsuarioData, IUsuarioData> datosUsuario;
        bool estaLimpio;
        public SesionUsuario()
        {
            datosUsuario = new LlistaOrdenada<UsuarioData, IUsuarioData>((s, e1,obj) => { return obj.TratarDatos == SesionDataEnum.Conservar; }, null, null, null, null);//es una tonteria guardar los que van a tirarse...no?
            datosUsuario.Added += (s, e) => { estaLimpio = false; };
        }
        public LlistaOrdenada<UsuarioData,IUsuarioData> DatosUsuario
        { get { return datosUsuario; } }
        public void LimpiarLista()
        {
            if (!estaLimpio)//asi esta operacion tan costosa solo lo hago una vez
            {
                datosUsuario.Elimina(datosUsuario.Filtra((obj) => obj.Value.TratarDatos == SesionDataEnum.Liberar).KeysToArray());
                estaLimpio = true;
            }
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
