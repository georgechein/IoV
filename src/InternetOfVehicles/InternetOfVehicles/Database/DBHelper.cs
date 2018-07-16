using System;
using System.Data;
using System.Data.Common;

namespace DBLibrary.Database
{
    public class DBHelper
    {
        #region Declaration 

        private string sProviderName = "";
        private string sConnectionString = "";
        private DbProviderFactory oFactory;
        private DbConnection oConnection;
        //private ConnectionState oConnectionState;
        private DbCommand oCommand;
        private DbParameter oParameter;
        private DbTransaction oTransaction;
        private bool mTransaction;

        public ConnectionState ConnectionState
        {
            get
            {
                //return oConnectionState;
                return oConnection == null ? ConnectionState.Closed : oConnection.State;
            }
        }

        public DbCommand Command
        {
            get
            {
                return oCommand;
            }
        }

        #endregion

        #region Structure

        public struct Parameters
        {
            public string ParamName;
            public object ParamValue;
            public ParameterDirection ParamDirection;
            public DbType ParamType;
            public int ParamSize;

            public Parameters(string Name, object Value, ParameterDirection Direction, DbType Type = DbType.String, int Size = 0) : this()
            {
                ParamName = Name;
                ParamValue = Value;
                ParamDirection = Direction;
                ParamType = Type;
                ParamSize = Size;
            }

            public Parameters(string Name, object Value, DbType Type = DbType.String, int Size = 0) : this()
            {
                ParamName = Name;
                ParamValue = Value;
                ParamDirection = ParameterDirection.Input;
                ParamType = Type;
                ParamSize = Size;
            }
        }

        #endregion

        #region Constructor 

        public DBHelper(string providerName, string connectionString)
        {
            if (string.IsNullOrEmpty(providerName))
                throw new DBHelperException("The ProviderName is missing.");

            if (string.IsNullOrEmpty(providerName))
                throw new DBHelperException("The ConnectionString is missing.");

            sProviderName = providerName;
            sConnectionString = connectionString;
            oFactory = DbProviderFactories.GetFactory(sProviderName);
        }

        #endregion

        #region Destructor 

        ~DBHelper()
        {
            oFactory = null;
        }

        #endregion

        #region Connection

        public void OpenFactoryConnection()
        {
            oConnection = oFactory.CreateConnection();

            if (oConnection.State == ConnectionState.Closed)
            {
                oConnection.ConnectionString = sConnectionString;
                oConnection.Open();
                //oConnectionState = ConnectionState.Open;
            }
        }

        public void CloseFactoryConnection()
        {
            try
            {
                if (oConnection.State == ConnectionState.Open)
                {
                    oConnection.Close();
                    //oConnectionState = ConnectionState.Closed;
                }
            }
            catch (DbException dbEx)
            {
                throw new DBHelperException(dbEx.Message, dbEx);
            }
            catch (System.NullReferenceException nullEx)
            {
                throw new DBHelperException(nullEx.Message, nullEx);
            }
            finally
            {
                if (oConnection != null)
                    oConnection.Dispose();
            }
        }

        #endregion

        #region Transaction 

        public void BeginTransaction()
        {
            try
            {
                oTransaction = oConnection.BeginTransaction();
                mTransaction = true;
            }
            catch (InvalidOperationException ex)
            {
                throw new DBHelperException(ex.Message, ex);
            }
        }

        public void Commit()
        {
            if (oTransaction.Connection != null)
            {
                try
                {
                    oTransaction.Commit();
                    mTransaction = false;
                }
                catch (InvalidOperationException ex)
                {
                    throw new DBHelperException(ex.Message, ex);
                }
            }
        }

        public void Rollback()
        {
            try
            {
                if (mTransaction)
                {
                    oTransaction.Rollback();
                }
                mTransaction = false;
            }
            catch (InvalidOperationException ex)
            {
                throw new DBHelperException(ex.Message, ex);
            }
        }

        #endregion

        #region COMMANDS 

        #region PARAMETERLESS METHODS 

        /// <summary>
        ///Description	    :	This function is used to Prepare Command For Execution
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Transaction, Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	NA
        ///Comments			:	Has to be changed/removed if object based array concept is removed.
        /// </summary>
        private void PrepareCommand(bool blTransaction, CommandType cmdType, string cmdText)
        {

            if (oConnection.State != ConnectionState.Open)
            {
                oConnection.ConnectionString = sConnectionString;
                oConnection.Open();
                //oConnectionState = ConnectionState.Open;
            }

            if (null == oCommand)
                oCommand = oFactory.CreateCommand();

            oCommand.Connection = oConnection;
            oCommand.CommandText = cmdText;
            oCommand.CommandType = cmdType;

            if (blTransaction)
                oCommand.Transaction = oTransaction;
        }

