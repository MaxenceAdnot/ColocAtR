using System.ServiceModel;

namespace WSColocAtR
{
    [ServiceContract]
    public interface IColocAtR
    {

        #region Auth

        //
        // Permet de créer un compte avec pour parametre un nom d'utilisateur, un mot de passe, un prénom et un nom
        [OperationContract]
        WSAuthMessage CreateAccount(string login, string email, string password, string firstName, string lastName);

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

        //
        // Retourne le nom de l'utilisateur
        [OperationContract]
        WSAuthMessage Whoami(string token);

        #endregion

        #region ProfilManager


        //
        // Permet de mettre à jour le profil complet
        [OperationContract]
        WSAuthMessage ProfilWizard(string token, bool type, int age, int price, string city, string desc, int m2 = 0);

        //
        // Permet de changer le mot de passe de l'utilisateur connecté
        [OperationContract]
        WSAuthMessage ChangePassword(string token, string oldPassword, string newPassword);


        //
        // Retourne les annonces correspondantes aux critères de l'utilisateur
        [OperationContract]
        WSAuthMessage GetScoringResults(string token);

        #endregion

    }
}
