/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 19/11/2014
 * Hora: 18:04
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;

namespace Gabriel.Cat
{
	

	/// <summary>
	/// Description of Servidor.
	/// </summary>
	class ServidorLocal:ServidorXarxaLocal
	{
		protected override string ObetenirIpServidor()
		{
			return "127.1.1.0";
		}


	}
}
