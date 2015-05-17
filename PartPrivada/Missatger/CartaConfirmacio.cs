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
	/// Description of CartaConfirmacio.
	/// </summary>
	class CartaConfirmacio:Carta
	{
		public enum Tipus
		{
			cartaPartPaquet,
			cartaPaquet,
			partPaquet

		}
		string idRebut;
		Tipus tipus;
		bool correcte;
		public CartaConfirmacio(string idRebut, Tipus tipus,bool correcte)
		{
			IdRebut = idRebut;
			TipusId=tipus;
			Correcte=correcte;
		}
		public CartaConfirmacio(JsonObject jSon)
			: base("0")
		{
			if (jSon == null)
				throw new Exception("Json CartaPaquet incorrecte");
			CartaConfirmacio cartaJson = JsonConvert.DeserializeObject<CartaConfirmacio>(jSon);
			if (!cartaJson.IdIntern.Contains(ToString()))
				throw new Exception("Json incorrecte");
			this.IdIntern = cartaJson.IdIntern;
			this.IdRebut = cartaJson.IdRebut;
			this.TipusId = cartaJson.TipusId;
			this.Correcte=cartaJson.Correcte;

		}
		public CartaConfirmacio()
			: base("0")
		{
		}

		public bool Correcte {
			get {
				return correcte;
			}
			set {
				correcte = value;
			}
		}
		public Tipus TipusId {
			get {
				return tipus;
			}
			set {
				tipus = value;
			}
		}

		public string IdRebut {
			get {
				return idRebut;
			}
			set {
				idRebut = value;
			}
		}

		public override string ToString()
		{
			return base.ToString() + "Confirmacio";
		}
	}
}
