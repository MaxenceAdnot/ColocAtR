using System.Web;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;


namespace WSColocAtR
{

    [DataContract]
    public class WSProfile
    {
        [DataMember]
        public string username;
        [DataMember]
        public string firstName;
        [DataMember]
        public string lastName;
        [DataMember]
        public bool type;
        [DataMember]
        public int age;
        [DataMember]
        public int price;
        [DataMember]
        public string city;
        [DataMember]
        public string desc;
        [DataMember]
        public int m2;
        [DataMember]
        public string email;

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            WSProfile p = obj as WSProfile;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (username == p.username);
        }
    }


    [DataContract] 
    public enum StatusCode
    {
        [EnumMember]
        Error,         // Generic error
        [EnumMember]
        AccessRefused, // Access refused
        [EnumMember]
        OK,            // OK signal, operation done

    };

    [DataContract] 
    public class WSAuthMessage
    {

        private StatusCode code { get; set; }
        private string data { get; set; }

        [DataMember]
        public StatusCode StatusCode
        {

            get { return code; }
            set { code = value; }
        }

        [DataMember]
        public string Data
        {

            get { return data; }
            set { data = value; }
        }
    }
}