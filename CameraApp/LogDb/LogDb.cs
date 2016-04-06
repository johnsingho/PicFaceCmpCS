using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Johnkit.Sqlite;
using JohnKit;

namespace CameraApp.DbLog
{
    internal class LogDb : IDisposable
    {
        #region Prop
        private HeSqlite dbHelper = new HeSqlite();
        private DbHandle db;
        private string strCurDBName;

        private static readonly string DEF_LOG_DB_FILENAME = "shebao.dat";
        //这不是最终密码，最终密码是： We_Want^girls*
        private static readonly string DEF_LOG_DB_PWD="we like girls.";
        private static readonly string DEF_LOG_DB_NAME= "TBL_LOG";
        //private static readonly long MAX_LOG_DB_SIZE= 500 * 1024 * 1024; //500MB
        private static readonly long MAX_LOG_DB_SIZE = 8 * 1024 * 1024; //test 1MB

        #endregion

        internal LogDb()
        {
            string strDBFile = Path.Combine(Application.StartupPath, DEF_LOG_DB_FILENAME);
            SetInitFileName(strDBFile);
        }
        private void SetInitFileName(string strInitDBName)
        {
            strCurDBName = strInitDBName;
        }

        private string GenNextFileName(string strInitDBName)
        {
            string strDir = Path.GetDirectoryName(strInitDBName);
            string strNameOnly = Path.GetFileNameWithoutExtension(strInitDBName);
            string strExt = Path.GetExtension(strInitDBName);
            int iEnd = strNameOnly.Length - 1;
            for (; iEnd >= 0; iEnd--)
            {
                if (!Char.IsDigit(strNameOnly[iEnd]))
                {
                    break;
                }
            }
            string strNewPath = "";
            string strPre = strNameOnly.Substring(0, iEnd + 1);
            int i = 0;
            while (true)
            {
                string strNewName = string.Format("{0}{1:D8}{2}", strPre,i, strExt);
                i++;

                strNewPath = Path.Combine(strDir, strNewName);
                if (CanWriteFile(strNewPath))
                {
                    WinCall.TraceMessage("**new db="+strNewPath);
                    break; //找到了
                }
            }
            return strNewPath;
        }

        private bool IsValidDB()
        {
            return db.DbPtr != IntPtr.Zero;
        }

        public bool TryOpen()
        {
            bool bWrite = CanWriteFile(strCurDBName);
            if (IsValidDB())
            {
                if (bWrite)
                {
                    return true;
                }
                else
                {
                    CloseDB();
                    strCurDBName = GenNextFileName(strCurDBName);
                }
            }
            else if (!bWrite)
            {
                strCurDBName = GenNextFileName(strCurDBName);
            }
            
            return OpenDB(strCurDBName, GetDefPs(DEF_LOG_DB_PWD));
        }

        private string GetDefPs(string defLogDbPwd)
        {
            //we like girls.
            //We_Want^girls*
            char[] byPs = defLogDbPwd.ToCharArray();
            byPs[0] = 'W';
            byPs[2] = '_';
            byPs[3] = 'W';
            byPs[7] = '^';
            byPs[13] = '*';
            byPs[4] = 'a';
            byPs[5] = 'n';
            byPs[6] = 't';
            return new string(byPs);
        }

        private bool OpenDB(string strCurDbName, string strPass)
        {
            if (0 != dbHelper.Open(strCurDbName, out db))
            {
                WinCall.TraceMessage(string.Format("***OpenDB {0} failed!",strCurDbName));
                return false;
            }
            string strPSstr=string.Format("PRAGMA key ='{0}'", strPass);
            if (!dbHelper.Exec(db, strPSstr))
            {
                WinCall.TraceMessage("***OpenDB, 密码不正确\n");
                CloseDB();
                return false;
            }
            if (!CreateTable())
            {
                CloseDB();
                return false;
            }
            return true;
        }

        private bool CreateTable()
        {
            if (dbHelper.CheckTableExist(db,DEF_LOG_DB_NAME))
            {
                return true;
            }
            string strCreate=string.Format("CREATE TABLE {0}([ID] INTEGER PRIMARY KEY,[name] VARCHAR(20),[IDNo] CHAR(20),"+
                                         "[UpdateTime] TimeStamp NOT NULL DEFAULT(datetime('now','localtime')),[IDPhoto] BLOB, [LivePhoto] BLOB,"+
                                         "[RecRate] FLOAT DEFAULT(0.0))",
                                         DEF_LOG_DB_NAME);
            return dbHelper.Exec(db, strCreate);
        }

        private void CloseDB()
        {
            dbHelper.Close(db);
            db.DbPtr = IntPtr.Zero;
        }

        //是否还能够写这个文件？
        private bool CanWriteFile(string strCurDbName)
        {
            FileInfo fi = new FileInfo(strCurDbName);
            return !fi.Exists || (fi.Length < MAX_LOG_DB_SIZE);
        }

        internal bool InsertRec(string strName, string strID, float fScore, Bitmap bmIDPhoto, Bitmap bmLivePhoto)
        {
            byte[] byIDPhoto = GetPhotoBytes(bmIDPhoto);
            byte[] byLivePhoto = null;
            //把现场照片缩小一点，减小数据库大小
            using (Bitmap bmLiveSmall = WinCall.BitmapScale(bmLivePhoto, 1024, 768))
            {
                byLivePhoto = GetPhotoBytes(bmLiveSmall);
            }
            
            string strIns=string.Format("INSERT INTO {0}(name,IDNo,RecRate,IDPhoto,LivePhoto) VALUES('{1}','{2}',{3},?,?)",
                          DEF_LOG_DB_NAME,
                          strName, strID,
                          fScore);
            var stmt = dbHelper.Prepare2(db, strIns);
            if (stmt.StmtPtr == IntPtr.Zero)
            {
                return false;
            }

            int iCol = 1;
            var err = dbHelper.BindBlob(stmt, iCol++, byIDPhoto, byIDPhoto.Length, IntPtr.Zero);
            if (err != SQLiteErrorCode.Ok)
            {
                return false;
            }
            err = dbHelper.BindBlob(stmt, iCol++, byLivePhoto, byLivePhoto.Length, IntPtr.Zero);
            if (err != SQLiteErrorCode.Ok)
            {
                return false;
            }
            dbHelper.Step(stmt);
            err=dbHelper.Finalize(stmt);
            return (err == SQLiteErrorCode.Ok);
        }

        private static byte[] GetPhotoBytes(Bitmap bm)
        {
            byte[] bys = new byte[0];
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bm.Save(memoryStream, ImageFormat.Jpeg);
                bys=memoryStream.ToArray();
            }
            return bys;
        }

        public void Dispose()
        {
            CloseDB();
        }
    }
}
