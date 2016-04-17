using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Server;
using Npgsql;
using NpgsqlTypes;

namespace ExplorerServer.Core.DataBase
{
    /// <summary>
    /// Взаимодействие с базой данных.
    /// Объект создается для каждого пользователя и хранит некоторые пользовательские данные.
    /// </summary>
    public class DbController
    {
        private NpgsqlConnection _dbConnection;
        private string _userId;

        public DbController(NpgsqlConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        #region Public

        public string Authorization(string login, string passHash)
        {
            lock (_dbConnection)
            {
                var command = "SELECT user_id, name FROM users WHERE login = @login AND pass_hash = @pass";
                NpgsqlCommand npgCommand = new NpgsqlCommand(command, _dbConnection);
                npgCommand.Parameters.Add("@login", NpgsqlDbType.Text).Value = login;
                npgCommand.Parameters.Add("@pass", NpgsqlDbType.Text).Value = passHash;
                npgCommand.ExecuteNonQuery();
                var dbReader = npgCommand.ExecuteReader();
                if (dbReader.HasRows)
                {
                    dbReader.Read();
                    _userId = dbReader.GetValue(0).ToString();
                    var userName = dbReader.GetValue(1).ToString();
                    dbReader.Close();
                    return userName;
                }
                dbReader.Close();
                return null;
            }
        }

        public string GetCountUserFiles()
        {
            string result = String.Empty;
            lock (_dbConnection)
            {
                var query = "SELECT count(file_id) FROM files WHERE user_id = @userId";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
                try
                {
                    var npgReader = command.ExecuteReader();
                    if (npgReader.HasRows)
                    {
                        npgReader.Read();
                        result = npgReader.GetValue(0).ToString();
                        npgReader.Close();
                    }
                    npgReader.Close();
                }
                catch (Exception)
                {
                    result = "0";
                }
            }
            return result;
        }

        public string GetCountControlFiles()
        {
            string result = String.Empty;
            lock (_dbConnection)
            {
                var query = "SELECT count(file_id) FROM files WHERE user_id = @userId AND is_controled = true";
                var command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
                try
                {
                    var npgReader = command.ExecuteReader();
                    if (npgReader.HasRows)
                    {
                        npgReader.Read();
                        result = npgReader.GetValue(0).ToString();
                        npgReader.Close();
                    }
                    npgReader.Close();
                }
                catch (Exception)
                {
                    result = "0";
                }
            }
            return result;
        }

        public string GetCountEncryptedFiles()
        {
            string result = String.Empty;
            lock (_dbConnection)
            {
                var query = "SELECT count(file_id) FROM files WHERE user_id = @userId AND is_encrypted = true";
                var command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
                try
                {
                    var npgReader = command.ExecuteReader();
                    if (npgReader.HasRows)
                    {
                        npgReader.Read();
                        result = npgReader.GetValue(0).ToString();
                        npgReader.Close();
                    }
                    npgReader.Close();
                }
                catch (Exception)
                {
                    result = "0";
                }
            }
            return result;
        }

        public string GetCountNewFiles()
        {
            string result = String.Empty;
            lock (_dbConnection)
            {
                var query = "SELECT count(transfer_id) FROM file_transfer WHERE to_user_id = @userId AND recived = false;";
                var command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
                try
                {
                    var npgReader = command.ExecuteReader();
                    if (npgReader.HasRows)
                    {
                        npgReader.Read();
                        result = npgReader.GetValue(0).ToString();
                        npgReader.Close();
                    }
                    npgReader.Close();
                }
                catch (Exception)
                {
                    result = "0";
                }
            }
            return result;
        }

        public int GetFreeMemCount()
        {
            int result = -1;
            lock (_dbConnection)
            {
                var query = "SELECT free_memory FROM memory_state WHERE user_id = @userId";
                var command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
                try
                {
                    var npgReader = command.ExecuteReader();
                    if (npgReader.HasRows)
                    {
                        npgReader.Read();
                        result = (int)npgReader.GetValue(0);
                        npgReader.Close();
                    }
                    npgReader.Close();
                }
                catch (Exception)
                {
                    result = -1;
                }
            }
            return result;
        }

        public int GetTotalMemCount()
        {
            int result = -1;
            lock (_dbConnection)
            {
                var query = "SELECT total_memory FROM memory_state WHERE user_id = @userId";
                var command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
                try
                {
                    var npgReader = command.ExecuteReader();
                    if (npgReader.HasRows)
                    {
                        npgReader.Read();
                        result = (int)npgReader.GetValue(0);
                        npgReader.Close();
                    }
                    npgReader.Close();
                }
                catch (Exception)
                {
                    result = -1;
                }
            }
            return result;
        }

        public bool SetNewPassword(string passHash)
        {
            lock (_dbConnection)
            {
                var query = "UPDATE users SET pass_hash = @passHash WHERE user_id = @userId";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@passHash", NpgsqlDbType.Text).Value = passHash;
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        public string GetPublicKey(string userId)
        {
            string key = String.Empty;
            lock (_dbConnection)
            {
                var query = "SELECT public_key FROM users WHERE user_id = @userId";
                var command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = userId;
                try
                {
                    var npgReader = command.ExecuteReader();
                    if (npgReader.HasRows)
                    {
                        npgReader.Read();
                        key = npgReader.GetValue(0).ToString();
                        npgReader.Close();
                    }
                    npgReader.Close();
                }
                catch (Exception)
                {
                    key = String.Empty;
                }
            }
            return key;
        }

        public bool SetShareStatus(bool allow)
        {
            lock (_dbConnection)
            {
                var query = "UPDATE users SET allow_sharing = @allow WHERE user_id = @userId";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@allow", NpgsqlDbType.Boolean).Value = allow;
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        public bool SaveNewCommonFile(string fileName, string filePath, string fileSize)
        {
            lock (_dbConnection)
            {
                var query =
                    "INSERT INTO common_files (file_name, file_path, user_id, size, load_time) VALUES (@name, @path, @userId, @size, @time)";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@name", NpgsqlDbType.Text).Value = fileName;
                command.Parameters.Add("@path", NpgsqlDbType.Text).Value = filePath;
                command.Parameters.Add("@size", NpgsqlDbType.Text).Value = fileSize;
                command.Parameters.Add("@time", NpgsqlDbType.Text).Value = DateTime.Now;
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Не удалось сохранить файл. user_id = " + _userId);
                    Console.WriteLine("DB error: " + ex.Message);
                    return false;
                }
            }
            return true;
        }

        public string GetCommonFilePath(string fileId)
        {
            string path = String.Empty;
            lock (_dbConnection)
            {
                var query = "SELECT file_path FROM common_files WHERE file_id = @fileId";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@fileId", NpgsqlDbType.Uuid).Value = fileId;
                try
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        path = reader.GetValue(0).ToString();
                    }
                    reader.Close();
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return path;
        }

        public List<string> GetCommonFileList()
        {
            var result = new List<string>();
            lock (_dbConnection)
            {
                var query = "SELECT file_id, file_name, size, load_time, login FROM common_files LEFT JOIN users ON users.user_id = common_files.user_id";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                try
                {
                    var reader = command.ExecuteReader();
                    while (reader.HasRows)
                    {
                        reader.Read();
                        string file = "";
                        file += reader.GetValue(0) + "$";
                        file += reader.GetValue(1) + "$";
                        file += reader.GetValue(2) + "$";
                        file += reader.GetValue(3) + "$";
                        file += reader.GetValue(4).ToString();
                        result.Add(file);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                    return result;
                }
            }
            return result;
        }

        public List<string> GetPrivateFileList()
        {
            var result = new List<string>();
            lock (_dbConnection)
            {
                var query =
                    "SELECT file_id, file_name, file_size, load_time, login, is_damaged, is_encrypted, last_hash_check " +
                    "FROM private_files " +
                    "LEFT JOIN users " +
                    "ON users.user_id = private_files.user_id";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                try
                {
                    var reader = command.ExecuteReader();
                    while (reader.HasRows)
                    {
                        reader.Read();
                        string file = "";
                        file += reader.GetValue(0) + "$";
                        file += reader.GetValue(1) + "$";
                        file += reader.GetValue(2) + "$";
                        file += reader.GetValue(3) + "$";
                        file += reader.GetValue(4) + "$";
                        file += reader.GetValue(5) + "$";
                        file += reader.GetValue(6) + "$";
                        file += reader.GetValue(7).ToString();
                        result.Add(file);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                    return result;
                }
            }
            return result;
        }

        public bool SaveNewPrivateFile(string fileName, string filePath, string fileSize, string fileHash,
            bool isEncrypt, string keyHash)
        {
            lock (_dbConnection)
            {
                var query =
                    "INSERT INTO private_files (file_name, file_path, user_id, file_size, load_time, file_hash_sum, is_encrypted, is_damaged, encrypt_key_hash, last_hash_check) " +
                    "VALUES (@name, @path, @userId, @size, @time, @hashSum, @isEncrypt, 'false', @keyHash, @time)";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@name", NpgsqlDbType.Text).Value = fileName;
                command.Parameters.Add("@path", NpgsqlDbType.Text).Value = filePath;
                command.Parameters.Add("@size", NpgsqlDbType.Text).Value = fileSize;
                command.Parameters.Add("@time", NpgsqlDbType.Text).Value = DateTime.Now;
                command.Parameters.Add("@hashSum", NpgsqlDbType.Text).Value = fileHash;
                command.Parameters.Add("@isEncrypt", NpgsqlDbType.Boolean).Value = isEncrypt;
                command.Parameters.Add("@keyHash", NpgsqlDbType.Text).Value = keyHash;
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Не удалось сохранить файл. user_id = " + _userId);
                    Console.WriteLine("DB error: " + ex.Message);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Получение пути до файла по идентификатору и клчу
        /// При неверном ключе будет возвращено NULL
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="keyHash"></param>
        /// <returns></returns>
        public string GetPrivateFilePath(string fileId, string keyHash)
        {
            lock (_dbConnection)
            {
                var command = "SELECT file_path FROM private_files WHERE file_id = @fileId AND encrypt_key_hash = @keyHash";
                NpgsqlCommand npgCommand = new NpgsqlCommand(command, _dbConnection);
                npgCommand.Parameters.Add("@fileId", NpgsqlDbType.Uuid).Value = fileId;
                npgCommand.Parameters.Add("@keyHash", NpgsqlDbType.Text).Value = keyHash;
                npgCommand.ExecuteNonQuery();
                var dbReader = npgCommand.ExecuteReader();
                if (dbReader.HasRows)
                {
                    dbReader.Read();
                    var path = dbReader.GetValue(0).ToString();
                    dbReader.Close();
                    return path;
                }
                dbReader.Close();
                return null;
            }
        }

        #endregion
    }
}