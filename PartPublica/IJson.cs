/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 19/11/2014
 * Hora: 18:01
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;

namespace Gabriel.Cat
{
	/// <summary>
	/// Per poder Utilitzar el json ha d'haver un constructor sense parametres i totes les propietats dels valors a dessar han de tenir get;set; el altres tenen de tenir una etiqueta [JsonIgnore]
	/// per tornar-lo objecte s'utilitza
	/// "JsonConvert.DeserializeObject>NOMCLASSE>(jSon);"//el json pot ser un objecteJson o un string json
	/// 
	/// </summary>
	public interface IJson
		
	{
		/// <summary>
		///  s'ha de posar aquest codi
		/// "return (JsonObject)Newtonsoft.Json.JsonConvert.SerializeObject(this);"
		/// </summary>
		/// <returns></returns>
		JsonObject ToJson();
	}
}
