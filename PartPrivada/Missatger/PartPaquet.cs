/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 19/11/2014
 * Hora: 18:01
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using Newtonsoft.Json;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of PartPaquet.
	/// </summary>
	class PartPaquet:IJson
	{
		string contingut;
		int numPart;
		int totalParts;
		string idPaquet;

		public PartPaquet(int idPaquet, string contingut, int numPart,int totalParts)
		{
			if (String.IsNullOrEmpty(contingut) || idPaquet < 0 || numPart < 0||totalParts<=0)
				throw new Exception("Error al crear la part");
			this.totalParts=totalParts;
			this.contingut = contingut;
			this.numPart = numPart;
			this.idPaquet = "idPaquet" +
			idPaquet;
		}
		public PartPaquet(JsonObject json)
		{
			if (json == null)
				throw new Exception("Error al crear la part,json incorrecte");
			PartPaquet partJson = JsonConvert.DeserializeObject<PartPaquet>(json);
			if (!partJson.idPaquet.Contains("idPaquet:"))
				throw new Exception("Json incorrecte");
			contingut = partJson.contingut;
			numPart = partJson.numPart;
			idPaquet = partJson.idPaquet;
			totalParts=partJson.totalParts;
		}
		public PartPaquet()
		{
		}
		[JsonIgnore]
		public string Id
		{get{return idPaquet+";"+numPart;}}
		public int TotalParts {
			get {
				return totalParts;
			}
			set {
				totalParts = value;
			}
		}

		public string PartMissatge {
			get{ return contingut; }
			set {
				if (String.IsNullOrEmpty(value))
					throw new Exception("Falta contingut");
				contingut = value;
			}


		}

		public int NumPart {
			get {
				return numPart;
			}
			set {
				if (value < 0)
					throw new Exception("Ha de ser un numero més gran de 0");
				numPart = value;
			}
		}

		public string IdPaquet {
			get {
				return idPaquet;
			}
			set {
				idPaquet = "idPaquet" + value;
			}
		}

		#region IJson implementation

		public JsonObject ToJson()
		{
			return (JsonObject)JsonConvert.SerializeObject(this);
		}

		#endregion
	}
}
