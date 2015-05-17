/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 21/11/2014
 * Hora: 16:27
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of ServidorXarxaEnLinea.
	/// </summary>
	class ServidorXarxaEnLinea:Servidor
	{
		public ServidorXarxaEnLinea()
		{
		}

		#region implemented abstract members of Servidor

		public override void Envia(string ipÌd, JsonObject json)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
