namespace Itminus.InDirectLine.Services{


    public class InDirectLineOptions{
        public string ServiceUrl{get;set;} 
        public int TokenExpiresIn {get;set;} = 36000;
        public int StreamUrlMustBeConnectedIn{get;set;} =60;
    }
}