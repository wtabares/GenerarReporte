using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace GenerarReporteConsole
{
    class Program
    {

		public static string tag840 = @"BQT";
		public static string tag843 = @"BQR";
		public static string tag850 = @"BEG";
		
		public static string lin840 = @"PO1";

		static int Main(string[] args)
        {

			string rootFolder    = @ConfigurationManager.AppSettings["rootFolder"];
			string folder840Path = @rootFolder + ConfigurationManager.AppSettings["folder840Path"];
			string folder843Path = @rootFolder + ConfigurationManager.AppSettings["folder843Path"];
			string folder850Path = @rootFolder + ConfigurationManager.AppSettings["folder850Path"];

			try
            {
				string dateInput;
				// revisamos si los argumentos traen la fecha
				if (args.Length == 0)
				{
					dateInput = DateTime.Today.ToString();
				}
				else
				{
					dateInput = args[0];
				}

				ProcesarDirectorio(folder840Path, DateTime.Parse(dateInput), tag840);
				DisplayBreak('>', 2);
				ProcesarDirectorio(folder843Path, DateTime.Parse(dateInput), tag843);
				DisplayBreak('>', 2);
				ProcesarDirectorio(folder850Path, DateTime.Parse(dateInput), tag850);
				return 0;
			}
			catch (Exception e)
			{
				Console.WriteLine("Error: {0}", e.Message);
				return -1;
			}
		}

		static void ProcesarDirectorio(string path, DateTime date, string tag)
        {
			try
            {
				// filtrar solo los archivos de la fecha solicitada y ordenar por fecha
				string[] files = Directory.GetFiles(path)
					.Where(file => new FileInfo(file).LastWriteTime.Date == date.Date)
					.OrderBy(file => File.GetLastWriteTime(file))
					.ToArray();

				foreach (string file in files)
                {
					ReadFile(file, tag);
				}
            }
			catch (Exception e)
            {
				Console.WriteLine("Error: {0}", e.Message);
            }
        }

		static void ReadFile(string pathFile, string tag)
		{
			String numeroControl = "";
			String fileName = Path.GetFileName(pathFile);
			DateTime lastMod = File.GetLastWriteTime(pathFile);
			List<string> current = new List<string>();
			List<int> lineas = new List<int>();

			try
			{
				int currentLine = 1;
				int numLineas = 0;
				// Open the file text using a stream reader.
				foreach (var line in File.ReadAllLines(pathFile))
				{
					// obtener numero de control
					if (currentLine == 2)
                    {
						numeroControl = line.Substring(40,8);
					}
					if (line.Contains(tag))
					{
						if (tag840.Equals(tag))
                        {
							// logica para leer prop 840
							IEnumerable<string> tokens =
								!string.IsNullOrEmpty(line) ? line.Split(' ', StringSplitOptions.RemoveEmptyEntries) : Enumerable.Empty<string>();
							string token = tokens.Last();
							current.Add(token.Substring(0, token.Length-1));
							lineas.Add(numLineas);
							numLineas = 0;
						}
						else if (tag843.Equals(tag))
						{
							// logica para leer prop 843
							IEnumerable<string> tokens =
								!string.IsNullOrEmpty(line) ? line.Split(' ', StringSplitOptions.RemoveEmptyEntries) : Enumerable.Empty<string>();
							string token = tokens.Last();
							current.Add(token.Substring(0, token.Length - 1));
							lineas.Add(numLineas);
							numLineas = 0;
						}
						else if (tag850.Equals(tag))
						{
							// agregar logica para prop 850 - BEG)
							string token = !string.IsNullOrEmpty(line) ? line.Substring(65, 30) : "";
							current.Add(token.Trim());
							lineas.Add(numLineas);
							numLineas = 0;
						}
					}

					// identificar líneas de las prop edi840
					if (line.Contains(lin840))
                    {
						numLineas++;
                    }
					currentLine++;
				}
				lineas.Add(numLineas);
				DisplayData(fileName, lastMod.ToString(), numeroControl, current, lineas);
			}
			catch (IOException e)
			{
				Console.WriteLine("The file could not be read: {0}", e.Message);
			}
		}


		public static void DisplayData(string fileName, string lastMod, string noControl, List<string> props, List<int> lineas)
        {
			bool isEmpty = props.Count == 0;
			bool isLineasEmpty = lineas.Count == 0;

			if (isEmpty)
            {
				Console.WriteLine("{0},{1},{2},{3},{4}", fileName, lastMod, noControl, "NO DATA", "NO LINES");
			}
			else
            {
				// Show content
				int lin = 1;
				foreach (var item in props)
				{
					int numLineas = !isLineasEmpty ? lineas.ElementAt(lin) : 0;
					if (lin == 1)
					{
						Console.WriteLine("{0},{1},{2},{3},{4}", fileName, lastMod, noControl, item.Trim(), numLineas);
					}
					else
					{
						Console.WriteLine(",,,{0},{1}", item.Trim(), numLineas);
					}
					lin++;
				}
			}
			DisplayBreak('*', 1);
		}

		public static void DisplayBreak(Char caracter, int times)
        {
			for (int i = 1; i <= times; i++)
			{ 
				Console.WriteLine();
				Console.WriteLine(new string(caracter, 100));
				Console.WriteLine();
			}
		}
	}
}
