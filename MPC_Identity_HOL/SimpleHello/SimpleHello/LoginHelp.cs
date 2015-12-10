using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace SimpleHello
{
    class LoginHelp
    {
        private Account activeAccount;

        public LoginHelp(Account account)
        {
            this.activeAccount = account;
        }

        /// <summary>
        /// The function called when the user wants to sign in with Passport but from
        /// the username/password dialog.
        ///
        /// First we check if they already use Passport as their primary login method
        ///
        /// Otherwise we authenticate their username/password and then begin the Passport
        /// enrollment process which consists of creating a Passport key and then sending that to
        /// the authentication server.
        /// </summary>
        public async Task<bool> SignInPassport()
        {
            if (await AuthenticatePassport() == true)
            {
                SuccessfulSignIn(false);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Username and password authentication
        /// </summary>
        /// <param name="isnewAccount">true:new user,false:old user</param>
        /// <returns></returns>
        public async Task<bool> SignInPassword(bool isnewAccount)
        {
            try
            {
                bool signedIn = await AuthenticatePasswordCredentials();

                if (signedIn == false)
                {
                    return false;
                }
                else
                {
                    if (isnewAccount)
                        SuccessfulSignIn(true);
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Authenticate user credentials with server and return result.
        /// </summary>
        /// <returns>Boolean representing if authenticating the user credentials with the server succeeded</returns>
        private async Task<bool> AuthenticatePasswordCredentials()
        {
            // TODO: Authenticate with server once that part is done for the sample.

            return true;
        }

        /// <summary>
        /// Function to be called when we need to register our public key with
        /// the server for Microsoft Passport
        /// </summary>
        /// <returns>Boolean representing if adding the Passport login method to this account on the server succeeded</returns>
        public async Task<bool> AddPassportToAccountOnServer()
        {
            // TODO: Add Passport signing info to server when that part is done for the sample

            return true;
        }

        /// <summary>
        /// Attempts to get the authentication message from the Passport key for this account.
        ///
        /// This will bring up the Passport PIN dialog and prompt the user for their PIN.
        /// 
        /// The authentication message will be null if signing fails.
        /// </summary>
        /// <returns>Boolean representing if authenticating Passport succeeded</returns>
        private async Task<bool> AuthenticatePassport()
        {
            IBuffer message = CryptographicBuffer.ConvertStringToBinary("LoginAuth", BinaryStringEncoding.Utf8);
            IBuffer authMessage = await GetPassportAuthenticationMessage(message, this.activeAccount.Email);

            if (authMessage != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles user saving for our list of users if this is a new user
        /// </summary>
        private void SuccessfulSignIn(bool isAdd)
        {
            // If this is an already existing account, replace the old
            // version of this account in the account list.
            if (isAdd == false)
            {
                foreach (Account a in UserSelect.accountList)
                {
                    if (a.Email == this.activeAccount.Email)
                    {
                        UserSelect.accountList.Remove(a);
                        break;
                    }
                }
            }
            UserSelect.accountList.Add(this.activeAccount);
            AccountsHelper.SaveAccountList(UserSelect.accountList);
        }

        /// <summary>
        /// Attempts to sign a message using the Passport key on the system for the accountId passed.
        /// </summary>
        /// <param name="message">The message to be signed</param>
        /// <param name="accountId">The account id for the Passport key we are using to sign</param>
        /// <returns>Boolean representing if creating the Passport authentication message succeeded</returns>
        private async Task<IBuffer> GetPassportAuthenticationMessage(IBuffer message, string accountId)
        {
            KeyCredentialRetrievalResult openKeyResult = await KeyCredentialManager.OpenAsync(accountId);

            if (openKeyResult.Status == KeyCredentialStatus.Success)
            {
                KeyCredential userKey = openKeyResult.Credential;
                IBuffer publicKey = userKey.RetrievePublicKey();

                KeyCredentialOperationResult signResult = await userKey.RequestSignAsync(message);

                if (signResult.Status == KeyCredentialStatus.Success)
                {
                    return signResult.Result;
                }
                else if (signResult.Status == KeyCredentialStatus.UserCanceled)
                {
                    // User cancelled the Passport PIN entry.
                    //
                    // We will return null below this and the username/password
                    // sign in form will show.
                }
                else if (signResult.Status == KeyCredentialStatus.NotFound)
                {
                    // Must recreate Passport key
                }
                else if (signResult.Status == KeyCredentialStatus.SecurityDeviceLocked)
                {
                    // Can't use Passport right now, remember that hardware failed and suggest restart
                }
                else if (signResult.Status == KeyCredentialStatus.UnknownError)
                {
                    // Can't use Passport right now, try again later
                }

                return null;
            }
            else if (openKeyResult.Status == KeyCredentialStatus.NotFound)
            {
                // Passport key lost, need to recreate it
            }
            else
            {
                // Can't use Passport right now, try again later
            }
            return null;
        }

        /// <summary>
        /// Creates a Passport key on the machine using the account id passed.
        /// Then returns a boolean based on whether we were able to create a Passport key or not.
        ///
        /// Will also attempt to create an attestation that this key is backed by hardware on the device, but is not a requirement
        /// for a working key in this scenario. It is possible to not accept a key that is software-based only.
        /// </summary>
        /// <param name="accountId">The account id associated with the account that we are enrolling into Passport</param>
        /// <returns>Boolean representing if creating the Passport key succeeded</returns>
        public async Task<bool> CreatePassportKey(string accountId)
        {
            KeyCredentialRetrievalResult keyCreationResult = await KeyCredentialManager.RequestCreateAsync(accountId, KeyCredentialCreationOption.ReplaceExisting);

            if (keyCreationResult.Status == KeyCredentialStatus.Success)
            {
                KeyCredential userKey = keyCreationResult.Credential;
                IBuffer publicKey = userKey.RetrievePublicKey();
                KeyCredentialAttestationResult keyAttestationResult = await userKey.GetAttestationAsync();

                if (keyAttestationResult.Status == KeyCredentialAttestationStatus.Success)
                {
                    //keyAttestation Included.
                    //TODO:read  keyAttestationResult.AttestationBuffer and keyAttestationResult.CertificateChainBuffer
                }
                else if (keyAttestationResult.Status == KeyCredentialAttestationStatus.TemporaryFailure)
                {
                    //keyAttestation CanBeRetrievedLater
                }
                else if (keyAttestationResult.Status == KeyCredentialAttestationStatus.NotSupported)
                {
                    //keyAttestation is not supported
                }

                // Package public key, keyAttesation if available, 
                // certificate chain for attestation endorsement key if available,  
                // status code of key attestation result: keyAttestationIncluded or 
                // keyAttestationCanBeRetrievedLater and keyAttestationRetryType
                // and send it to application server to register the user.
                bool serverAddedPassportToAccount = await AddPassportToAccountOnServer();

                if (serverAddedPassportToAccount == true)
                {
                    return true;
                }
            }
            else if (keyCreationResult.Status == KeyCredentialStatus.UserCanceled)
            {
                // User cancelled the Passport enrollment process
            }
            else if (keyCreationResult.Status == KeyCredentialStatus.NotFound)
            {
                // User needs to create PIN
                return false;
            }
            return false;
        }

    }
}
