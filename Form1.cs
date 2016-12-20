using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace NetflixApp
{
  public partial class Form1 : Form
  {
    //
    // Class members:
    //
    private string m_connectionInfo;

    //
    // Constructor:
    //
    public Form1()
    {
      InitializeComponent();

      string filename, version;

      version = "v11.0";    // for VS 2013:
      //version = "MSSQLLocalDB";  // for VS 2015:
      filename = "netflix.mdf";

      m_connectionInfo = String.Format(@"Data Source=(LocalDB)\{0};AttachDbFilename=|DataDirectory|\{1};Integrated Security=True;", 
        version, 
        filename);
    }


    //
    // Form1_Load:  called just before the form is displayed to the user:
    //
    private void Form1_Load(object sender, EventArgs e)
    {
    }


    private void tbarRating_Scroll(object sender, EventArgs e)
    {
      lblRating.Text = tbarRating.Value.ToString();
    }

    //
    // Add Review:
    //
    private void cmdInsertReview_Click(object sender, EventArgs e)
    {
      //
      // Get the movie name from the list of movies:
      //
      if (this.listBox1.SelectedIndex < 0)
      {
        MessageBox.Show("Please select a movie...");
        return;
      }

      string MovieName = this.listBox1.Text;

      //
      // And the user name from the list of users:
      //
      if (this.listBox2.SelectedIndex < 0)
      {
        MessageBox.Show("Please select a user...");
        return;
      }

      string UserName = this.listBox2.Text;

      //
      // NOTE: since a movie and a user is selected, the movie and user IDs are 
      // available from the associated text boxes:
      //
      int movieid = Convert.ToInt32(this.txtMovieID.Text);
      int userid = Convert.ToInt32(this.txtUserID.Text);
      int rating = Convert.ToInt32(lblRating.Text); //grab the rating

      //
      // Insert movie review:
      //

      BusinessTier.Business BT = new BusinessTier.Business("netflix.mdf");
   
      var review = BT.AddReview(movieid, userid, rating);


      if (review != null) // success!
      {
          SubForm frm = new SubForm();

          frm.lblHeader.Text = string.Format("Great Success!");

          string msg = string.Format("ReviewID added: {0}", review.ReviewID);

          frm.listBox1.Items.Add(msg);
          frm.ShowDialog();
      }
    }


    //
    // All Movies:
    //
    private void cmdAllMovies_Click(object sender, EventArgs e)
    {
        listBox1.Items.Clear();

        SqlConnection db;
        db = new SqlConnection(m_connectionInfo);
        db.Open();

        SqlCommand cmd = new SqlCommand();
        cmd.Connection = db;

        //
        // Select all movies, sorted by name ASC:
        //
        cmd.CommandText = string.Format("SELECT MovieName FROM Movies ORDER BY MovieName ASC;");

        // MessageBox.Show(cmd.CommandText);

        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
        DataSet ds = new DataSet();

        adapter.Fill(ds);  // execute!

        db.Close();

        //
        // display results:
        //
        DataTable dt = ds.Tables["TABLE"];

        if (dt.Rows.Count == 0)
        {
            listBox1.Items.Add("**Error, or database is empty?!");
        }
        else
        {
            //
            // we have ratings data, display:
            //
            foreach (DataRow row in dt.Rows)
                listBox1.Items.Add(row["MovieName"].ToString());
        }
    }


    //
    // When the user selects a movie, display movie id and average rating...
    //
    private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      string name;

      name = this.listBox1.Text;  // selected movie:

      BusinessTier.Business BT = new BusinessTier.Business("netflix.mdf");
      var movie = BT.GetMovie(name);

      if (movie == null)
      {
          MessageBox.Show("**Internal Error: no movie?!");
          return;
      }

      var detail = BT.GetMovieDetail(movie.MovieID); 
      if (detail == null) 
      { 
            MessageBox.Show("**Internal Error: no details?!"); 
            return; 
      }

      this.txtMovieID.Text = movie.MovieID.ToString();
      this.txtAvgRating.Text = detail.AvgRating.ToString();
    }


    //
    // Reviews for selected movie:
    //
    private void cmdMovieReviews_Click(object sender, EventArgs e)
    {
      string name;

      if (this.listBox1.SelectedIndex < 0)
      {
        MessageBox.Show("Please select a movie...");
        return;
      }

      name = this.listBox1.Text; //name of selected movie

      //
      // NOTE: since a movie is selected, the movie id is in the associated textbox:
      //

      int movieid = Convert.ToInt32(this.txtMovieID.Text); //movie id of selected movie

        /*
              listBox1.Items.Clear();
              BusinessTier.Business BT = new BusinessTier.Business("netflix.mdf");
              var movies = BT.GetAllMovies();

              foreach (BusinessTier.Movie m in movies)
              {
                  this.listBox1.Items.Add(m.MovieName);
              } 
        */

        //
      // Get all the reviews for this movie:
      //

      BusinessTier.Business BT = new BusinessTier.Business("netflix.mdf");
      var ratings = BT.GetMovieDetail(movieid);

        /*
      SubForm frm = new SubForm();

      frm.lblHeader.Text = string.Format("Reviews for \"{0}\"", name);

      frm.listBox1.Items.Add(name);
      frm.listBox1.Items.Add("");

      DataTable dt = ds.Tables["TABLE"];

      if (dt.Rows.Count == 0)
      {
          frm.listBox1.Items.Add("No reviews...");
      }
      else
      {
          foreach (DataRow row in dt.Rows)
          {
              string msg = string.Format("{0}: {1}",
                row["UserID"], row["Rating"]);

              frm.listBox1.Items.Add(msg);
          }
      }

      frm.ShowDialog();

        */

      SubForm frm = new SubForm();

      frm.lblHeader.Text = string.Format("Reviews for \"{0}\"", name);

      frm.listBox1.Items.Add(name);
      frm.listBox1.Items.Add("");

      if (ratings != null)
      {
          foreach (BusinessTier.Review m in ratings.Reviews)
          {
              //this.listBox1.Items.Add(m.Rating);
              frm.listBox1.Items.Add(m.ReviewID + ": " + m.Rating);
          }
      }
      else
      {
          frm.listBox1.Items.Add("No reviews...");
      }

      frm.ShowDialog();

      /*
      SqlConnection db;
      db = new SqlConnection(m_connectionInfo);
      db.Open();

      SqlCommand cmd = new SqlCommand();
      cmd.Connection = db; 
      
      SqlDataAdapter adapter = new SqlDataAdapter(cmd);
      DataSet ds = new DataSet();

      cmd.CommandText = string.Format(@"SELECT UserID, Rating 
            FROM Reviews 
            WHERE MovieID={0}
            ORDER BY Rating Desc, UserID ASC;",
        movieid);

      // MessageBox.Show(cmd.CommandText);

      adapter.Fill(ds);

      db.Close();  

      // 
      // Display the results in a subform:
      //
      SubForm frm = new SubForm();

      frm.lblHeader.Text = string.Format("Reviews for \"{0}\"", name);

      frm.listBox1.Items.Add(name);
      frm.listBox1.Items.Add("");

      DataTable dt = ds.Tables["TABLE"];

      if (dt.Rows.Count == 0)
      {
        frm.listBox1.Items.Add("No reviews...");
      }
      else
      {
        foreach (DataRow row in dt.Rows)
        {
          string msg = string.Format("{0}: {1}",
            row["UserID"], row["Rating"]);

          frm.listBox1.Items.Add(msg);
        }
      }

      frm.ShowDialog();

      */
    }
    

    //
    // Summary of reviews (by each rating) for selected movie:
    //
    private void cmdReviewsSummary_Click(object sender, EventArgs e)
    {
      string name;

      if (this.listBox1.SelectedIndex < 0)
      {
        MessageBox.Show("Please select a movie...");
        return;
      }

      name = this.listBox1.Text;

      //
      // NOTE: since a movie is selected, the movie id is in the associated textbox:
      //

      int movieid = Convert.ToInt32(this.txtMovieID.Text);

      BusinessTier.Business BT = new BusinessTier.Business("netflix.mdf");
      var ratings = BT.GetMovieDetail(movieid);

        int one = ratings.Reviews.Count(p => p.Rating == 1);
        int two = ratings.Reviews.Count(p => p.Rating == 2);
        int thr = ratings.Reviews.Count(p => p.Rating == 3);
        int fur = ratings.Reviews.Count(p => p.Rating == 4);
        int fiv = ratings.Reviews.Count(p => p.Rating == 5);

        SubForm frm = new SubForm();

        frm.lblHeader.Text = string.Format("Reviews count for \"{0}\"", name);

        frm.listBox1.Items.Add(name);
        frm.listBox1.Items.Add("");

        if (ratings != null)
        {
            frm.listBox1.Items.Add("5 : " + fiv);
            frm.listBox1.Items.Add("4 : " + fur);
            frm.listBox1.Items.Add("3 : " + thr);
            frm.listBox1.Items.Add("2 : " + two);
            frm.listBox1.Items.Add("1 : " + one);

        }
        else
        {
            frm.listBox1.Items.Add("No reviews...");
        }

        frm.ShowDialog();


      //
      // Let's get all the reviews, grouped by rating and count each group:
      //

        /*
      SqlConnection db;
      db = new SqlConnection(m_connectionInfo);
      db.Open();

      SqlCommand cmd = new SqlCommand();
      cmd.Connection = db;

      //
      // Group all the ratings for given movie, and then count each group:
      //
      cmd.CommandText = string.Format(@"SELECT Rating, COUNT(Rating) as RatingCount
          FROM Reviews
          WHERE Reviews.MovieID={0}
          GROUP BY Rating
          ORDER BY Rating DESC;",
        movieid);

      // MessageBox.Show(cmd.CommandText);

      SqlDataAdapter adapter = new SqlDataAdapter(cmd);
      DataSet ds = new DataSet();

      adapter.Fill(ds);  // execute!

      db.Close();

      //
      // display results:
      //
      SubForm frm = new SubForm();

      frm.lblHeader.Text = string.Format("Review Summary for \"{0}\"", name);

      frm.listBox1.Items.Add(name);
      frm.listBox1.Items.Add("");

      DataTable dt = ds.Tables["TABLE"];

      if (dt.Rows.Count == 0)
      {
        frm.listBox1.Items.Add("No reviews...");
      }
      else
      {
        int total = 0;

        //
        // we have ratings data, display:
        //
        foreach (DataRow row in dt.Rows)
        {
          string msg = string.Format("{0}: {1}",
            row["Rating"], row["RatingCount"]);

          frm.listBox1.Items.Add(msg);

          total = total + Convert.ToInt32(row["RatingCount"]);
        }

        frm.listBox1.Items.Add("");
        frm.listBox1.Items.Add("Total: " + total.ToString());
      }

      frm.ShowDialog();
       */
    }


    //
    // All Users:
    //
    private void cmdAllUsers_Click(object sender, EventArgs e)
    {
      listBox2.Items.Clear();
        
      BusinessTier.Business BT = new BusinessTier.Business("netflix.mdf");
      var movies = BT.GetAllNamedUsers();


      if (movies == null)
      {
          this.listBox2.Items.Add("No users found!");
      }
      else
      {
          foreach (BusinessTier.User m in movies)
          {
              this.listBox2.Items.Add(m.UserName);
          }
      }
    
        /*
      SqlConnection db;
      db = new SqlConnection(m_connectionInfo);
      db.Open();

      SqlCommand cmd = new SqlCommand();
      cmd.Connection = db;

      //
      // Select all users, sorted by name ASC:
      //
      cmd.CommandText = string.Format("SELECT UserName FROM Users ORDER BY UserName ASC;");

      // MessageBox.Show(cmd.CommandText);

      SqlDataAdapter adapter = new SqlDataAdapter(cmd);
      DataSet ds = new DataSet();

      adapter.Fill(ds);  // execute!

      db.Close();

      //
      // display results:
      //
      DataTable dt = ds.Tables["TABLE"];

      if (dt.Rows.Count == 0)
      {
        MessageBox.Show("**Error: no users, is database empty?!");
      }
      else
      {
        //
        // we have ratings data, display:
        //
        foreach (DataRow row in dt.Rows)
          listBox2.Items.Add(row["UserName"].ToString());
      }
        */
    }


    //
    // User has selected a user in the list:
    //
    private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
    {


        string name; //name of selected user

        name = this.listBox2.Text;  


        BusinessTier.Business BT = new BusinessTier.Business("netflix.mdf");
        var user = BT.GetNamedUser(name);

        if (user == null)
        {
            MessageBox.Show("**Internal Error: no movie?!");
            return;
        }
        //meesge
        else
        {
            this.txtUserID.Text = user.UserID.ToString();
            this.txtOccupation.Text = user.Occupation.ToString();
            //this.txtUserID.Text = row["UserID"].ToString();
            //this.txtOccupation.Text = row["Occupation"].ToString();
        }

        


        /*
      string name;

      name = this.listBox2.Text;  // selected user:

      //
      // open connection to DB:
      //
      SqlConnection db;
      db = new SqlConnection(m_connectionInfo);
      db.Open();

      SqlCommand cmd = new SqlCommand();
      cmd.Connection = db;

      //
      // Escape the user name in case it contains a ':
      //
      name = name.Replace("'", "''");  // escape any single ' in the string:

      //
      // Get the user ID:
      //
      cmd.CommandText = string.Format(@"SELECT Users.UserID, Users.Occupation
          FROM Users
          WHERE Users.UserName='{0}';",
        name);

      // MessageBox.Show(cmd.CommandText);

      SqlDataAdapter adapter = new SqlDataAdapter(cmd);
      DataSet ds = new DataSet();

      adapter.Fill(ds);  // execute!

      db.Close();

      //
      // display results:
      //
      DataTable dt = ds.Tables["TABLE"];

      if (dt.Rows.Count == 0)
      {
        MessageBox.Show("**Error: user not found?!");

        this.txtUserID.Text = "";
        this.txtOccupation.Text = "";
      }
      else
      {
        DataRow row = dt.Rows[0];

        this.txtUserID.Text = row["UserID"].ToString();
        this.txtOccupation.Text = row["Occupation"].ToString();
      }
        */
    }


    //
    // Reviews for selected user:
    //
    private void cmdUserReviews_Click(object sender, EventArgs e)
    {
      string name;

      if (this.listBox2.SelectedIndex < 0)
      {
        MessageBox.Show("Please select a user...");
        return;
      }

      name = this.listBox2.Text;

      //
      // NOTE: since a user is selected, the user id is in the associated textbox:
      //

      int userid = Convert.ToInt32(this.txtUserID.Text);

      BusinessTier.Business BT = new BusinessTier.Business("netflix.mdf");
      var selectedReviews = BT.GetUserDetail(userid);



      SubForm frm = new SubForm();

      frm.lblHeader.Text = string.Format("Reviews for \"{0}\"", name);

      frm.listBox1.Items.Add(name);
      frm.listBox1.Items.Add("");

      if (selectedReviews != null)
      {
          foreach (BusinessTier.Review m in selectedReviews.Reviews)
          {
              //this.listBox1.Items.Add(m.Rating);
              BusinessTier.Business BT2 = new BusinessTier.Business("netflix.mdf");
              var movieIn = BT2.GetMovieDetail(m.MovieID);

              frm.listBox1.Items.Add(movieIn.movie.MovieName + ": " + m.Rating);
          }
      }
      else
      {
          frm.listBox1.Items.Add("No reviews...");
      }

      frm.ShowDialog();




        /*
      //
      // Get all the reviews by this user:
      //
      SqlConnection db;
      db = new SqlConnection(m_connectionInfo);
      db.Open();

      SqlCommand cmd = new SqlCommand();
      cmd.Connection = db;

      SqlDataAdapter adapter = new SqlDataAdapter(cmd);
      DataSet ds = new DataSet();

      cmd.CommandText = string.Format(@"SELECT Movies.MovieName, Temp.Rating 
            FROM Movies
            INNER JOIN
            (
              SELECT MovieID, Rating 
              FROM   Reviews
              WHERE  UserID={0}
            ) AS Temp
            ON Temp.MovieID = Movies.MovieID
            ORDER BY Movies.MovieName ASC, Temp.Rating ASC;",
        userid);

      // MessageBox.Show(cmd.CommandText);

      adapter.Fill(ds);

      db.Close();

      // 
      // Display the results in a subform:
      //
      SubForm frm = new SubForm();

      frm.lblHeader.Text = string.Format("Reviews by \"{0}\"", name);

      frm.listBox1.Items.Add(name);
      frm.listBox1.Items.Add("");

      DataTable dt = ds.Tables["TABLE"];

      if (dt.Rows.Count == 0)
      {
        frm.listBox1.Items.Add("No reviews...");
      }
      else
      {
        foreach (DataRow row in dt.Rows)
        {
          string msg = string.Format("{0} -> {1}",
            row["MovieName"], row["Rating"]);

          frm.listBox1.Items.Add(msg);
        }
      }

      frm.ShowDialog();
         */
    }


    //
    // File >> Test Connection:
    //
    private void testConnectionToolStripMenuItem_Click(object sender, EventArgs e)
    {
        BusinessTier.Business BT = new BusinessTier.Business("netflix.mdf");
        var testConnection = BT.TestConnection();

        if (testConnection == false)
        {
            MessageBox.Show("**Error: database file not found?!");
        }   
    /*
      try
      {
          
        SqlConnection db;

        db = new SqlConnection(m_connectionInfo);
        db.Open();

        MessageBox.Show(db.State.ToString());

        db.Close();

        MessageBox.Show(db.State.ToString());
       
      }
      catch
      {
        MessageBox.Show("**Error: database file not found?!");
      }
     */
    }


    //
    // File >> Exit:
    //
    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      this.Close();
    }


    //
    // File >> Top Movies by Avg Rating:
    //
    private void topMoviesByRatingToolStripMenuItem_Click(object sender, EventArgs e)
    {
        string N = txtTopN.Text;
        int topN = System.Convert.ToInt32(N);

        BusinessTier.Business BT = new BusinessTier.Business("netflix.mdf");
        var TopMovies = BT.GetTopMoviesByAvgRating(topN);


        SubForm frm = new SubForm();

        frm.lblHeader.Text = string.Format("Top \"{0}\" average reviews", N);
        //frm.listBox1.Items.Add(name);
        frm.listBox1.Items.Add("");

        if (TopMovies == null)
        {
            frm.listBox1.Items.Add("No reviews...");
        }
        else
        {
            foreach (BusinessTier.Movie m in TopMovies)
            {
                int theMovie = m.MovieID;
                BusinessTier.Business BT2 = new BusinessTier.Business("netflix.mdf");
                var rating = BT.GetMovieDetail(theMovie).Reviews.Count;

                string msg = string.Format("{0} -> {1}", m.MovieName, rating);

                frm.listBox1.Items.Add(msg);
            }
        }

        frm.ShowDialog();

        /*
     SqlConnection db;
     db = new SqlConnection(m_connectionInfo);
     db.Open();

     SqlCommand cmd = new SqlCommand();
     cmd.Connection = db;

     //
     // Group all the reviews for each movie, compute averages, and take top N:
     //
     string N = txtTopN.Text;

     cmd.CommandText = string.Format(@"SELECT TOP {0} Movies.MovieName, g.AvgRating 
           FROM Movies
           INNER JOIN 
             (
               SELECT MovieID, ROUND(AVG(CAST(Rating AS Float)), 4) as AvgRating 
               FROM Reviews
               GROUP BY MovieID
             ) g
           ON g.MovieID = Movies.MovieID
           ORDER BY g.AvgRating DESC, Movies.MovieName Asc;",
       N);

     // MessageBox.Show(cmd.CommandText);
       
     SqlDataAdapter adapter = new SqlDataAdapter(cmd);
     DataSet ds = new DataSet();

     adapter.Fill(ds);  // execute!

     db.Close();

     //
     // display results:
     //
     DataTable dt = ds.Tables["TABLE"];

     if (dt.Rows.Count == 0)
     {
       MessageBox.Show("**Error: no movies, is database empty?!");
     }
     else
     {
       //
       // we have ratings data, display in our subform:
       //
       SubForm frm = new SubForm();

       frm.lblHeader.Text = "Top Movies by Average Rating";

       foreach (DataRow row in dt.Rows)
       {
         string msg = string.Format("{0}: {1}",
           row["MovieName"], row["AvgRating"]);

         frm.listBox1.Items.Add(msg);
       }

       frm.ShowDialog();
     }
       */
    }


    //
    // File >> Top Movies by Num Reviews:
    //
    private void topMoviesByNumReviewsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        string N = txtTopN.Text;
        int topN = System.Convert.ToInt32(N);

        BusinessTier.Business BT = new BusinessTier.Business("netflix.mdf");
        var TopMovies = BT.GetTopMoviesByNumReviews(topN);


        SubForm frm = new SubForm();

        frm.lblHeader.Text = string.Format("Top \"{0}\" num reviews", N);
        //frm.listBox1.Items.Add(name);
        frm.listBox1.Items.Add("");

        if (TopMovies == null)
        {
            frm.listBox1.Items.Add("No reviews...");
        }
        else
        {
            foreach (BusinessTier.Movie m in TopMovies)
            {
                int theMovie = m.MovieID;
                BusinessTier.Business BT2 = new BusinessTier.Business("netflix.mdf");
                var rating = BT.GetMovieDetail(theMovie).Reviews.Count;

                string msg = string.Format("{0} -> {1}", m.MovieName, rating);

                frm.listBox1.Items.Add(msg);
            }
        }

        frm.ShowDialog();


        /*
      SqlConnection db;
      db = new SqlConnection(m_connectionInfo);
      db.Open();

      SqlCommand cmd = new SqlCommand();
      cmd.Connection = db;

      //
      // Group all the reviews for each movie, compute averages, and take top N:
      //
      string N = txtTopN.Text;

      cmd.CommandText = string.Format(@"SELECT TOP {0} Movies.MovieName, g.RatingCount 
            FROM Movies
            INNER JOIN 
              (
                SELECT MovieID, COUNT(*) as RatingCount 
                FROM Reviews
                GROUP BY MovieID
              ) g
            ON g.MovieID = Movies.MovieID
            ORDER BY g.RatingCount DESC, Movies.MovieName Asc;",
        N);

      // MessageBox.Show(cmd.CommandText);

      SqlDataAdapter adapter = new SqlDataAdapter(cmd);
      DataSet ds = new DataSet();

      adapter.Fill(ds);  // execute!

      db.Close();

      //
      // display results:
      //
      DataTable dt = ds.Tables["TABLE"];

      if (dt.Rows.Count == 0)
      {
        MessageBox.Show("**Error: no movies, is database empty?!");
      }
      else
      {
        //
        // we have ratings data, display in our subform:
        //
        SubForm frm = new SubForm();

        frm.lblHeader.Text = "Top Movies by Number of Reviews";

        foreach (DataRow row in dt.Rows)
        {
          string msg = string.Format("{0}: {1}",
            row["MovieName"], row["RatingCount"]);

          frm.listBox1.Items.Add(msg);
        }

        frm.ShowDialog();
      }
         * */
    }


    //
    // File >> Top Users by New Reviews:
    //
    private void topUsersByNumReviewsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        
        string N = txtTopN.Text;
        int topN = System.Convert.ToInt32(N);

        BusinessTier.Business BT = new BusinessTier.Business("netflix.mdf");
        var TopMovies = BT.GetTopUsersByNumReviews(topN);


        SubForm frm = new SubForm();

        frm.lblHeader.Text = string.Format("Top \"{0}\" movies by num reviews", N);
        //frm.listBox1.Items.Add(name);
        frm.listBox1.Items.Add("");

        if (TopMovies == null)
        {
            frm.listBox1.Items.Add("No reviews...");
        }
        else
        {
            foreach (BusinessTier.User m in TopMovies)
            {
                int theMovie = m.UserID;
                BusinessTier.Business BT2 = new BusinessTier.Business("netflix.mdf");
                var count = BT2.GetUserDetail(theMovie).NumReviews;
     
                string msg = string.Format("{0} -> {1}", m.UserName, count);

                frm.listBox1.Items.Add(msg);
            }
        }

        frm.ShowDialog();
        

       /*
      SqlConnection db;
      db = new SqlConnection(m_connectionInfo);
      db.Open();

      SqlCommand cmd = new SqlCommand();
      cmd.Connection = db;

      //
      // Group all the reivews by user, count, and take top N:
      //
      string N = txtTopN.Text;

      //
      // NOTE: some reviews are anonymous, i.e. we don't have a username.  So we
      // use a "RIGHT JOIN" to capture those as well.
      //
      cmd.CommandText = string.Format(@"SELECT TOP {0} Users.UserName, Temp.RatingCount
            FROM Users
            RIGHT JOIN
            (
              SELECT UserID, COUNT(*) AS RatingCount
              FROM Reviews
              GROUP BY UserID
            ) AS Temp
            On Temp.UserID = Users.UserID
            ORDER BY Temp.RatingCount DESC, Users.UserName Asc;",
        N);

      // MessageBox.Show(cmd.CommandText);

      SqlDataAdapter adapter = new SqlDataAdapter(cmd);
      DataSet ds = new DataSet();

      adapter.Fill(ds);  // execute!

      db.Close();

      //
      // display results:
      //
      DataTable dt = ds.Tables["TABLE"];

      if (dt.Rows.Count == 0)
      {
        MessageBox.Show("**Error: no movies, is database empty?!");
      }
      else
      {
        //
        // we have ratings data, display in our subform:
        //
        SubForm frm = new SubForm();

        frm.lblHeader.Text = "Top Users by Number of Reviews";

        foreach (DataRow row in dt.Rows)
        {
          string username = row["UserName"].ToString();

          if (username == "")
            username = " <anonymous>";

          string msg = string.Format("{0}: {1}",
            username, row["RatingCount"]);

          frm.listBox1.Items.Add(msg);
        }

        frm.ShowDialog();
      }
      */
    }

    private void txtTopN_TextChanged(object sender, EventArgs e)
    {

    }

    private void button1_Click(object sender, EventArgs e)
    {
        //string movieIDin = textBox1.Text;
        int movieID = System.Convert.ToInt32(textBox1.Text);

        BusinessTier.Business BT = new BusinessTier.Business("netflix.mdf");
        var MovieName = BT.GetMovie(movieID);


        SubForm frm = new SubForm();

        frm.lblHeader.Text = string.Format("MovieName");


        if (MovieName == null)
        {
            frm.listBox1.Items.Add("MovieID does not exist");
        }
        else
        {

            frm.listBox1.Items.Add(MovieName.MovieName);
        }

        frm.ShowDialog();
    }

    private void button2_Click(object sender, EventArgs e)
    {

        int userID = System.Convert.ToInt32(textBox2.Text);

        BusinessTier.Business BT = new BusinessTier.Business("netflix.mdf");
        var userName = BT.GetUser(userID);


        SubForm frm = new SubForm();

        frm.lblHeader.Text = string.Format("UserName");


        if (userName == null)
        {
            frm.listBox1.Items.Add("UserID does not exist");
        }
        else
        {

            frm.listBox1.Items.Add(userName.UserName);
        }

        frm.ShowDialog();

    }

  }//class
}//namespace
