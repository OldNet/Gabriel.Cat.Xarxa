/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 19/11/2014
 * Hora: 18:02
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using Newtonsoft.Json;
namespace Gabriel.Cat
{
	/// <summary>
	/// Description of JsonObject.
	/// </summary>
	 public class JsonObject
    {
        string json;
        public JsonObject() { }
        public JsonObject(string json)
        {
            try { JsonConvert.DeserializeObject(json); }
            catch { throw new Exception("El Json no te el format correcte"); }
            this.json = json;
            
        }
        public string Json
        {
            get { return json; }
            set { json = value; }
        }
        public override string ToString()
        {
            return json;
        }
        public static implicit operator string(JsonObject jsonObject)
        {
            return jsonObject.json;
        }
        public static explicit operator JsonObject(string json)
        {
            JsonObject jSonObj=null;
            try
            {
                jSonObj = new JsonObject(json);
            }
            catch { }
            return jSonObj;
        }
    }
}
