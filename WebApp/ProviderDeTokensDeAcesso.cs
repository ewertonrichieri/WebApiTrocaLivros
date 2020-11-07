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
            if (context.Request.Method == "POST" && context.UserName != null && context.Password != null)
            {
                Response auth = UsuarioController.AutenticarUsuario(context.UserName, context.Password);

                if (auth.Code == 200)
                {
                    var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                    identity.AddClaim(new Claim("sub", context.UserName));
                    identity.AddClaim(new Claim(ClaimTypes.Role, auth.TypeAccount));

                    identity.AddClaim(new Claim(ClaimTypes.Email, auth.Email));
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, auth.ID));
                    identity.AddClaim(new Claim(ClaimTypes.StreetAddress, auth.Endereco));
                    identity.AddClaim(new Claim(ClaimTypes.Locality, auth.LatitudeLongitude));
                    identity.AddClaim(new Claim(ClaimTypes.MobilePhone, auth.Celular));
                    //identity.AddClaim(new Claim("role", "user"));
                    
                    context.Validated(identity);
                }
                else
                {
                    context.SetError("acesso invalido", auth.Msg);
                    return;
                }
            }
            else
            {
                context.SetError("Operação Inválida");
                return;
            }
        }
    }
}