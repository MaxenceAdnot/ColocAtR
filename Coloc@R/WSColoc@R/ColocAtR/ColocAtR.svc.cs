using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Collections;

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

            if( String.IsNullOrEmpty(login) 
                || String.IsNullOrEmpty(email) 
                || String.IsNullOrEmpty(password) 
                || String.IsNullOrEmpty(firstName) 
                || String.IsNullOrEmpty(lastName)
                )
            {
                Response.StatusCode = StatusCode.Error;
                Response.Data = "Un des champs n'est pas renseigné.";
                return Response;
            }

            // on vérifie que le mail ne soit pas déjà utilisé
            var mailList = (from users in data.Users
                             where users.emailUser == email
                             select users);

            if (mailList.Count() != 0)
            {
                Response.StatusCode = StatusCode.Error;
                Response.Data = "Cette adresse mail est déjà utilisée.";
                return Response;
            }

            // on vérifie que le login ne soit pas déjà utilisé
            var loginList = (from users in data.Users
                                where users.loginUser == login
                                select users);

            if (loginList.Count() != 0 )
            {
                Response.StatusCode = StatusCode.Error;
                Response.Data = "Ce nom d'utilisateur est déjà utilisé.";
                return Response;
            }

            try
            {

                //   Créer un enregistrement de User
                var newUser = new User();
                newUser.loginUser = login;
                newUser.passwordUser = Utils.ToSha256(password);
                newUser.emailUser = email;
                newUser.firstNameUser = firstName;
                newUser.lastNameUser = lastName;

                //default values
                newUser.city = 19895;
                newUser.age = 0;
                newUser.type = false; 
                newUser.priceColoc = 0;
                newUser.descUser = "Je suis intéressé par la colocation";
                newUser.m2Coloc = 0;

                data.Users.InsertOnSubmit(newUser);
                data.SubmitChanges();

                Response.StatusCode = StatusCode.OK;
                Response.Data = newUser.loginUser;
            }
            catch(Exception e)
            {
                Response.StatusCode = StatusCode.Error;
                Response.Data = e.Message;
                return Response;
            }

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
            // Vérifie si un token n'a pas déjà été généré pour l'utilisateur
            var sessionDB = (from sessions in data.AuthTokens
                             where sessions.idUser == usersList.First().idUser
                             select sessions);

            if (sessionDB.Count() == 1) // Session existante
            {
                var session = sessionDB.First();
                if (session.lastActivity.AddMinutes(int.Parse(WSColocAtR.Properties.Resources.SessionTimeout)) > DateTime.Now)
                {
                    session.fullyAuthAndOnline = true; // Evite la compromission des sessions connectées
                    try
                    {
                        data.SubmitChanges();
                    }
                    catch
                    {
                        Response.StatusCode = StatusCode.Error;
                        Response.Data = "Impossible de démarrer une nouvelle session";
                        return Response;
                    }
                    Response.StatusCode = StatusCode.OK;
                    Response.Data = session.token;
                }
                else
                {
                    SignOut(session.token);
                    return SignIn(email,password);
                }
            }
            else
            {
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

                var user = (from p in data.Users
                            where p.idUser == session.idUser
                            select p).First();

                user.type = type;
                user.age = age;
                user.priceColoc = price;
                user.city = (from cityNum in data.Cities
                                     where cityNum.libelleCity == city
                                     select cityNum.idCity).FirstOrDefault();
                user.descUser = desc;
                user.m2Coloc = m2;

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
                Response.Data = "Profil mis à jour";
            }
            return Response;
        }

        public WSAuthMessage ChangePassword(string token, string oldPassword, string newPassword)
        {
            WSAuthMessage Response = new WSAuthMessage();

            if (RefreshLastActivity(token).StatusCode == StatusCode.OK)
            {
                var session = GetSession(token);

                if (session.User.passwordUser == Utils.ToSha256(oldPassword))
                {
                    session.User.passwordUser = Utils.ToSha256(newPassword);
                    Response.StatusCode = StatusCode.OK;
                    Response.Data = "Mot de passe mis à jour";
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
                }
                else
                {
                    Response.StatusCode = StatusCode.AccessRefused;
                    Response.Data = "Ancien mot de passe incorrect";
                }
            }
            return Response;
        }

        #endregion
        
        public WSProfile[] GetScoringResults(string token)
        {

            if (RefreshLastActivity(token).StatusCode == StatusCode.OK)
            {
                var session = GetSession(token);

                var user = (from p in data.Users
                            where p.idUser == session.idUser
                            select p).First();

                ArrayList matched = new ArrayList();

                // Perfect match
                var perfectMatch = (from p in data.Users
                                    where p.type == !user.type
                                     && p.city == user.city
                                     && p.age == user.age
                                     && p.priceColoc == user.priceColoc
                                    select p);

                foreach (User u in perfectMatch)
                {
                    matched.Add(ProfileDBToWSProfile(u));
                }

                // Unperfect match
                var unperfectMatch = (from p in data.Users
                                      where p.type == !user.type
                                       && p.city == user.city
                                       && p.age <= user.age + 5
                                       && p.age >= user.age - 5
                                       && p.priceColoc <= user.priceColoc + 100
                                       && p.priceColoc >= user.priceColoc - 100
                                      select p);

                foreach (User u in unperfectMatch)
                {
                    var p = ProfileDBToWSProfile(u);
                    if (!matched.Contains(p))
                        matched.Add(p);
                }

                return (WSProfile[])matched.ToArray(typeof(WSProfile));
            }

            return null;
        }


        public WSProfile RetrieveProfileUN(string token, string username)
        {
            WSProfile profile = new WSProfile();

            if (RefreshLastActivity(token).StatusCode == StatusCode.OK)
            {
                var profileDB = (from p in data.Users
                                 where p.loginUser == username
                                 select p).First();

                if (profileDB != null)
                {
                    profile = ProfileDBToWSProfile(profileDB);
                }
            }

            return profile;
        }

        public WSProfile RetrieveProfileUID(string token, int userID)
        {
            WSProfile profile = new WSProfile();

            if (RefreshLastActivity(token).StatusCode == StatusCode.OK)
            {
                var profileDB = (from p in data.Users
                                 where p.idUser == userID
                                 select p).FirstOrDefault();

                if (profileDB != null)
                {
                    profile = ProfileDBToWSProfile(profileDB);
                }
            }

            return profile;
        }
        
        private WSProfile ProfileDBToWSProfile(User profileDB)
        {
            WSProfile profile = new WSProfile();

            profile.username = profileDB.loginUser;
            profile.firstName = profileDB.firstNameUser;
            profile.lastName = profileDB.lastNameUser;
            profile.type = profileDB.type;
            profile.age = profileDB.age;
            profile.city = (from c in data.Cities
                            where c.idCity == profileDB.city
                            select c.libelleCity).FirstOrDefault().ToLower();
            profile.desc = profileDB.descUser;
            profile.price = profileDB.priceColoc;
            profile.m2 = (int)profileDB.m2Coloc;
            profile.email = profileDB.emailUser;

            return profile;
        }

    }
}