using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ZTI.Project.Client.Data;
using static ZTI.Project.Client.Constants;

namespace ZTI.Project.Client {
	/// <summary>
	/// Klasa zawierajaca funkcje pomocnicze wykorzystywane w calej aplikacji
	/// </summary>
	public static class Utils {
		/// <summary>
		/// Funkcja pobierajaca dane o przystankach lub liniach z servera
		/// </summary>
		/// <typeparam name="T">Typ jaki ma byc pobrany z serwera</typeparam>
		/// <param name="url">Adres z jakiego dane maja byc pobrane</param>
		/// <param name="numberOfRequests">Ile razy zapytanie ma byc wykonane w razie niepowodzenia</param>
		/// <returns>Pobrane dane w formie tablicy</returns>
		public static async Task<List<T>> GetListOfDataFromServer<T>(string url, int numberOfRequests = 10) {
			Exception lastException = null;
			for ( int i = 0 ; i < numberOfRequests ; ++i ) {
				try {
					using ( Stream response = await Http.Client.GetStreamAsync(url) ) {
						XmlRootAttribute rootAttrib = null;
						if ( typeof(T) == typeof(Line) )
							rootAttrib = new XmlRootAttribute(LINES);
						else if ( typeof(T) == typeof(Stop) )
							rootAttrib = new XmlRootAttribute(STOPS);

						XmlSerializer deserializer = new XmlSerializer(typeof(List<T>), rootAttrib);

						return (List<T>)deserializer.Deserialize(response);
					}
				} catch ( Exception ex ) {
					lastException = ex;
				}
			}

			throw lastException;
		}

		/// <summary>
		/// Funkcja pobierajaca logi z serwera
		/// </summary>
		/// <param name="numberOfRequests">Ile razy zapytanie ma byc wykonane w razie niepowodzenia</param>
		/// <returns>Logi z serwera w formie listy</returns>
		public static async Task<IEnumerable<Log>> GetListOfLogsFromServer(int numberOfRequests = 10) {
			Exception lastException = null;
			for ( int i = 0 ; i < numberOfRequests ; ++i ) {
				try {
					string response = await Http.Client.GetStringAsync(Url.APP + Url.LOG);
					return Log.CreateFromXml(response);
				} catch ( Exception ex ) {
					lastException = ex;
				}
			}

			throw lastException;
		}
	}
}