        #endregion

        #region OBJECT BASED PARAMETER ARRAY 

        /// <summary>
        ///Description	    :	This function is used to Prepare Command For Execution
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Transaction, Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	NA
        ///Comments			:	
        /// </summary>
        private void PrepareCommand(bool blTransaction, CommandType cmdType, string cmdText, object[,] cmdParms)
        {

            if (oConnection.State != ConnectionState.Open)
            {
                oConnection.ConnectionString = sConnectionString;
                oConnection.Open();
                //oConnectionState = ConnectionState.Open;
            }

            if (null == oCommand)
                oCommand = oFactory.CreateCommand();

            oCommand.Connection = oConnection;
            oCommand.CommandText = cmdText;
            oCommand.CommandType = cmdType;

            if (blTransaction)
                oCommand.Transaction = oTransaction;

            if (null != cmdParms)
                CreateDBParameters(cmdParms);
        }

        #endregion

        #region STRUCTURE BASED PARAMETER ARRAY 

        /// <summary>
        ///Description	    :	This function is used to Prepare Command For Execution
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Transaction, Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	NA
        ///Comments			:	
        /// </summary>
        private void PrepareCommand(bool blTransaction, CommandType cmdType, string cmdText, Parameters[] cmdParms)
        {

            if (oConnection.State != ConnectionState.Open)
            {
                oConnection.ConnectionString = sConnectionString;
                oConnection.Open();
                //oConnectionState = ConnectionState.Open;
            }

            oCommand = oFactory.CreateCommand();
            oCommand.Connection = oConnection;
            oCommand.CommandText = cmdText;
            oCommand.CommandType = cmdType;

            if (blTransaction)
                oCommand.Transaction = oTransaction;

            if (null != cmdParms)
                CreateDBParameters(cmdParms);
        }

        #endregion

        #endregion

        #region PARAMETER METHODS 

        #region OBJECT BASED 

        /// <summary>
        ///Description	    :	This function is used to Create Parameters for the Command For Execution
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	2-Dimensional Parameter Array
        ///OutPut			:	NA
        ///Comments			:	
        /// </summary>
        private void CreateDBParameters(object[,] colParameters)
        {
            for (int i = 0; i < colParameters.Length / 2; i++)
            {
                oParameter = oCommand.CreateParameter();
                oParameter.ParameterName = colParameters[i, 0].ToString();
                oParameter.Value = colParameters[i, 1];
                oCommand.Parameters.Add(oParameter);
            }
        }

        #endregion

        #region STRUCTURE BASED 

        /// <summary>
        ///Description	    :	This function is used to Create Parameters for the Command For Execution
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	2-Dimensional Parameter Array
        ///OutPut			:	NA
        ///Comments			:	
        /// </summary>
        private void CreateDBParameters(Parameters[] colParameters)
        {
            for (int i = 0; i < colParameters.Length; i++)
            {
                Parameters oParam = (Parameters)colParameters[i];

                oParameter = oCommand.CreateParameter();
                oParameter.ParameterName = oParam.ParamName;
                oParameter.Value = oParam.ParamValue;
                oParameter.Direction = oParam.ParamDirection;
                oParameter.DbType = oParam.ParamType;
                oParameter.Size = oParam.ParamSize;
                oCommand.Parameters.Add(oParameter);

            }
        }

        #endregion

        #endregion

        #region EXCEUTE METHODS 

        #region PARAMETERLESS METHODS 

        /// <summary>
        ///Description	    :	This function is used to Execute the Command
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	Count of Records Affected
        ///Comments			:	
        ///                     Has to be changed/removed if object based array concept is removed.
        /// </summary>
        public int ExecuteNonQuery(CommandType cmdType, string cmdText)
        {
            try
            {

                OpenFactoryConnection();
                PrepareCommand(false, cmdType, cmdText);
                return oCommand.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (null != oCommand)
                    oCommand.Dispose();
                CloseFactoryConnection();
            }
        }

        /// <summary>
        ///Description	    :	This function is used to Execute the Command
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Transaction, Command Type, Command Text, 2-Dimensional Parameter Array, Clear Paramaeters
        ///OutPut			:	Count of Records Affected
        ///Comments			:	
        ///                     Has to be changed/removed if object based array concept is removed.
        /// </summary>
        public int ExecuteNonQuery(bool blTransaction, CommandType cmdType, string cmdText)
        {
            try
            {
                PrepareCommand(blTransaction, cmdType, cmdText);
                int val = oCommand.ExecuteNonQuery();

                return val;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (null != oCommand)
                    oCommand.Dispose();
            }
        }

