using System;
using System.Collections.Generic;
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
        private readonly NpgsqlConnection _dbConnection;
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
                var query = "SELECT count(file_id) FROM private_files WHERE user_id = @userId";
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
                var query = "SELECT count(file_id) FROM private_files WHERE user_id = @userId AND is_controled = true";
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
                var query =
                    "SELECT count(transfer_id) FROM file_transfer WHERE to_user_id = @userId AND recived = false;";
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
                        result = (int) npgReader.GetValue(0);
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
                        result = (int) npgReader.GetValue(0);
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
                var query = "UPDATE users SET pass_hash = @passHash, mast_change_pass = 'false' WHERE user_id = @userId";
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
            string path = null;
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
                var query =
                    "SELECT file_id, file_name, size, load_time, name FROM common_files LEFT JOIN users ON users.user_id = common_files.user_id";
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
                    "ON users.user_id = private_files.user_id " +
                    "WHERE private_files.user_id = @userId";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
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
                var command =
                    "SELECT file_path FROM private_files WHERE file_id = @fileId AND encrypt_key_hash = @keyHash";
                NpgsqlCommand npgCommand = new NpgsqlCommand(command, _dbConnection);
                npgCommand.Parameters.Add("@fileId", NpgsqlDbType.Uuid).Value = fileId;
                npgCommand.Parameters.Add("@keyHash", NpgsqlDbType.Text).Value = keyHash;
                try
                {
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                    return null;
                }
                return null;
            }
        }

        public string GetPrivateFilePath(string fileId)
        {
            lock (_dbConnection)
            {
                var command = "SELECT file_path FROM private_files WHERE file_id = @fileId";
                NpgsqlCommand npgCommand = new NpgsqlCommand(command, _dbConnection);
                npgCommand.Parameters.Add("@fileId", NpgsqlDbType.Uuid).Value = fileId;
                try
                {
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                    return null;
                }
                return null;
            }
        }

        public void DeleteCommonFile(string fileId)
        {
            lock (_dbConnection)
            {
                var command = "DELETE FROM common_files WHERE file_id = @fileId";
                NpgsqlCommand npgCommand = new NpgsqlCommand(command, _dbConnection);
                npgCommand.Parameters.Add("@fileId", NpgsqlDbType.Uuid).Value = fileId;
                try
                {
                    npgCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                }
            }
        }

        public void DeletePrivateFile(string fileId)
        {
            lock (_dbConnection)
            {
                var command = "DELETE FROM private_files WHERE file_id = @fileId";
                NpgsqlCommand npgCommand = new NpgsqlCommand(command, _dbConnection);
                npgCommand.Parameters.Add("@fileId", NpgsqlDbType.Uuid).Value = fileId;
                try
                {
                    npgCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                }

            }
        }

        public void UpdateFileKey(string fileId, string keyHash)
        {
            lock (_dbConnection)
            {
                var query =
                    "UPDATE private_files SET encrypt_key_hash = @keyHash, is_encrypted = 'true' WHERE file_id = @fileId";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@keyHash", NpgsqlDbType.Text).Value = keyHash;
                command.Parameters.Add("@fileId", NpgsqlDbType.Uuid).Value = fileId;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                }
            }
        }

        public bool CheckFileHash(string fileId, string fileHash)
        {
            lock (_dbConnection)
            {
                var query = "SELECT file_id FROM private_files WHERE file_id = @fileId AND file_hash_sum = @fileHash";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@fileId", NpgsqlDbType.Uuid).Value = fileId;
                command.Parameters.Add("@fileHash", NpgsqlDbType.Text).Value = fileHash;
                try
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Close();
                        return true;
                    }
                    reader.Close();
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                    return false;
                }
            }
        }

        public void UpdateDamageStatus(string fileId, bool damaged)
        {
            lock (_dbConnection)
            {
                var query =
                    "UPDATE private_files SET is_damaged = @damaged, last_hash_check = @time WHERE file_id = @fileId";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@damaged", NpgsqlDbType.Boolean).Value = damaged;
                command.Parameters.Add("@fileId", NpgsqlDbType.Uuid).Value = fileId;
                command.Parameters.Add("@time", NpgsqlDbType.Text).Value = DateTime.Now;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                }
            }
        }

        public void UpdateFileHash(string fileId, string fileHash)
        {
            lock (_dbConnection)
            {
                var query =
                    "UPDATE private_files SET file_hash_sum = @hashSum WHERE file_id = @fileId";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@hashSum", NpgsqlDbType.Text).Value = fileHash;
                command.Parameters.Add("@fileId", NpgsqlDbType.Uuid).Value = fileId;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                }
            }
        }

        public string GetPublicKeyPath(string userId)
        {
            lock (_dbConnection)
            {
                var command =
                    "SELECT file_path FROM public_key_files WHERE user_id = @userId";
                NpgsqlCommand npgCommand = new NpgsqlCommand(command, _dbConnection);
                npgCommand.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = userId;
                try
                {
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                    return null;
                }
            }
            return null;
        }

        public string GetPrivateKeyPath(string userId, string keyHash)
        {
            lock (_dbConnection)
            {
                var command =
                    "SELECT file_path FROM private_key_files WHERE user_id = @userId AND file_key_hash = @keyHash";
                NpgsqlCommand npgCommand = new NpgsqlCommand(command, _dbConnection);
                npgCommand.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = userId;
                npgCommand.Parameters.Add("@keyHash", NpgsqlDbType.Text).Value = keyHash;
                try
                {
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                    return null;
                }
            }
            return null;
        }

        public void AddRsaKey(string userId, string privateFilePath, string publicFilePath, string keyHash)
        {
            lock (_dbConnection)
            {
                var query =
                    "INSERT INTO public_key_files (user_id, file_path) " +
                    "VALUES (@userId, @path)";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@path", NpgsqlDbType.Text).Value = publicFilePath;
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Не удалось сохранить файл ключа. user_id = " + _userId);
                    Console.WriteLine("DB error: " + ex.Message);
                }

                query =
                    "INSERT INTO private_key_files (user_id, file_path, file_key_hash) " +
                    "VALUES (@userId, @path, @keyHash)";
                command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@keyHash", NpgsqlDbType.Text).Value = keyHash;
                command.Parameters.Add("@path", NpgsqlDbType.Text).Value = privateFilePath;
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Не удалось сохранить файл ключа. user_id = " + _userId);
                    Console.WriteLine("DB error: " + ex.Message);
                }
            }
        }

        public void UpdateRsaKey(string userId, string privateFilePath, string publicFilePath, string keyHash)
        {
            lock (_dbConnection)
            {
                var query = "UPDATE public_key_files SET user_id = @userId, file_path = @path";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@path", NpgsqlDbType.Text).Value = publicFilePath;
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Не удалось сохранить файл ключа. user_id = " + _userId);
                    Console.WriteLine("DB error: " + ex.Message);
                }

                query = "UPDATE private_key_files SET user_id = @userId, file_path = @path, file_key_hash = @keyHash";
                command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@keyHash", NpgsqlDbType.Text).Value = keyHash;
                command.Parameters.Add("@path", NpgsqlDbType.Text).Value = privateFilePath;
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Не удалось сохранить файл ключа. user_id = " + _userId);
                    Console.WriteLine("DB error: " + ex.Message);
                }
            }
        }

        public string GetUserId(string login)
        {
            lock (_dbConnection)
            {
                var command =
                    "SELECT user_id FROM users WHERE login = @login";
                NpgsqlCommand npgCommand = new NpgsqlCommand(command, _dbConnection);
                npgCommand.Parameters.Add("@login", NpgsqlDbType.Text).Value = login;
                try
                {
                    npgCommand.ExecuteNonQuery();
                    var dbReader = npgCommand.ExecuteReader();
                    if (dbReader.HasRows)
                    {
                        dbReader.Read();
                        var id = dbReader.GetValue(0).ToString();
                        dbReader.Close();
                        return id;
                    }
                    dbReader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                    return null;
                }
            }
            return null;
        }

        public List<string> GetNewFileList()
        {
            var result = new List<string>();
            lock (_dbConnection)
            {
                var query =
                    "SELECT name, comment, send_time, file_name, transfer_id " +
                    "FROM file_transfer " +
                    "LEFT JOIN users ON users.user_id = file_transfer.from_user_id " +
                    "WHERE recived = 'false' AND to_user_id = @userId";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
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

        public List<string> GetReportList()
        {
            var result = new List<string>();
            lock (_dbConnection)
            {
                var query =
                    "SELECT name, comment, send_time, file_name, transfer_id, recived " +
                    "FROM file_transfer " +
                    "LEFT JOIN users ON users.user_id = file_transfer.to_user_id " +
                    "WHERE from_user_id = @userId";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
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
                        file += reader.GetValue(5).ToString();
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

        public void ShareFile(string filePath, string fileName, string toUserId, string encryptedKey, string comment)
        {
            lock (_dbConnection)
            {
                var query =
                    "INSERT INTO file_transfer (from_user_id, to_user_id, file_path, secret_key, send_time, file_name, comment)" +
                    " VALUES (@fromId, @toId, @path, @key, @time, @name, @comment)";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@name", NpgsqlDbType.Text).Value = fileName;
                command.Parameters.Add("@path", NpgsqlDbType.Text).Value = filePath;
                command.Parameters.Add("@time", NpgsqlDbType.Text).Value = DateTime.Now;
                command.Parameters.Add("@fromId", NpgsqlDbType.Uuid).Value = _userId;
                command.Parameters.Add("@toId", NpgsqlDbType.Uuid).Value = toUserId;
                command.Parameters.Add("@key", NpgsqlDbType.Text).Value = encryptedKey;
                command.Parameters.Add("@comment", NpgsqlDbType.Text).Value = comment;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Не удалось отправить файл. user_id = " + _userId);
                    Console.WriteLine("DB error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Вернет параметры переданного файла
        /// 1) путь до файла
        /// 2) имя файла
        /// 3) зашифрованный ключ для файла
        /// </summary>
        /// <param name="transferId"></param>
        /// <returns></returns>
        public string[] GetTransferParams(string transferId)
        {
            string[] transferParams = new string[3];
            lock (_dbConnection)
            {
                var command =
                    "SELECT file_path, file_name, secret_key FROM file_transfer  WHERE transfer_id = @transferId";
                NpgsqlCommand npgCommand = new NpgsqlCommand(command, _dbConnection);
                npgCommand.Parameters.Add("@transferId", NpgsqlDbType.Uuid).Value = transferId;
                try
                {
                    npgCommand.ExecuteNonQuery();
                    var dbReader = npgCommand.ExecuteReader();
                    if (dbReader.HasRows)
                    {
                        dbReader.Read();
                        transferParams[0] = dbReader.GetValue(0).ToString();
                        transferParams[1] = dbReader.GetValue(1).ToString();
                        transferParams[2] = dbReader.GetValue(2).ToString();
                        dbReader.Close();
                        return transferParams;
                    }
                    dbReader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                    return null;
                }
            }
            return null;
        }

        public void UpdateTransferStatus(string transferId)
        {
            lock (_dbConnection)
            {
                var query =
                    "UPDATE file_transfer SET recived = 'true' WHERE transfer_id = @transferId";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@transferId", NpgsqlDbType.Uuid).Value = transferId;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                }
            }
        }

        public List<string> GetUsersList()
        {
            var result = new List<string>();
            lock (_dbConnection)
            {
                var query =
                    "SELECT name, user_id " +
                    "FROM users "; /* +
                    "WHERE user_id != @userId";*/
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                //command.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = _userId;
                try
                {
                    var reader = command.ExecuteReader();
                    while (reader.HasRows)
                    {
                        reader.Read();
                        string file = "";
                        file += reader.GetValue(0) + "$";
                        file += reader.GetValue(1).ToString();
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

        public bool GetUserShareState(string userId)
        {
            lock (_dbConnection)
            {
                var command =
                    "SELECT allow_sharing FROM users WHERE user_id = @userId";
                NpgsqlCommand npgCommand = new NpgsqlCommand(command, _dbConnection);
                npgCommand.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = userId;
                try
                {
                    npgCommand.ExecuteNonQuery();
                    var dbReader = npgCommand.ExecuteReader();
                    if (dbReader.HasRows)
                    {
                        dbReader.Read();
                        var path = (bool) dbReader.GetValue(0);
                        dbReader.Close();
                        return path;
                    }
                    dbReader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                    return false;
                }
            }
            return false;
        }

        public bool MustChangePass(string userId)
        {
            lock (_dbConnection)
            {
                var command =
                    "SELECT mast_change_pass FROM users WHERE user_id = @userId";
                NpgsqlCommand npgCommand = new NpgsqlCommand(command, _dbConnection);
                npgCommand.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = userId;
                try
                {
                    npgCommand.ExecuteNonQuery();
                    var dbReader = npgCommand.ExecuteReader();
                    if (dbReader.HasRows)
                    {
                        dbReader.Read();
                        var path = (bool) dbReader.GetValue(0);
                        dbReader.Close();
                        return path;
                    }
                    dbReader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                    return false;
                }
            }
            return false;
        }

        public void AddNewUser(string login, string passHash, string name, bool isAdmin)
        {
            lock (_dbConnection)
            {
                var query =
                    "INSERT INTO users (login, pass_hash, name, is_admin) " +
                    "VALUES (@login, @pass_hash, @name, @is_admin)";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                command.Parameters.Add("@login", NpgsqlDbType.Text).Value = login;
                command.Parameters.Add("@pass_hash", NpgsqlDbType.Text).Value = passHash;
                command.Parameters.Add("@name", NpgsqlDbType.Text).Value = name;
                command.Parameters.Add("@is_admin", NpgsqlDbType.Boolean).Value = isAdmin;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Не удалось сохранить файл ключа. user_id = " + _userId);
                    Console.WriteLine("DB error: " + ex.Message);
                }
            }
        }

        public bool UserIsAdmin(string userId)
        {
            lock (_dbConnection)
            {
                var command =
                    "SELECT is_admin FROM users WHERE user_id = @userId";
                NpgsqlCommand npgCommand = new NpgsqlCommand(command, _dbConnection);
                npgCommand.Parameters.Add("@userId", NpgsqlDbType.Uuid).Value = userId;
                try
                {
                    npgCommand.ExecuteNonQuery();
                    var dbReader = npgCommand.ExecuteReader();
                    if (dbReader.HasRows)
                    {
                        dbReader.Read();
                        var path = (bool)dbReader.GetValue(0);
                        dbReader.Close();
                        return path;
                    }
                    dbReader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                    return false;
                }
            }
            return false;
        }

        public void SetMastChangePassAllUsers()
        {
            lock (_dbConnection)
            {
                var query =
                    "UPDATE users SET mast_change_pass = 'true'";
                NpgsqlCommand command = new NpgsqlCommand(query, _dbConnection);
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DB error: " + ex.Message);
                }
            }
        }

        #endregion
    }
}