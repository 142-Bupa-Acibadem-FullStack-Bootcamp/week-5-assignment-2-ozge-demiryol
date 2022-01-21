using Northwind.Bll.Base;
using Northwind.Dal.Abstract;
using Northwind.Entity.Dto;
using Northwind.Entity.Models;
using Northwind.Interface;
using Microsoft.Extensions.DependencyInjection;
using System;
using Northwind.Entity.IBase;
using Northwind.Entity.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace Northwind.Bll
{
    //API katmaninda singleton olarak kullanacagimiz icin public
    public class UserManager : BllBase<User, DtoUser>, IUserService
    {
        public readonly IUserRepository userRepository;
        IConfiguration configuration;

        public UserManager(IServiceProvider service, IConfiguration configuration) : base (service)
        {
            userRepository = service.GetService<IUserRepository>();
            this.configuration = configuration;
        }
        public IResponse<DtoUserToken> Login(DtoLogin login)
        {
            //Kullanicinin girdigi parola sifrelenir ve veri tabaninda parolanin sifrelenmis haliyle karsilastirilir
            var user_login = new DtoLogin
            {
                UserCode= login.UserCode,
                Password = MD5Hash(login.Password)
            };

            var user = userRepository.Login(ObjectMapper.Mapper.Map<User>(user_login));

            if (user != null)
            {
                var dtoUser = ObjectMapper.Mapper.Map<DtoLoginUser>(user);
                var token = new TokenManager(configuration).CreateAccessToken(dtoUser);

                var userToken = new DtoUserToken()
                {
                    DtoLoginUser = dtoUser,
                    AccessToken = token
                };

                return new Response<DtoUserToken>
                {
                    Message = "Token is created",
                    StatusCode = StatusCodes.Status200OK,
                    Data = userToken
                };
            } 
            else
            {
                return new Response<DtoUserToken>()
                {
                    Message = "User code or password is wrong" ,
                    StatusCode = StatusCodes.Status406NotAcceptable,
                    Data = null
                };
            }
        }

        public IResponse<DtoUser> Register(DtoUser register, bool saveChanges = true)
        {
           try
           {
              //Kullanicinin parolasi sifrelenecegi icin DtoUser'in instance'i olusturulur ve parola haricindeki kullanicinin girdigi
              //tum veriler aynen alinir, parola MD5 metoduyla sifrelenerek kaydedilir.
              var encryptedUser = new DtoUser
              {
                  UserName = register.UserName,
                  UserLastName = register.UserLastName,
                  UserCode = register.UserCode,
                  Password = MD5Hash(register.Password),
              };

              var resolvedResult = "";
              var TResult = userRepository.Register(ObjectMapper.Mapper.Map<User>(encryptedUser));//Parolasi sifrelenmis kullaniciyi veri tabanindaki modele cevirir.
              resolvedResult = String.Join(',', TResult.GetType().GetProperties().Select(x => $" - {x.Name} : {x.GetValue(TResult) ?? ""} - "));
              if (saveChanges)
                Save();

             return new Response<DtoUser>
             {
                 StatusCode = StatusCodes.Status200OK,
                 Message = "User has been registered successfully",
                 Data = ObjectMapper.Mapper.Map<User, DtoUser>(TResult)
             };
           }
           catch (Exception ex)
           {
                return new Response<DtoUser>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        //MD5 sifreleme metodu
        public string MD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.ASCII.GetBytes(input));
                var strResult = BitConverter.ToString(result);
                return strResult.Replace("-", "");
            }
        }
    }
}
