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