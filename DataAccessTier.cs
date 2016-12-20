//
// Data Access Tier:  interface between business tier and data store.
//

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace DataAccessTier
{

  public class Data
  {
    //
    // Fields:
    //
    private string _DBFile;
    private string _DBConnectionInfo;

    //
    // constructor:
    //
    public Data(string DatabaseFilename)
    {
      string version;

      version = "v11.0";    // for VS 2013:
      //version = "MSSQLLocalDB";  // for VS 2015:

      _DBFile = DatabaseFilename;
      _DBConnectionInfo = String.Format(@"Data Source=(LocalDB)\{0};AttachDbFilename=|DataDirectory|\{1};Integrated Security=True;",
        version,
        DatabaseFilename);
    }

    //
    // TestConnection:  returns true if the database can be successfully opened and closed,
    // false if not.
    //
    public bool TestConnection()
    {
      SqlConnection db = new SqlConnection(_DBConnectionInfo);

      bool  state = false;

      try
      {
        db.Open();

        state = (db.State == ConnectionState.Open);
      }
      catch
      {
        // nothing, just discard:
      }
      finally
      {
        db.Close();
      }

      return state;
    }

    //
    // ExecuteScalarQuery:  executes a scalar Select query, returning the single result 
    // as an object.  
    //
    public object ExecuteScalarQuery(string sql)
    {
        string filename, version, connectionInfo;
        SqlConnection db;

        version = "v11.0";
        filename = "netflix.mdf";

        connectionInfo = String.Format(@"Data Source=(LocalDB)\{0};AttachDbFilename=|DataDirectory|\{1};Integrated Security=True;", version, filename);
        db = new SqlConnection(connectionInfo);
        db.Open();

        //sql = string.Format("SELECT Rating FROM Reviews where movieID = {0};", movieIDint);
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = db;
        cmd.CommandText = sql;
        object result = cmd.ExecuteScalar();
        db.Close();
      
      return null;
    }

    // 
    // ExecuteNonScalarQuery:  executes a Select query that generates a temporary table,
    // returning this table in the form of a Dataset.
    //
    public DataSet ExecuteNonScalarQuery(string sql)
    {
        string filename, version, connectionInfo;
        SqlConnection db;

        version = "v11.0";
        filename = "netflix.mdf";

        connectionInfo = String.Format(@"Data Source=(LocalDB)\{0};AttachDbFilename=|DataDirectory|\{1};Integrated Security=True;", version, filename);
        db = new SqlConnection(connectionInfo);
        db.Open();

        //string sql = string.Format("INSERT INTO Reviews(MovieID, UserID, Rating) Values({0},{1},{2});", movieIDint, userIDint, ratingInint);
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = db;
        cmd.CommandText = sql;
        int rowsModified = cmd.ExecuteNonQuery();
        db.Close();
      return null;
    }

    //
    // ExecutionActionQuery:  executes an Insert, Update or Delete query, and returns
    // the number of records modified.
    //
    public int ExecuteActionQuery(string sql)
    {
        string filename, version, connectionInfo;
        SqlConnection db;

        version = "v11.0";
        filename = "netflix.mdf";

        connectionInfo = String.Format(@"Data Source=(LocalDB)\{0};AttachDbFilename=|DataDirectory|\{1};Integrated Security=True;", version, filename);
        db = new SqlConnection(connectionInfo);
        db.Open();

        //sql = string.Format("SELECT Rating FROM Reviews where movieID = {0};", movieIDint);
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = db;
        cmd.CommandText = sql;
        object result = cmd.ExecuteScalar();
        db.Close();
      
      return -1;
    }

  }//class
}//namespace
