//
// BusinessTier:  business logic, acting as interface between UI and data store.
//

using System;
using System.Collections.Generic;
using System.Data;


namespace BusinessTier
{

  //
  // Business:
  //
  public class Business
  {
    //
    // Fields:
    //
    private string _DBFile;
    private DataAccessTier.Data dataTier;


    //
    // Constructor:
    //
    public Business(string DatabaseFilename)
    {
      _DBFile = DatabaseFilename;

      dataTier = new DataAccessTier.Data(DatabaseFilename);
    }


    //
    // TestConnection:
    //
    // Returns true if we can establish a connection to the database, false if not.
    //
    public bool TestConnection()
    {
      return dataTier.TestConnection();
    }


    //
    // GetUser:
    //
    // Retrieves User object based on USER ID; returns null if user is not
    // found.
    //
    // NOTE: if the user exists in the Users table, then a name and occupation
    // is returned.  If the user does not exist in the Users table, then the user
    // id is looked up in the Reviews table.  If found, then the user is an
    // "anonymous" user, so a User object with name = "<UserID>" and no occupation
    // ("") is returned.  In other words, name = the user’s id surrounded by < >.
    //
    public User GetUser(int UserID)
    {
        DataTable dataTable = this.dataTier.ExecuteNonScalarQuery(string.Format("SELECT * FROM Users WHERE UserID={0};", (object)UserID)).Tables["TABLE"];
        if (dataTable.Rows.Count > 0)
        {
            DataRow dataRow = dataTable.Rows[0];
            return new User(UserID, dataRow["UserName"].ToString(), dataRow["Occupation"].ToString());
        }
        if (Convert.ToInt32(this.dataTier.ExecuteScalarQuery(string.Format("SELECT Count(ReviewID) FROM Reviews WHERE UserID={0};", (object)UserID))) <= 0)
            return (User)null;
        string userName = string.Format("<{0}>", (object)UserID);
        return new User(UserID, userName, "");
    }

    public User GetNamedUser(string UserName)
    {
        DataTable dataTable = this.dataTier.ExecuteNonScalarQuery(string.Format("SELECT * FROM Users WHERE UserName='{0}';", (object)UserName.Replace("'", "''"))).Tables["TABLE"];
        if (dataTable.Rows.Count <= 0)
            return (User)null;
        DataRow dataRow = dataTable.Rows[0];
        return new User(Convert.ToInt32(dataRow["UserID"]), UserName, dataRow["Occupation"].ToString());
    }

    public IReadOnlyList<User> GetAllNamedUsers()
    {
        List<User> list = new List<User>();
        foreach (DataRow dataRow in (InternalDataCollectionBase)this.dataTier.ExecuteNonScalarQuery(string.Format("SELECT * \r\n            FROM Users \r\n            ORDER BY UserName ASC;")).Tables["TABLE"].Rows)
        {
            User user = new User(Convert.ToInt32(dataRow["UserID"]), dataRow["UserName"].ToString(), dataRow["Occupation"].ToString());
            list.Add(user);
        }
        return (IReadOnlyList<User>)list;
    }

    public Movie GetMovie(int MovieID)
    {
        object obj = this.dataTier.ExecuteScalarQuery(string.Format("SELECT MovieName FROM Movies WHERE MovieID={0};", (object)MovieID));
        if (obj == null || obj == DBNull.Value)
            return (Movie)null;
        return new Movie(MovieID, obj.ToString());
    }

    public Movie GetMovie(string MovieName)
    {
        object obj = this.dataTier.ExecuteScalarQuery(string.Format("SELECT MovieID FROM Movies WHERE MovieName='{0}';", (object)MovieName.Replace("'", "''")));
        if (obj == null || obj == DBNull.Value)
            return (Movie)null;
        return new Movie(Convert.ToInt32(obj), MovieName);
    }

    public Review AddReview(int MovieID, int UserID, int Rating)
    {
        object obj = this.dataTier.ExecuteScalarQuery(string.Format("INSERT INTO Reviews(MovieID, UserID, Rating) Values({0}, {1}, {2});;\r\n          SELECT ReviewID FROM Reviews WHERE ReviewID = SCOPE_IDENTITY();", (object)MovieID, (object)UserID, (object)Rating));
        if (obj == null)
            return (Review)null;
        return new Review(Convert.ToInt32(obj), MovieID, UserID, Rating);
    }

    public MovieDetail GetMovieDetail(int MovieID)
    {
        Movie movie = this.GetMovie(MovieID);
        if (movie == null)
            return (MovieDetail)null;
        int numReviews = Convert.ToInt32(this.dataTier.ExecuteScalarQuery(string.Format("SELECT COUNT(*) FROM Reviews WHERE MovieID={0};", (object)MovieID)));
        double avgRating = 0.0;
        if (numReviews > 0)
            avgRating = Convert.ToDouble(this.dataTier.ExecuteScalarQuery(string.Format("SELECT ROUND(AVG(CAST(Rating AS Float)), 4) FROM Reviews WHERE MovieID={0};;", (object)MovieID)));
        List<Review> list = new List<Review>();
        if (numReviews > 0)
        {
            foreach (DataRow dataRow in (InternalDataCollectionBase)this.dataTier.ExecuteNonScalarQuery(string.Format("SELECT ReviewID, UserID, Rating \r\n            FROM Reviews \r\n            WHERE MovieID={0}\r\n            ORDER BY Rating Desc, UserID ASC;", (object)MovieID)).Tables["TABLE"].Rows)
            {
                Review review = new Review(Convert.ToInt32(dataRow["ReviewID"]), MovieID, Convert.ToInt32(dataRow["UserID"]), Convert.ToInt32(dataRow["Rating"]));
                list.Add(review);
            }
        }
        return new MovieDetail(movie, avgRating, numReviews, (IReadOnlyList<Review>)list);
    }