        #endregion

        #region OBJECT BASED PARAMETER ARRAY 

        /// <summary>
        ///Description	    :	This function is used to Execute the Command
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, 2-Dimensional Parameter Array, Clear Parameters
        ///OutPut			:	Count of Records Affected
        ///Comments			:	
        /// </summary>
        public int ExecuteNonQuery(CommandType cmdType, string cmdText, object[,] cmdParms, bool blDisposeCommand)
        {
            try
            {

                OpenFactoryConnection();
                PrepareCommand(false, cmdType, cmdText, cmdParms);
                return oCommand.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (blDisposeCommand && null != oCommand)
                    oCommand.Dispose();
                CloseFactoryConnection();
            }
        }

        /// <summary>
        ///Description	    :	This function is used to Execute the Command
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	Count of Records Affected
        ///Comments			:	Overloaded method. 
        /// </summary>
        public int ExecuteNonQuery(CommandType cmdType, string cmdText, object[,] cmdParms)
        {
            return ExecuteNonQuery(cmdType, cmdText, cmdParms, true);
        }

        /// <summary>
        ///Description	    :	This function is used to Execute the Command
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Transaction, Command Type, Command Text, 2-Dimensional Parameter Array, Clear Paramaeters
        ///OutPut			:	Count of Records Affected
        ///Comments			:	
        /// </summary>
        public int ExecuteNonQuery(bool blTransaction, CommandType cmdType, string cmdText, object[,] cmdParms, bool blDisposeCommand)
        {
            try
            {

                PrepareCommand(blTransaction, cmdType, cmdText, cmdParms);
                return oCommand.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (blDisposeCommand && null != oCommand)
                    oCommand.Dispose();
            }
        }

        /// <summary>
        ///Description	    :	This function is used to Execute the Command
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Transaction, Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	Count of Records Affected
        ///Comments			:	Overloaded function. 
        /// </summary>
        public int ExecuteNonQuery(bool blTransaction, CommandType cmdType, string cmdText, object[,] cmdParms)
        {
            return ExecuteNonQuery(blTransaction, cmdType, cmdText, cmdParms, true);
        }

        #endregion

        #region STRUCTURE BASED PARAMETER ARRAY 

        /// <summary>
        ///Description	    :	This function is used to Execute the Command
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, Parameter Structure Array, Clear Parameters
        ///OutPut			:	Count of Records Affected
        ///Comments			:	
        /// </summary>
        public int ExecuteNonQuery(CommandType cmdType, string cmdText, Parameters[] cmdParms, bool blDisposeCommand)
        {
            try
            {

                OpenFactoryConnection();
                PrepareCommand(false, cmdType, cmdText, cmdParms);
                return oCommand.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (blDisposeCommand && null != oCommand)
                    oCommand.Dispose();
                CloseFactoryConnection();
            }
        }

        /// <summary>
        ///Description	    :	This function is used to Execute the Command
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, Parameter Structure Array
        ///OutPut			:	Count of Records Affected
        ///Comments			:	Overloaded method. 
        /// </summary>
        public int ExecuteNonQuery(CommandType cmdType, string cmdText, Parameters[] cmdParms)
        {
            return ExecuteNonQuery(cmdType, cmdText, cmdParms, true);
        }

        /// <summary>
        ///Description	    :	This function is used to Execute the Command
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Transaction, Command Type, Command Text, Parameter Structure Array, Clear Parameters
        ///OutPut			:	Count of Records Affected
        ///Comments			:	
        /// </summary>
        public int ExecuteNonQuery(bool blTransaction, CommandType cmdType, string cmdText, Parameters[] cmdParms, bool blDisposeCommand)
        {
            try
            {

                PrepareCommand(blTransaction, cmdType, cmdText, cmdParms);
                return oCommand.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (blDisposeCommand && null != oCommand)
                    oCommand.Dispose();
            }
        }

        /// <summary>
        ///Description	    :	This function is used to Execute the Command
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Transaction, Command Type, Command Text, Parameter Structure Array
        ///OutPut			:	Count of Records Affected
        ///Comments			:	
        /// </summary>
        public int ExecuteNonQuery(bool blTransaction, CommandType cmdType, string cmdText, Parameters[] cmdParms)
        {
            return ExecuteNonQuery(blTransaction, cmdType, cmdText, cmdParms, true);
        }

        #endregion

        #endregion

        #region READER METHODS 

        #region PARAMETERLESS METHODS 

