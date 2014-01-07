using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace WSColocAtR
{

    [ServiceBehavior]
    public class ColocAtR : IColocAtR
    {

        DataClassesDataContext data = new DataClassesDataContext();
        RemoteEndpointMessageProperty endpoint = (RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];

        #region Auth

        public WSAuthMessage CreateAccount(string login, string email, string password, string firstName, string lastName)
        {
            WSAuthMessage Response = new WSAuthMessage();

            // TODO : Creation compte

            Response.StatusCode = StatusCode.OK;
            Response.Data = "Code de retour";

            return Response;
        }

        public WSAuthMessage SignIn(string email, string password)
        {
            WSAuthMessage Response = new WSAuthMessage();

            // Vérifie que le couple email/password correspond en base de données
            var usersList = (from users in data.Users
                             where users.emailUser == email && users.passwordUser == Utils.ToSha256(password)
                             select users);

            if (usersList.Count() != 1)
            {
                Response.StatusCode = StatusCode.AccessRefused;
                Response.Data = "Couple email / mot de passe non valide";
                return Response;
            }

            var user = usersList.First();

            try
            {

                //   Créer un enregistrement de AuthToken
                AuthToken newSession = new AuthToken();
                newSession.token = Utils.GetUniqueKey();
                newSession.idUser = user.idUser;
                newSession.ipAddress = endpoint.Address;
                newSession.lastActivity = DateTime.Now;
                newSession.fullyAuthAndOnline = true;

                data.AuthTokens.InsertOnSubmit(newSession);
                data.SubmitChanges();

                Response.StatusCode = StatusCode.OK;
                Response.Data = newSession.token;
            }
            catch
            {
                Response.StatusCode = StatusCode.Error;
                Response.Data = "Impossible de mettre à jour la base de données";
                return Response;
            }

            return Response;
        }

        public WSAuthMessage SignOut(string token)
        {
            WSAuthMessage Response = new WSAuthMessage();

            var session = GetSession(token);

            if (session == null)
            {
                Response.StatusCode = StatusCode.AccessRefused;
                return Response;
            }

            if (session.ipAddress == endpoint.Address)
            {
                data.AuthTokens.DeleteOnSubmit(session);
            }
            else
            {
                Response.StatusCode = StatusCode.AccessRefused;
                Response.Data = "Adresse IP différente";
            }

            try
            {
                data.SubmitChanges();
            }
            catch
            {
                Response.StatusCode = StatusCode.Error;
                Response.Data = "Impossible de mettre à jour la base de données";
                return Response;
            }

            Response.StatusCode = StatusCode.OK;
            Response.Data = "Deconnecté";

            return Response;
        }

        public WSAuthMessage RefreshLastActivity(string token)
        {

            WSAuthMessage Response = new WSAuthMessage();

            AuthToken session = GetSession(token);
            if (session == null)
            {
                Response.StatusCode = StatusCode.AccessRefused;
                return Response;
            }

            else if (!CheckIfOnline(token))
            {
                SignOut(token);
                Response.StatusCode = StatusCode.AccessRefused;
                return Response;
            }

            session.lastActivity = DateTime.Now;
            try
            {
                data.SubmitChanges();
            }
            catch
            {
                Response.StatusCode = StatusCode.Error;
                Response.Data = "Impossible de mettre à jour la base de données";
                return Response;
            }

            Response.StatusCode = StatusCode.OK;
            Response.Data = "Date de dernière activité rafraichie";
            return Response;

        }
        bool CheckIfOnline(string token)
        {
            bool online = false;

            AuthToken session = GetSession(token);
            if (session != null)
            {
                if (session.lastActivity.AddMinutes(int.Parse(WSColocAtR.Properties.Resources.SessionTimeout)) > DateTime.Now)
                {
                    if (session.ipAddress == endpoint.Address)
                    {
                        online = true;
                    }
                }
            }
            return online;
        }

        AuthToken GetSession(string token)
        {
            var session = (from sess in data.AuthTokens
                           where sess.token == token
                           select sess);
            if (session.Count() != 1)
            {
                return null;
            }
            return session.First();
        }

        public WSAuthMessage Whoami(string token)
        {
            WSAuthMessage Response = new WSAuthMessage();


            if (RefreshLastActivity(token).StatusCode == StatusCode.OK)
            {
                var session = GetSession(token);

                var userLogin = (from users in data.Users
                                 where users.idUser == session.idUser
                                 select users.loginUser).FirstOrDefault();

                Response.StatusCode = StatusCode.OK;
                Response.Data = userLogin == null ? "inconnu" : userLogin;
            }

            return Response;
        }

        #endregion

        #region ProfilManager

        public WSAuthMessage ProfilWizard(string token, bool type, int age, int price, string city, string desc, int m2 = 0)
        {
            WSAuthMessage Response = new WSAuthMessage();

            if (RefreshLastActivity(token).StatusCode == StatusCode.OK)
            {
                var session = GetSession(token);

                // TODO : Remplissage du profil

                Response.StatusCode = StatusCode.OK;
                Response.Data = "Code de retour";
            }
            return Response;
        }

        public WSAuthMessage ChangePassword(string token, string oldPassword, string newPassword)
        {
            WSAuthMessage Response = new WSAuthMessage();

            if (RefreshLastActivity(token).StatusCode == StatusCode.OK)
            {
                var session = GetSession(token);

                // TODO : Changement du mot de passe

                Response.StatusCode = StatusCode.OK;
                Response.Data = "Code de retour";
            }
            return Response;
        }

        #endregion

        public WSAuthMessage GetScoringResults(string token)
        {
            WSAuthMessage Response = new WSAuthMessage();

            if (RefreshLastActivity(token).StatusCode == StatusCode.OK)
            {
                var session = GetSession(token);

                // TODO : Check Scoring

                Response.StatusCode = StatusCode.OK;
                Response.Data = "Code de retour";
            }
            return Response;
        }

    }
}