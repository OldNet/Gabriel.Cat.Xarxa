/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 04/12/2014
 * Hora: 19:45
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using Newtonsoft.Json;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of ByteJson.
	/// </summary>
	public 	class ByteJson:IJson
	{
		#region IJson implementation
		byte[] dades;
		public ByteJson(byte[] dades)
		{
			this.dades = dades;
		}
		public ByteJson(JsonObject json)
		{
			if (json == null)
				throw new Exception("Json incorrecte");
			ByteJson byteJson = JsonConvert.DeserializeObject<ByteJson>(json);
			this.dades = byteJson.dades;
		}
		public ByteJson()
		{
		}

		public byte[] Dades {
			get {
				return dades;
			}
			set {
				dades = value;
			}
		}

		public JsonObject ToJson()
		{
			return (JsonObject)JsonConvert.SerializeObject(this);
		}
		#endregion
		
	}
}