        /// <summary>
        ///Description	    :	This function is used to fetch data using Data Reader	
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	Data Reader
        ///Comments			:	
        ///                     Has to be changed/removed if object based array concept is removed.
        /// </summary>
        public DbDataReader ExecuteReader(CommandType cmdType, string cmdText)
        {

            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            try
            {

                OpenFactoryConnection();
                PrepareCommand(false, cmdType, cmdText);
                DbDataReader dr = oCommand.ExecuteReader(CommandBehavior.CloseConnection);
                oCommand.Parameters.Clear();
                return dr;

            }
            catch (Exception ex)
            {
                CloseFactoryConnection();
                throw ex;
            }
            finally
            {
                if (null != oCommand)
                    oCommand.Dispose();
            }
        }

        #endregion

        #region OBJECT BASED PARAMETER ARRAY 

        /// <summary>
        ///Description	    :	This function is used to fetch data using Data Reader	
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	Data Reader
        ///Comments			:	
        /// </summary>
        public DbDataReader ExecuteReader(CommandType cmdType, string cmdText, object[,] cmdParms)
        {

            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work

            try
            {

                OpenFactoryConnection();
                PrepareCommand(false, cmdType, cmdText, cmdParms);
                DbDataReader dr = oCommand.ExecuteReader(CommandBehavior.CloseConnection);
                oCommand.Parameters.Clear();
                return dr;

            }
            catch (Exception ex)
            {
                CloseFactoryConnection();
                throw ex;
            }
            finally
            {
                if (null != oCommand)
                    oCommand.Dispose();
            }
        }

        #endregion

        #region STRUCTURE BASED PARAMETER ARRAY 

        /// <summary>
        ///Description	    :	This function is used to fetch data using Data Reader	
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, Parameter AStructure Array
        ///OutPut			:	Data Reader
        ///Comments			:	
        /// </summary>
        public DbDataReader ExecuteReader(CommandType cmdType, string cmdText, Parameters[] cmdParms)
        {

            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            try
            {

                OpenFactoryConnection();
                PrepareCommand(false, cmdType, cmdText, cmdParms);
                return oCommand.ExecuteReader(CommandBehavior.CloseConnection);

            }
            catch (Exception ex)
            {
                CloseFactoryConnection();
                throw ex;
            }
            finally
            {
                if (null != oCommand)
                    oCommand.Dispose();
            }
        }

        #endregion

        #endregion

        #region ADAPTER METHODS 

        #region PARAMETERLESS METHODS 

        /// <summary>
        ///Description	    :	This function is used to fetch data using Data Adapter	
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	Data Set
        ///Comments			:	
        ///                     Has to be changed/removed if object based array concept is removed.
        /// </summary>
        public DataSet DataAdapter(CommandType cmdType, string cmdText)
        {

            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            DbDataAdapter dda = null;
            try
            {
                OpenFactoryConnection();
                dda = oFactory.CreateDataAdapter();
                PrepareCommand(false, cmdType, cmdText);

                dda.SelectCommand = oCommand;
                DataSet ds = new DataSet();
                dda.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (null != oCommand)
                    oCommand.Dispose();
                CloseFactoryConnection();
            }
        }

        #endregion

        #region OBJECT BASED PARAMETER ARRAY 

        /// <summary>
        ///Description	    :	This function is used to fetch data using Data Adapter	
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	Data Set
        ///Comments			:	
        /// </summary>
        public DataSet DataAdapter(CommandType cmdType, string cmdText, object[,] cmdParms)
        {

            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            DbDataAdapter dda = null;
            try
            {
                OpenFactoryConnection();
                dda = oFactory.CreateDataAdapter();
                PrepareCommand(false, cmdType, cmdText, cmdParms);

                dda.SelectCommand = oCommand;
                DataSet ds = new DataSet();
                dda.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (null != oCommand)
                    oCommand.Dispose();
                CloseFactoryConnection();
            }
        }

        #endregion

        #region STRUCTURE BASED PARAMETER ARRAY 

        /// <summary>
        ///Description	    :	This function is used to fetch data using Data Adapter	
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	Data Set
        ///Comments			:	
        /// </summary>
        public DataSet DataAdapter(CommandType cmdType, string cmdText, Parameters[] cmdParms)
        {

            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            DbDataAdapter dda = null;
            try
            {
                OpenFactoryConnection();
                dda = oFactory.CreateDataAdapter();
                PrepareCommand(false, cmdType, cmdText, cmdParms);

                dda.SelectCommand = oCommand;
                DataSet ds = new DataSet();
                dda.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (null != oCommand)
                    oCommand.Dispose();
                CloseFactoryConnection();
            }
        }

        #endregion

        #endregion

