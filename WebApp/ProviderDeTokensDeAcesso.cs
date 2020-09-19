using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Controllers;
using WebApp.Models;

namespace WebApp
{
    public class ProviderDeTokensDeAcesso : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            if (context.Request.Method == "POST")
            {
                MensagemResult auth = LivroController.AutenticarUsuario(context.UserName, context.Password);

                if (auth.code == 200)
                {
                    var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                    identity.AddClaim(new Claim("sub", context.UserName));
                    identity.AddClaim(new Claim("role", "user"));

                    context.Validated(identity);
                }
                else
                {
                    context.SetError("acesso invalido", auth.msg);
                    return;
                }
            }
            else
            {
                context.SetError("Metodo Inválido");
                return;
            }
        }
    }
}