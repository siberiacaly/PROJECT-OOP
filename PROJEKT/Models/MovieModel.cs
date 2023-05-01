using Microsoft.Data.SqlClient;

namespace FINALNIPROJEKTBOOP.Models
{
    public class Movie {
        public int Id {get; set;}
        public string NameOfMovie {get; set;}
        public string Genre {get; set;}
        public int ReleaseDate {get; set;}
    }

    public class IndexView {

        public string NameOfMovie { get; set; }
        public string Genre { get; set; }
        public int ReleaseDate { get; set; }
        public string Actors { get; set; }
        public string Directors { get; set; }

    }

    public class Actor
    {
        public string FirstName { get; set; }

        public string Surname { get; set; }

        public string NameOfMovie { get; set; }

        public int Id { get; set; }

    }

    public class Director
    {
        public string FirstName { get; set; }

        public string Surname { get; set; }

        public string NameOfMovie { get; set;}
        public int Id { get; set; }
    }
}
