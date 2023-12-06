using System.Net;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace Recuperation_fichier
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Combien de fichiers doit-on récupérer ?");
            int nb = int.Parse(Console.ReadLine());
            for (int i = 1; i <= nb; i++)
            {
                Console.WriteLine("Quel est le chemin du fichier voulu");
                string pathrecup = Console.ReadLine();
                string contenu = recuperation(pathrecup);
                string pathcopie = copie(contenu, pathrecup);
                stats(pathcopie);
                Hashtable loueurs = new Hashtable();
                loueurs = traitementstats(pathcopie);
                alimentationBDD(loueurs);
                Console.WriteLine("Fichier numéro {0} fini", i);
            }

            static string recuperation(string pathrecup)
            {
                Console.WriteLine("Identifiant de l'utilisateur");
                string Username = Console.ReadLine();
                Console.WriteLine("Mot de passe de l'utilisateur");
                string password = Console.ReadLine();
                string contient = "";
                FtpWebResponse response = null;
                Stream responseStream = null;
                StreamReader reader = null;
                FtpWebRequest request = null;
                System.IO.StreamReader readStream = null;
                StreamWriter writeStream = null;

                try
                {
                    request = (FtpWebRequest)WebRequest.Create(pathrecup);
                    
                    request.Method = WebRequestMethods.Ftp.DownloadFile;
                    request.Credentials = new NetworkCredential(Username, password);
                    response = (FtpWebResponse)request.GetResponse();
                    responseStream = response.GetResponseStream();
                    reader = new StreamReader(responseStream);
                    contient= reader.ReadToEnd();
                    if (readStream != null)
                    {
                        writeStream = new StreamWriter(contient, false);
                        writeStream.Write(readStream.ReadToEnd());
                    }
                }
                finally
                {
                    response.Close();
                    responseStream.Close();
                }
                return contient;
            }

            static string copie(string contenu, string pathrecup)
            {
                Console.WriteLine("Destination du contenu copié");
                string pathcopie = Console.ReadLine();
                File.WriteAllText(pathcopie, contenu);
                return pathcopie;
            }

            static void stats(string pathcopie)
            {
                string[] texte = File.ReadAllLines(pathcopie);
                int appelsansretours = 0;
                int timeouts = 0;
                int autres = 0;
                foreach (string ligne in texte)
                {
                    if (ligne.Contains("RATE"))
                    {
                        if (ligne.Contains("KO"))
                        {
                            appelsansretours++;
                        }
                        if (ligne.Contains("WebException"))
                        {
                            timeouts++;
                        }
                    }
                    else
                    {
                        autres++;
                    }
                }
                Console.WriteLine("Il y a eu {0} appels sans retours, {1} timeouts et {2} autres erreurs", appelsansretours, timeouts, autres);
            }
            

            static Hashtable traitementstats(string path)
            {
                Hashtable listeloueurs;
                Loueur loueur;
                int ko;
                int timeout;
                int id;
                string nom;
                listeloueurs = new Hashtable();
                using (StreamReader sr = new StreamReader(path))
                {
                    string ligne;
                    while ((ligne = sr.ReadLine()) != null)
                    {
                        ko = 0;
                        timeout = 0;
                        id = 0;
                        nom = "";
                        Loueur loueurEnCours;


                        string[] partsligne = ligne.Split(" ");
                        nom = partsligne[2].Split("(")[0];
                        if (partsligne[2].Contains('('))
                        {
                            string idpart = partsligne[2].Split('(', ')')[1];
                            if (idpart != "")
                            {
                                id = int.Parse(idpart);
                            }
                        }
                        if (listeloueurs.ContainsKey(id))
                        {
                            if (partsligne[3].Contains("RATE"))
                            {
                                loueurEnCours = (Loueur) listeloueurs[id];
                                if (ligne.Contains("KO"))
                                {
                                    loueurEnCours.Ko += 1;
                                }
                                if (ligne.Contains("WebException"))
                                {
                                    loueurEnCours.Timeout += 1;
                                }
                            }
                        }
                        else
                        {
                            if (partsligne[3].Contains("RATE"))
                            {
                                if (ligne.Contains("KO"))
                                {
                                    ko++;
                                }
                                if (ligne.Contains("WebException"))
                                {
                                    timeout++;
                                }
                            }
                            loueur = new Loueur(nom, id, ko, timeout);
                            listeloueurs.Add(id, loueur);
                        }
                    }
                }
                foreach (Loueur stat in listeloueurs.Values)
                {
                    Console.WriteLine("Loueur avec l'id {0} et le nom {1} a reçu {2} timeout(s) et {3} retour(s) KO", stat.Id, stat.Name, stat.Timeout, stat.Ko);
                }
                
                return listeloueurs;
            }

            static void alimentationBDD(Hashtable liste)
            {
                string server;
                string database;
                string idDatabase;
                string passwordDatabase;
                Console.WriteLine("Nom du serveur - BDD");
                server = Console.ReadLine();
                Console.WriteLine("Nom de la BDD - BDD");
                database = Console.ReadLine();
                Console.WriteLine("Id login de la BDD - BDD");
                idDatabase = Console.ReadLine();
                Console.WriteLine("Mot de passe de la BDD - BDD");
                passwordDatabase = Console.ReadLine();
                //string infosLogIn = "server=localhost;database=projet_log;uid=root;pwd=;";
                string infosLogIn = "server=" + server + ";database=" + database + ";uid=" + idDatabase + ";pwd=" + passwordDatabase + ";";
                string sql;
                MySqlCommand ligne;
                MySqlDataReader reader;
                MySqlConnection connection = new MySqlConnection(infosLogIn);
                connection.Open();
                foreach (Loueur loueur in liste.Values)
                {
                    sql = "Insert into loueur (IdLoueur, NomLoueur, RetoursKoLoueur, TimeoutsLoueur) values ('" + loueur.Id + "', '" + loueur.Name + "', '" + loueur.Ko + "', '" + loueur.Timeout + "')";
                    liste.Remove(loueur);
                    ligne = new MySqlCommand(sql, connection);
                    reader = ligne.ExecuteReader();
                    reader.Close();
                }
                connection.Close();
            }
        }
    }
}