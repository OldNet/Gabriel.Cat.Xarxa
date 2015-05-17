/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 19/11/2014
 * Hora: 18:02
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of IClientMissatgerObject.
	/// </summary>
	public interface IClientMissatgerObject
	{
		/// <summary>
		/// Serveix per rebre el objectes que arriben al missatger
		/// </summary>
		/// <param name="referenciaObjecte">Es sol utilitzar una enumeracio per referencia</param>
		/// <param name="objecteRebut">json amb les dades d'un objecte IJson</param>
		void RebObjecte(int referenciaObjecte,JsonObject objecteRebut);
		void RebArxiu(string nomAmbExtensio,byte[] arxiu);
		
	}
}
