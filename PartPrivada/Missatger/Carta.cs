/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 19/11/2014
 * Hora: 17:56
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using Newtonsoft.Json;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of Carta.
	/// </summary>
	abstract class Carta:IJson
	{
		string idIntern;
		static int idGen=0;
		public Carta(string id)
		{
			idIntern=id;
		}
		public Carta()
		{
			idIntern=ToString()+idGen++;
		}
		public string IdIntern {
			get {
				return idIntern;
			}
			set {
				idIntern = value;
			}
		}

		#region IJson implementation

		public JsonObject ToJson()
		{
			return (JsonObject)JsonConvert.SerializeObject(this);
		}
		public override string ToString()
		{
			return "Carta:";
		}
		#endregion
	}
}
