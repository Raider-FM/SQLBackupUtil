using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Configuration;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Ionic.Zip;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Starting...");
        Console.WriteLine("Processing parameters...");
        String SQLServer = ConfigurationSettings.AppSettings["SQLServer"]; ;
        String SQLUser = ConfigurationSettings.AppSettings["SQLUser"]; ;
        String SQLPassword = ConfigurationSettings.AppSettings["SQLPassword"]; ;
        String DBList = ConfigurationSettings.AppSettings["DBList"]; ;
        String Backup_dir = ConfigurationSettings.AppSettings["BackupDir"];
        String Network_dir = ConfigurationSettings.AppSettings["NetworkDir"];
        int DelDays = Convert.ToInt16(ConfigurationSettings.AppSettings["DelDays"]);
        String KeepOnly = ConfigurationSettings.AppSettings["KeepOnly"];
        String password = ConfigurationSettings.AppSettings["Password"];
        bool NetworkBackup = Convert.ToBoolean(ConfigurationSettings.AppSettings["NetworkBackup"]);
        String strSQL = ConfigurationSettings.AppSettings["ExecSQLBefore"];
        String ExecSQLDBName = ConfigurationSettings.AppSettings["ExecSQLDBName"];
        Console.WriteLine("..done");

        DateTime dt_now = DateTime.Now;
        String fileDate = "";
        fileDate = dt_now.Year.ToString() + "-" + dt_now.Month.ToString() + "-" + dt_now.Day.ToString() + " " + dt_now.Hour.ToString() + "_" + dt_now.Minute.ToString() + ".bak";

        if (strSQL != "")
        {
            SqlCommand cmd = new SqlCommand(strSQL, new SqlConnection("server=" + SQLServer + ";user id=" + SQLUser + ";password=" + SQLPassword + ";database=" + ExecSQLDBName));
            cmd.Connection.Open();
            cmd.CommandTimeout = 600;
            Console.WriteLine("Exec SQL Before...");
            try
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine("..done");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                cmd.Connection.Close();
            }
        }

        foreach (String DBName in DBList.Split(','))
        {
            String filename = "";
            filename = DBName + "_" + fileDate + ".bak";
            BackupDeviceItem bdi =
                new BackupDeviceItem(Backup_dir + filename
                , DeviceType.File);
            Backup bu = new Backup();
            bu.Database = DBName;
            bu.Devices.Add(bdi);
            bu.Initialize = true;
            bu.PercentComplete +=
                new PercentCompleteEventHandler(Backup_PercentComplete);
            bu.Complete += new ServerMessageEventHandler(Backup_Complete);

            Server server = new Server(SQLServer);
            Console.WriteLine("Processing DB: "+DBName);
            bu.SqlBackup(server);

            Console.WriteLine("Adding to archive {0}...", filename);

            ZipFile zip = new ZipFile();
            zip.UseZip64WhenSaving = Zip64Option.Always;
            ZipEntry ent = zip.AddFile(Backup_dir + filename);
            if (password != "")
            {
                ent.Password = password;
            }
            zip.Save(Backup_dir + filename + ".zip");

            //Console.WriteLine(DBName + " backup is finished!");
            Console.WriteLine("..done");
        }
                
        strSQL = ConfigurationSettings.AppSettings["ExecSQLAfter"];
        if (strSQL != "")
        {
            SqlCommand cmd = new SqlCommand(strSQL, new SqlConnection("server=" + SQLServer + ";user id=" + SQLUser + ";password=" + SQLPassword + ";database=" + ExecSQLDBName));
            cmd.Connection.Open();
            cmd.CommandTimeout = 600;
            Console.WriteLine("Exec SQL After...");
            try
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine("..done");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                cmd.Connection.Close();
            }
        }

        String kp = "";
        if (KeepOnly != "")
        {
            kp = KeepOnly + ".bak.zip";
        }

        deleteOldFiles(Backup_dir, DateTime.Now.AddDays(-DelDays), kp);
        if (NetworkBackup)
        {
            Console.WriteLine("Processing network dir...");
            deleteOldFiles(Network_dir, DateTime.Now.AddDays(-DelDays), kp);
        }
        Console.WriteLine("All done!");
    }

    static private void CreateBackupArchive(string DBName){

    }

    protected static void Backup_PercentComplete(
        object sender, PercentCompleteEventArgs e)
    {
        Console.WriteLine(e.Percent + "% processed.");
    }

    protected static void Backup_Complete(object sender, ServerMessageEventArgs e)
    {
        Console.WriteLine(Environment.NewLine + e.ToString());
    }

    static private void deleteOldFiles(string path, DateTime olderThanDate, string ExMask)
    {
        bool NetworkBackup = Convert.ToBoolean(ConfigurationSettings.AppSettings["NetworkBackup"]);
        String Network_dir = ConfigurationSettings.AppSettings["NetworkDir"];
        DirectoryInfo dirInfo = new DirectoryInfo(path);

        FileInfo[] files = dirInfo.GetFiles();
        foreach (FileInfo file in files)
        {
            System.Console.WriteLine(
                  String.Format("Found file {0}", file.Name)
            );
            if ((file.LastWriteTime < olderThanDate) || (file.Extension != ".zip"))
            {
                System.Console.WriteLine(
                      String.Format("Delete {0}.", file.FullName)
                );
                file.IsReadOnly = false;
                file.Delete();

            }
            else if ((file.LastWriteTime < DateTime.Now.AddDays(-3)) && !(file.Name.Contains(ExMask)))
            {
                System.Console.WriteLine(
                      String.Format("Delete {0}.", file.FullName)
                );
                file.IsReadOnly = false;
                file.Delete();                
            }
            else if ((path != Network_dir) && (NetworkBackup))
            {
                if(!File.Exists(Network_dir + file.Name)){
                    System.Console.WriteLine(
                          String.Format("Copy to network {0}.", Network_dir + file.Name)
                    );
                    file.CopyTo(Network_dir + file.Name);
                }
            }

        }
    }
}



