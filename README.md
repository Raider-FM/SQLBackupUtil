# SQLBackupUtil
Small console utility for multiple DB backups. 
Supports zip compression with password protection and shared network resources as a backup location. 
Supports sqlserver 2012+ (tested on 2016 Express also)
To configure - edit config file in the application directory.
Parameters:
- "SQLServer" - name of the source SQL server
- "SQLUser" -
- "SQLPassword" -
- "DBList" - comma separated list of source DBs (Example: "testDB1,testDB2")
- "DelDays" - int value of the last archive (files older then <DelDays> will be deleted)
- "KeepOnly" - mask of prev day files that would be kept,"*" - ignore parameter (Example KeepOnly="23_0" means that backups created on 23:00 will be kept only)
- "BackupDir" - temporary dir on the server to store DB backup file and archive (Example: "D:\\DBBackupTemp\\")
- "NetworkBackup" - trigger of the network backup (true/false)
- "NetworkDir" - path to the shared network resource to store DB archive (Example: "////Someserver//BackupDB//")
- "ExecSQLBefore" - sql command that would be executed BEFORE backup process. Could be empty. (Example: "exec stp_StopProcessing" )
- "ExecSQLAfter" - sql command that would be executed AFTER backup process. Could be empty. (Example: "exec stp_CleanAll" )
- "ExecSQLDBName" - sql DB context for "ExecSQLBefore" and "ExecSQLAfter" (Example: "masterDB")
- "ArchivePassword" - password for DB archives.

Platform: NET Framework 4/C#
