using System;
using System.Runtime.CompilerServices;
using Npgsql;
using NpgsqlTypes;

namespace ExplorerServer.Core.DataBase
{
    public class DbController
    {
        private NpgsqlConnection _dbConnection = null;
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

        #endregion
    }
}