        #region SCALAR METHODS 

        #region PARAMETERLESS METHODS 

        /// <summary>
        ///Description	    :	This function is used to invoke Execute Scalar Method	
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	Object
        ///Comments			:	
        /// </summary>
        public object ExecuteScalar(CommandType cmdType, string cmdText)
        {
            try
            {
                OpenFactoryConnection();

                PrepareCommand(false, cmdType, cmdText);

                object val = oCommand.ExecuteScalar();

                return val;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (null != oCommand)
                    oCommand.Dispose();
                CloseFactoryConnection();
            }
        }

        #endregion

        #region OBJECT BASED PARAMETER ARRAY

        /// <summary>
        ///Description	    :	This function is used to invoke Execute Scalar Method	
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	Object
        ///Comments			:	
        /// </summary>
        public object ExecuteScalar(CommandType cmdType, string cmdText, object[,] cmdParms, bool blDisposeCommand)
        {
            try
            {

                OpenFactoryConnection();
                PrepareCommand(false, cmdType, cmdText, cmdParms);
                return oCommand.ExecuteScalar();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (blDisposeCommand && null != oCommand)
                    oCommand.Dispose();
                CloseFactoryConnection();
            }
        }

        /// <summary>
        ///Description	    :	This function is used to invoke Execute Scalar Method	
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	Object
        ///Comments			:	Overloaded Method. 
        /// </summary>
        public object ExecuteScalar(CommandType cmdType, string cmdText, object[,] cmdParms)
        {
            return ExecuteScalar(cmdType, cmdText, cmdParms, true);
        }

        /// <summary>
        ///Description	    :	This function is used to invoke Execute Scalar Method	
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	Object
        ///Comments			:	
        /// </summary>
        public object ExecuteScalar(bool blTransaction, CommandType cmdType, string cmdText, object[,] cmdParms, bool blDisposeCommand)
        {
            try
            {

                PrepareCommand(blTransaction, cmdType, cmdText, cmdParms);
                return oCommand.ExecuteScalar();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (blDisposeCommand && null != oCommand)
                    oCommand.Dispose();
            }
        }

        /// <summary>
        ///Description	    :	This function is used to invoke Execute Scalar Method	
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	Object
        ///Comments			:	
        /// </summary>
        public object ExecuteScalar(bool blTransaction, CommandType cmdType, string cmdText, object[,] cmdParms)
        {
            return ExecuteScalar(blTransaction, cmdType, cmdText, cmdParms, true);
        }

        #endregion

        #region STRUCTURE BASED PARAMETER ARRAY 

        /// <summary>
        ///Description	    :	This function is used to invoke Execute Scalar Method	
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	Object
        ///Comments			:	
        /// </summary>
        public object ExecuteScalar(CommandType cmdType, string cmdText, Parameters[] cmdParms, bool blDisposeCommand)
        {
            try
            {
                OpenFactoryConnection();
                PrepareCommand(false, cmdType, cmdText, cmdParms);
                return oCommand.ExecuteScalar();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (blDisposeCommand && null != oCommand)
                    oCommand.Dispose();
                CloseFactoryConnection();
            }
        }

        /// <summary>
        ///Description	    :	This function is used to invoke Execute Scalar Method	
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	Object
        ///Comments			:	Overloaded Method. 
        /// </summary>
        public object ExecuteScalar(CommandType cmdType, string cmdText, Parameters[] cmdParms)
        {
            return ExecuteScalar(cmdType, cmdText, cmdParms, true);
        }

        /// <summary>
        ///Description	    :	This function is used to invoke Execute Scalar Method	
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	Object
        ///Comments			:	
        /// </summary>
        public object ExecuteScalar(bool blTransaction, CommandType cmdType, string cmdText, Parameters[] cmdParms, bool blDisposeCommand)
        {
            try
            {

                PrepareCommand(blTransaction, cmdType, cmdText, cmdParms);
                return oCommand.ExecuteScalar();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (blDisposeCommand && null != oCommand)
                    oCommand.Dispose();
            }
        }

        /// <summary>
        ///Description	    :	This function is used to invoke Execute Scalar Method	
        ///Author			:	Shyam SS
        ///Date				:	28 June "200"7
        ///Input			:	Command Type, Command Text, 2-Dimensional Parameter Array
        ///OutPut			:	Object
        ///Comments			:	
        /// </summary>
        public object ExecuteScalar(bool blTransaction, CommandType cmdType, string cmdText, Parameters[] cmdParms)
        {
            return ExecuteScalar(blTransaction, cmdType, cmdText, cmdParms, true);
        }

        #endregion

        #endregion

    }
}
