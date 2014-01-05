using System.ServiceModel;

namespace WSColocAtR
{
    [ServiceContract]
    public interface IColocAtR
    {

        //
        // Permet à l'utilisateur de creer une session et de se connecter grâce à son email / mot de passe.
        [OperationContract]
        WSAuthMessage SignIn(string email, string password);

        //
        // Permet à l'utilisateur de se déconnecter proprement en supprimant sa session active
        [OperationContract]
        WSAuthMessage SignOut(string token);

        //
        // Met à jour la date de derniere activité pour éviter un timeout
        [OperationContract]
        WSAuthMessage RefreshLastActivity(string token);

        // Retourne le nom de l'utilisateur
        [OperationContract]
        WSAuthMessage Whoami(string token);

        [OperationContract]
        WSAuthMessage Kevbac(string token);
    }
}
