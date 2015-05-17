/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 04/12/2014
 * Hora: 16:27
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using Newtonsoft.Json;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of CartaReclamacio.
	/// </summary>
	class CartaReclamacio:Carta
	{
		bool esCarta;
		string idAReclamar;
		public CartaReclamacio(string idAReclamar,bool esCarta)
		{
			IdAReclamar=idAReclamar;
			EsCarta=esCarta;
		}
		public CartaReclamacio():base("0"){}
		public CartaReclamacio(JsonObject json)
		{
			if(json==null)
				throw new Exception("Json incorrecte");
			CartaReclamacio cartaJson=JsonConvert.DeserializeObject<CartaReclamacio>(json);
			if(!cartaJson.IdIntern.Contains(ToString()))
				throw new Exception("Json incorrecte");
			this.IdAReclamar=cartaJson.IdAReclamar;
			this.IdIntern=cartaJson.IdIntern;
			this.EsCarta=cartaJson.EsCarta;
		}
		public string IdAReclamar {
			get {
				return idAReclamar;
			}
			set {
				idAReclamar = value;
			}
		}

		public bool EsCarta {
			get {
				return esCarta;
			}
			set {
				esCarta = value;
			}
		}
				public override string ToString()
		{
			return base.ToString()+"Reclamacio";
		}
	}
}
