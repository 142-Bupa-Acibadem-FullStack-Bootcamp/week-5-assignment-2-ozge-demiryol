using Microsoft.EntityFrameworkCore;
using Northwind.Dal.Abstract;
using Northwind.Entity.Models;
using System.Linq;


namespace Northwind.Dal.Concrete.Entityframework.Repository
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(DbContext context) : base(context)
        {

        }

        public User Login(User login)
        {
            //dbset base'ten geliyor
            //var user = dbset.Where(x => x.UserCode == login.UserCode && x.Password == login.Password).SingleOrDefault();
            var user = dbset.FirstOrDefault(x => x.UserCode == login.UserCode && x.Password == login.Password);

            //var user = dbset.SingleOrDefault(x => x.UserCode == login.UserCode && x.Password == login.Password);
            return user;
        }
        public User Register(User register)
        {
            context.Entry(register).State=EntityState.Added;
            dbset.Add(register);
            return register;
        }
    }
}