    public UserDetail GetUserDetail(int UserID)
    {
        User user = this.GetUser(UserID);
        if (user == null)
            return (UserDetail)null;
        int numReviews = Convert.ToInt32(this.dataTier.ExecuteScalarQuery(string.Format("SELECT COUNT(*) FROM Reviews WHERE UserID={0};", (object)UserID)));
        double avgRating = 0.0;
        if (numReviews > 0)
            avgRating = Convert.ToDouble(this.dataTier.ExecuteScalarQuery(string.Format("SELECT ROUND(AVG(CAST(Rating AS Float)), 4) FROM Reviews WHERE UserID={0};", (object)UserID)));
        List<Review> list = new List<Review>();
        if (numReviews > 0)
        {
            foreach (DataRow dataRow in (InternalDataCollectionBase)this.dataTier.ExecuteNonScalarQuery(string.Format("SELECT Temp.ReviewID, Temp.MovieID, Temp.Rating \r\n            FROM Movies\r\n            INNER JOIN\r\n            (\r\n              SELECT ReviewID, MovieID, Rating \r\n              FROM   Reviews\r\n              WHERE  UserID={0}\r\n            ) AS Temp\r\n            ON Temp.MovieID = Movies.MovieID\r\n            ORDER BY Movies.MovieName ASC, Temp.Rating ASC;", (object)UserID)).Tables["TABLE"].Rows)
            {
                Review review = new Review(Convert.ToInt32(dataRow["ReviewID"]), Convert.ToInt32(dataRow["MovieID"]), UserID, Convert.ToInt32(dataRow["Rating"]));
                list.Add(review);
            }
        }
        return new UserDetail(user, avgRating, numReviews, (IReadOnlyList<Review>)list);
    }

    public IReadOnlyList<Movie> GetTopMoviesByAvgRating(int N)
    {
        List<Movie> list = new List<Movie>();
        foreach (DataRow dataRow in (InternalDataCollectionBase)this.dataTier.ExecuteNonScalarQuery(string.Format("SELECT TOP {0} Movies.MovieID, Movies.MovieName\r\n            FROM Movies\r\n            INNER JOIN \r\n              (\r\n                SELECT MovieID, ROUND(AVG(CAST(Rating AS Float)), 4) as AvgRating \r\n                FROM Reviews\r\n                GROUP BY MovieID\r\n              ) TEMP\r\n            ON TEMP.MovieID = Movies.MovieID\r\n            ORDER BY TEMP.AvgRating DESC, Movies.MovieName Asc;", (object)N)).Tables["TABLE"].Rows)
        {
            Movie movie = new Movie(Convert.ToInt32(dataRow["MovieID"]), dataRow["MovieName"].ToString());
            list.Add(movie);
        }
        return (IReadOnlyList<Movie>)list;
    }

    public IReadOnlyList<Movie> GetTopMoviesByNumReviews(int N)
    {
        List<Movie> list = new List<Movie>();
        foreach (DataRow dataRow in (InternalDataCollectionBase)this.dataTier.ExecuteNonScalarQuery(string.Format("SELECT TOP {0} Movies.MovieID, Movies.MovieName\r\n            FROM Movies\r\n            INNER JOIN \r\n              (\r\n                SELECT MovieID, COUNT(*) as RatingCount \r\n                FROM Reviews\r\n                GROUP BY MovieID\r\n              ) TEMP\r\n            ON TEMP.MovieID = Movies.MovieID\r\n            ORDER BY TEMP.RatingCount DESC, Movies.MovieName Asc;", (object)N)).Tables["TABLE"].Rows)
        {
            Movie movie = new Movie(Convert.ToInt32(dataRow["MovieID"]), dataRow["MovieName"].ToString());
            list.Add(movie);
        }
        return (IReadOnlyList<Movie>)list;
    }

    public IReadOnlyList<User> GetTopUsersByNumReviews(int N)
    {
        List<User> list = new List<User>();
        foreach (DataRow dataRow in (InternalDataCollectionBase)this.dataTier.ExecuteNonScalarQuery(string.Format("SELECT TOP {0} Temp.UserID, Users.UserName, Users.Occupation\r\n            FROM Users\r\n            RIGHT JOIN\r\n            (\r\n              SELECT UserID, COUNT(*) AS RatingCount\r\n              FROM Reviews\r\n              GROUP BY UserID\r\n            ) AS Temp\r\n            On Temp.UserID = Users.UserID\r\n            ORDER BY Temp.RatingCount DESC, Users.UserName Asc;", (object)N)).Tables["TABLE"].Rows)
        {
            int userId = Convert.ToInt32(dataRow["UserID"]);
            string userName;
            string occupation;
            if (dataRow["UserName"].ToString() == "")
            {
                userName = string.Format("<{0}>", (object)userId);
                occupation = "";
            }
            else
            {
                userName = dataRow["UserName"].ToString();
                occupation = dataRow["Occupation"].ToString();
            }
            User user = new User(userId, userName, occupation);
            list.Add(user);
        }
        return (IReadOnlyList<User>)list;
    }

  }//class
}//namespace
